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
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.ModApi
{
	public class ModApiPaymentHostedService : IHostedService, IDisposable
	{
		#region Типы данных

		private enum ConvertResult
		{
			Success,
			Fail,
			NoNeedToConvert,
			Duplicate,
			CantConvert
		}

		#endregion

		#region Поля

		private readonly ILogger _logger;
		private CancellationTokenSource _cts;
		private SqlServer _sqlServer;
		private Share.ShareService _shareService;
		private ModApiConfiguration _config;

		private List<IPaymentConverter> _paymentConverters;

		#endregion

		#region Конструктор, запуск и остановка

		public ModApiPaymentHostedService(ILogger<ModApiPaymentHostedService> logger, SqlServer sqlServer, IConfiguration config, Share.ShareService shareService)
		{
			this._logger = logger;
			this._sqlServer = sqlServer;

			this._cts = new CancellationTokenSource();

			this._config = Startup.Config.ModApi;

			this._shareService = shareService;

			this._paymentConverters = new List<IPaymentConverter>();
			this._paymentConverters.Add(new TinkoffConverter());
			this._paymentConverters.Add(new RaiffeisenConverter());
			this._paymentConverters.Add(new SberbankConverter());
			this._paymentConverters.Add(new SbAcqConverter());
			this._paymentConverters.Add(new SbRegConverter());
			//this._paymentConverters.Add(new YksRegConverter());
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("ModApiPaymentHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("ModApiPaymentHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "ModApiPaymentHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("ModApiPaymentHostedService disposed");
			_cts.Cancel();
		}

		#endregion

		#region Цикл обработки запросов

		private async Task DoWork()
		{
			try
			{
				_logger.LogInformation("ModApiPaymentHostedService is working.");
				bool firstTime = true;
				while (!this._cts.IsCancellationRequested)
				{
					await ProcessCycle(firstTime);
					firstTime = false;
				}
			}
			finally
			{
				_logger.LogInformation("ModApiPaymentHostedService is completing work.");
			}
		}

		private async Task ProcessCycle(bool firstTime)
		{
			try
			{
				TimeSpan interval = TimeSpan.FromSeconds(10);
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
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "ModApiPaymentHostedService.ProcessCycle: " + exStr, _logger);
			}
		}

		#endregion

		private async Task ProcessPayments()
		{
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				while (true)
				{
					string paymentJson = null;
					using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.GetModApiPaymentToProcess", conn))
					{
						using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
						{
							if (dr.Read())
							{
								paymentJson = dr.GetString("RawData");
							}
						}
					}

					if (paymentJson == null)
					{
						break;
					}

					OperationHistoryRequest.Operation o = JsonConvert.DeserializeObject<OperationHistoryRequest.Operation>(paymentJson);
					using (SqlTransaction tx = conn.BeginTransaction())
					{
						bool result = await this.ConvertPayment(o, paymentJson, conn, tx);

						string setCommand = (result ? "payment.SetModApiPaymentAsProcessed" : "payment.SetModApiPaymentAsInvalid");
						using (SqlCommand cmd = _sqlServer.GetSpCommand(setCommand, conn, tx))
						{
							cmd.AddUniqueIdentifierParam("@PaymentId", o.id);
							await cmd.ExecuteNonQueryAsync();
						}

						tx.Commit();
					}
				}
			}
		}

		private async Task<bool> ConvertPayment(OperationHistoryRequest.Operation o, string paymentJson, SqlConnection conn, SqlTransaction tx)
		{
			Payment payment;
			Exception exception;
			ConvertResult result;
			(result, payment, exception) = await this.ConvertPayment2(o, paymentJson, conn, tx);

			DiagOptions options = DiagOptions.Tech;
			string message;
			if (result == ConvertResult.CantConvert)
			{
				message = "ModApi: Конвертер не найден: " + paymentJson;
			}
			else if (result == ConvertResult.NoNeedToConvert)
			{
				message = "ModApi: Платёж не подлежит обработке: " + paymentJson;
			}
			else if (result == ConvertResult.Fail)
			{
				message = "ModApi: Ошибка \"" + exception.Message + "\" при проведении платежа: " + paymentJson;
			}
			else if (result == ConvertResult.Duplicate)
			{
				message = "ModApi: Платёж уже обработан: " + paymentJson;
			}
			else
			{
				options = DiagOptions.Biz | DiagOptions.Tech;
				if (payment.Status == PaymentStatus.Approved)
				{
					message = "ModApi: Платёж " + payment.Id.ToString() + " ожидает подтверждения: " + paymentJson;
				}
				else
				{
					message = "ModApi: Платёж " + payment.Id.ToString() + " успешно проведён: " + paymentJson;
				}
			}

			Helper.SaveDiagMessage(this._sqlServer, options, message, this._logger);

			return result.IsAny(ConvertResult.Success, ConvertResult.NoNeedToConvert, ConvertResult.Duplicate);
		}

		private async Task<(ConvertResult, Payment, Exception)> ConvertPayment2(OperationHistoryRequest.Operation o, string paymentJson, SqlConnection conn, SqlTransaction tx)
		{
			Payment payment = null;
			Exception exception = null;
			IPaymentConverter converter = null;
			for (int i = 0; i < this._paymentConverters.Count; i++)
			{
				IPaymentConverter pc = this._paymentConverters[i];
				if (pc.CanConvert(o))
				{
					converter = pc;
					break;
				}
			}

			if (converter == null)
			{
				return (ConvertResult.CantConvert, payment, exception);
			}

			if (!converter.NeedCreatePayment)
			{
				return (ConvertResult.NoNeedToConvert, payment, exception);
			}

			try
			{
				payment = converter.Convert(o, paymentJson);

				if (!await this._shareService.ProceedPayment(payment, conn, tx))
				{
					return (ConvertResult.Duplicate, payment, exception);
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "ModApiPaymentHostedService.ConvertPayment2: " + exStr, _logger);
				return (ConvertResult.Fail, payment, exception);
			}

			return (ConvertResult.Success, payment, exception);
		}
	}
}
