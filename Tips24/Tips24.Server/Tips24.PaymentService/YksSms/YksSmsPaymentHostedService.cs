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

namespace Tips24.PaymentService.YksSms
{
	public class YksSmsPaymentHostedService : IHostedService, IDisposable
	{
		#region Поля

		public static YksSmsPaymentHostedService Instance { get; private set; }

		private readonly ILogger _logger;
		private CancellationTokenSource _cts;
		private SqlServer _sqlServer;
		private Share.ShareService _shareService;
		private YksSmsConfiguration _config;
		private Logger _fileLogger;
		private HttpClient _client;

		#endregion

		#region Конструктор, запуск и остановка

		public YksSmsPaymentHostedService(ILogger<YksSmsPaymentHostedService> logger, SqlServer sqlServer, IConfiguration config, Share.ShareService shareService)
		{
			this._logger = logger;
			this._sqlServer = sqlServer;

			this._cts = new CancellationTokenSource();

			this._config = Startup.Config.YksSms;
			this._fileLogger = new Logger(this._config.LogFolder);

			this._shareService = shareService;
			Instance = this;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("YksSmsPaymentHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("YksSmsPaymentHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "SbRegHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("YksSmsPaymentHostedService disposed");
			_cts.Cancel();
			this.CloseClient();
		}

		private void CreateClient()
		{
			this.CloseClient();

			this._client = new HttpClient();
			string authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._config.ShopId + ":" + this._config.SecretKey));
			this._client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);
			this._client.BaseAddress = new Uri(this._config.ApiUrl);
		}

		private void CloseClient()
		{
			if (this._client != null)
			{
				this._client.Dispose();
				this._client = null;
			}
		}

		#endregion

		#region Цикл обработки запросов

		private async Task DoWork()
		{
			this.CreateClient();

			try
			{
				_logger.LogInformation("YksSmsPaymentHostedService is working.");
				bool firstTime = true;
				while (!this._cts.IsCancellationRequested)
				{
					await ProcessCycle(firstTime);
					firstTime = false;
				}
			}
			finally
			{
				this.CloseClient();
				_logger.LogInformation("YksSmsPaymentHostedService is completing work.");
			}
		}

		private async Task ProcessCycle(bool firstTime)
		{
			try
			{
				TimeSpan interval = TimeSpan.FromMinutes(5);
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
				_logger.LogError(ex.ToString());
			}
		}

		#endregion

		private async Task ProcessPayments()
		{
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.PrepareYandexKassaRequestsForNewCheckIteration", conn))
				{
					await cmd.ExecuteNonQueryAsync();
				}

				while (true)
				{
					PaymentRequest request = null;
					using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.GetYandexKassaRequest", conn))
					{
						cmd.AddTinyIntParam("@Status", (byte)PaymentRequestStatus.Created);
						using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
						{
							if (dr.Read())
							{
								request = new PaymentRequest();
								request.FillFromReader(dr);
							}
						}
					}

					if (request == null)
					{
						break;
					}

					CheckPaymentRequestData data = await this.SendCheckPaymentRequest(request);
					await ProcessCheckPaymentResponse(conn, request, data);
				}
			}
		}

		public async Task<bool> ProcessPaymentFromHook(string json)
		{
			HookDoc hookDoc = null;
			try
			{
				hookDoc = JsonConvert.DeserializeObject<HookDoc>(json, Startup.JsonSettings);
				this._fileLogger.WriteHookLog(json, hookDoc.Payment.Metadata.RequestId);
			}
			catch (Exception ex)
			{
				this._fileLogger.WriteHookErrorLog(json, ex);
				return false;
			}

			PaymentDoc payment = hookDoc.Payment;
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				PaymentRequest request = null;
				using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.GetYandexKassaRequestById", conn))
				{
					cmd.AddIntParam("@RequestId", payment.Metadata.RequestId);
					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						if (dr.Read())
						{
							request = new PaymentRequest();
							request.FillFromReader(dr);
						}
					}
				}

				CheckPaymentRequestData data = new CheckPaymentRequestData(null);
				ResponseData<PaymentDoc> response = data.CreateResponseInstance();
				response.ResponseJson = json;
				response.Response = payment;
				await ProcessCheckPaymentResponse(conn, request, data);
				return true;
			}
		}

		private async Task ProcessCheckPaymentResponse(SqlConnection conn, PaymentRequest request, CheckPaymentRequestData data)
		{
			if (data.HasError || data.ResponseData.Response.Status.IsAny(PaymentDoc.StatusType.Pending, PaymentDoc.StatusType.Canceled))
			{
				PaymentRequestStatus status;
				if (data.HasError)
				{
					status = PaymentRequestStatus.CheckFailed;
				}
				else if (data.ResponseData.Response.Status == PaymentDoc.StatusType.Pending)
				{
					status = PaymentRequestStatus.CheckPending;
				}
				else
				{
					status = (data.ResponseData.Response.CancellationDetails == null || data.ResponseData.Response.CancellationDetails.Party == PaymentDoc.CancellationParty.PaymentNetwork ?
						PaymentRequestStatus.CanceledByUser : PaymentRequestStatus.CanceledByKassa);
				}

				await SaveRequestStatus(conn, request.Id, data, status);
			}
			else if (data.ResponseData.Response.Status == PaymentDoc.StatusType.Succeeded)
			{
				using (SqlTransaction tx = conn.BeginTransaction())
				{
					bool result = await this.CreatePayment(request, data.ResponseData, conn, tx);
					if (!result)
					{
						this._logger.LogError("Платёж уже создан. Запрос:" + request.Id.ToString() + ", идентификатор в системе Яндекс.Касса: " + request.KassaPaymentId);
					}
					else
					{
						await CompleteRequest(request, data, conn, tx);
					}

					tx.Commit();
				}
			}
		}

		private async Task SaveRequestStatus(SqlConnection conn, int requestId, CheckPaymentRequestData data, PaymentRequestStatus status)
		{
			using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.UpdateYandexKassaRequestStatus", conn))
			{
				cmd.AddIntParam("@RequestId", requestId);
				cmd.AddTinyIntParam("@Status", (byte)status);
				cmd.AddVarCharParam("@KassaPaymentId", 50, data.ResponseData.Response.Id);
				await cmd.ExecuteNonQueryAsync();
			}
		}

		private async Task CompleteRequest(PaymentRequest request, CheckPaymentRequestData data, SqlConnection conn, SqlTransaction tx)
		{
			using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.CompleteYandexKassaRequest", conn, tx))
			{
				cmd.AddIntParam("@RequestId", request.Id);
				cmd.AddVarCharParam("@KassaPaymentId", 50, data.ResponseData.Response.Id);
				cmd.AddBigIntParam("@PaymentId", request.PaymentId);
				await cmd.ExecuteNonQueryAsync();
			}
		}

		private async Task<CheckPaymentRequestData> SendCheckPaymentRequest(PaymentRequest request)
		{
			CheckPaymentRequestData data = new CheckPaymentRequestData(request.KassaPaymentId);
			try
			{
				await CheckPaymentRequest.Run(this._client, data, this._logger);
			}
			catch (Exception e)
			{
				this._logger.LogError(e.ToString());
			}
			finally
			{
				this._fileLogger.WriteCheckLog(request.Id, data);
			}

			return data;
		}

		private async Task<bool> CreatePayment(PaymentRequest request, ResponseData<PaymentDoc> response, SqlConnection conn, SqlTransaction tx)
		{
			Share.Payment payment = new Share.Payment();

			payment.Id = 0;
			payment.Status = Share.PaymentStatus.Accounted;
			//payment.DocumentName = response.Response.Id;
			//payment.DocumentNumber = response.Response.Id;
			//payment.DocumentDate = DateTime.ParseExact(response.Response.CapturedAt, "O", CultureInfo.InvariantCulture).Date;

			payment.ExternalId = response.Response.Id;
			payment.DataSource = "YDXKAS";
			payment.Provider = "YKSBER";
			payment.OriginalAmount = request.Amount;
			payment.ReceivedAmount = decimal.Floor(request.Amount * 96M) / 100M;
			payment.PaymentDateTime = request.CreateDateTime;
			payment.IsTimeSpecified = true;
			payment.ArrivalDateTime = DateTime.Now;
			payment.Fio = "";
			payment.Address = "";
			payment.Purpose = response.Response.Description;
			payment.PlaceId = request.Place.Id;
			payment.EmployeeId = request.EmployeeId;
			payment.RawData = response.ResponseJson;

			bool result = await this._shareService.ProceedPayment(payment, conn, tx);
			if (result)
			{
				request.PaymentId = payment.Id;
			}

			return result;
		}
	}
}
