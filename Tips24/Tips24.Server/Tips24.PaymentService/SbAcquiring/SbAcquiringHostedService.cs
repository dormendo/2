using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;

namespace Tips24.PaymentService.SbAcquiring
{
	public class SbAcquiringHostedService : IHostedService, IDisposable
	{
		#region Типы данных

		private enum CreatePaymentResult
		{
			Success,
			Fail,
			Duplicate
		}

		#endregion

		#region Поля

		private readonly ILogger _logger;
		private CancellationTokenSource _cts;
		private SqlServer _sqlServer;
		private Share.ShareService _shareService;
		private SbAcquiringConfiguration _config;

		#endregion

		#region Конструктор, запуск и остановка

		public SbAcquiringHostedService(ILogger<SbAcquiringHostedService> logger, SqlServer sqlServer, IConfiguration config, Share.ShareService shareService)
		{
			this._logger = logger;
			this._sqlServer = sqlServer;

			this._cts = new CancellationTokenSource();
			this._config = Startup.Config.SbAcquiring;
			this._shareService = shareService;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("SbAcquiringHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("SbAcquiringHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "SbAcquiringHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("SbAcquiringHostedService disposed");
			_cts.Cancel();
		}

		#endregion

		#region Цикл обработки запросов

		private async Task DoWork()
		{
			try
			{
				_logger.LogInformation("SbAcquiringHostedService is working.");
				bool firstTime = true;
				while (!this._cts.IsCancellationRequested)
				{
					await ProcessCycle(firstTime);
					firstTime = false;
				}
			}
			finally
			{
				_logger.LogInformation("SbAcquiringHostedService is completing work.");
			}
		}

		private async Task ProcessCycle(bool firstTime)
		{
			try
			{
				TimeSpan interval = TimeSpan.FromSeconds(1);
				TimeSpan delay = (firstTime ? TimeSpan.Zero : interval);
				while (!_cts.IsCancellationRequested)
				{
					if (delay > TimeSpan.Zero)
					{
						_logger.LogTrace(delay.ToString());
						await Task.Delay(delay, _cts.Token);
						if (_cts.IsCancellationRequested)
						{
							break;
						}
					}

					DateTime startTimeUtc = DateTime.UtcNow;
					await ProcessPayments();

					if (_cts.IsCancellationRequested)
					{
						break;
					}

					DateTime endTimeUtc = DateTime.UtcNow;
					TimeSpan timeElapsed = endTimeUtc - startTimeUtc;
					delay = (timeElapsed > interval ? TimeSpan.Zero : interval - timeElapsed);
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "SbAcquiringHostedService.ProcessCycle: " + exStr, _logger);
			}
		}

		#endregion

		private async Task ProcessPayments()
		{
			List<PaymentRequest> requestList = new List<PaymentRequest>();
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.SbAcquiring_GetRequests", conn))
				{
					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						while (dr.Read())
						{
							PaymentRequest request = new PaymentRequest();
							request.FillFromReader(dr);
							requestList.Add(request);
						}
					}
				}

				foreach (PaymentRequest request in requestList)
				{
					await this.ProcessRequest(request, conn);
				}
			}
		}

		private async Task ProcessRequest(PaymentRequest request, SqlConnection conn)
		{
			CreatePaymentResult result;
			Share.Payment payment = null;
			Exception exception = null;

			using (SqlTransaction tx = conn.BeginTransaction())
			{
				(result, payment, exception) = await this.CreatePayment(request, conn, tx);

				if (result == CreatePaymentResult.Success)
				{
					await CompleteRequest(request, conn, tx);
				}
				else if (result == CreatePaymentResult.Fail)
				{
					await FailedToCompleteRequest(request, conn, tx);
				}

				tx.Commit();
			}

			DiagOptions o = DiagOptions.Tech;
			string message;
			if (result == CreatePaymentResult.Success)
			{
				o = DiagOptions.Tech | DiagOptions.Biz;
				message = "SbAcq: Платёж " + payment.Id.ToString() + (payment.Status == Share.PaymentStatus.Approved ? " успешно проведён" : " ожидает подтверждения") + ": " + payment.RawData;
			}
			else if (result == CreatePaymentResult.Duplicate)
			{
				message = "SbAcq: Платёж уже обработан: " + payment.RawData;
			}
			else
			{
				message = "SbAcq: Ошибка \"" + exception.Message + "\" при проведении платежа: " + payment.RawData;
			}

			Helper.SaveDiagMessage(_sqlServer, o, message, _logger);
		}

		private async Task CompleteRequest(PaymentRequest request, SqlConnection conn, SqlTransaction tx)
		{
			try
			{
				using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.SbAcquiring_CompleteRequest", conn, tx))
				{
					cmd.AddIntParam("@RequestId", request.RequestId);
					cmd.AddVarCharParam("@OrderId", 50, request.OrderId);
					cmd.AddBigIntParam("@PaymentId", request.PaymentId);

					await cmd.ExecuteNonQueryAsync();
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "SbAcquiringHostedService.CompleteRequest: " + exStr, _logger);
			}
		}

		private async Task FailedToCompleteRequest(PaymentRequest request, SqlConnection conn, SqlTransaction tx)
		{
			try
			{
				using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.SbAcquiring_FailedToCompleteRequest", conn, tx))
				{
					cmd.AddIntParam("@RequestId", request.RequestId);
					cmd.AddVarCharParam("@OrderId", 50, request.OrderId);

					await cmd.ExecuteNonQueryAsync();
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "SbAcquiringHostedService.FailedToCompleteRequest: " + exStr, _logger);
			}
		}

		private async Task<(CreatePaymentResult, Share.Payment, Exception)> CreatePayment(PaymentRequest request, SqlConnection conn, SqlTransaction tx)
		{
			Share.Payment payment = new Share.Payment();

			try
			{
				payment.Id = 0;
				payment.Status = Share.PaymentStatus.Approved;
				//payment.DocumentName = response.Response.Id;
				//payment.DocumentNumber = response.Response.Id;
				//payment.DocumentDate = DateTime.ParseExact(response.Response.CapturedAt, "O", CultureInfo.InvariantCulture).Date;

				payment.ExternalId = request.OrderId;
				payment.DataSource = "SBBACQ";
				payment.Provider = request.PaymentProvider;
				payment.OriginalAmount = request.Amount;
				payment.ReceivedAmount = decimal.Floor(request.Amount * 98M) / 100M;
				payment.PaymentDateTime = request.CreateDateTime;
				payment.IsTimeSpecified = true;
				payment.ArrivalDateTime = DateTime.Now;
				payment.Fio = "";
				payment.Address = "";
				payment.Purpose = "";
				payment.PlaceId = request.PlaceId;
				payment.EmployeeId = request.EmployeeId;
				payment.RawData = JsonConvert.SerializeObject(request);

				if (await this._shareService.ProceedPayment(payment, conn, tx))
				{
					request.PaymentId = payment.Id;
					return (CreatePaymentResult.Success, payment, null);
				}
				else
				{
					return (CreatePaymentResult.Duplicate, payment, null);
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "SbAcquiringHostedService.CreatePayment: " + exStr, _logger);
				return (CreatePaymentResult.Fail, payment, ex);
			}
		}
	}
}
