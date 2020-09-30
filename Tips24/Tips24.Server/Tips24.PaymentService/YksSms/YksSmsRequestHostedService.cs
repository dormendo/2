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
	public class YksSmsRequestHostedService : IHostedService, IDisposable
	{
		#region Поля

		private readonly ILogger _logger;
		private CancellationTokenSource _cts;
		private SqlServer _sqlServer;
		private YksSmsConfiguration _config;
		private HttpClient _client;
		private Logger _fileLogger;

		#endregion

		#region Конструктор, запуск и остановка

		public YksSmsRequestHostedService(ILogger<YksSmsRequestHostedService> logger, SqlServer sqlServer, IConfiguration config)
		{
			this._logger = logger;
			this._sqlServer = sqlServer;

			this._cts = new CancellationTokenSource();

			this._config = Startup.Config.YksSms;
			this._fileLogger = new Logger(this._config.LogFolder);
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("YksSmsRequestHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("YksSmsRequestHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "YksSmsRequestHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("YksSmsRequestHostedService disposed");
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
				_logger.LogInformation("YksSmsRequestHostedService is working.");
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
				_logger.LogInformation("YksSmsRequestHostedService is completing work.");
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

					await this.CreatePayments();

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

		#region Диспетчеризация вызовов

		private async Task CreatePayments()
		{
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				while (true)
				{
					PaymentRequest request = null;
					using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.GetYandexKassaRequest", conn))
					{
						cmd.AddTinyIntParam("@Status", (byte)PaymentRequestStatus.Arrived);
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

					CreatePaymentInDoc createPaymentDoc = new CreatePaymentInDoc();
					createPaymentDoc.Amount.Amount = request.Amount;
					createPaymentDoc.Capture = true; // После того, как гость подтвердил платёж, мы уже не должны подтверждать его
					createPaymentDoc.Description = "Дарение чаевых по агентскому договору tips24.ru/" + request.Place.Id.ToString() +
						(request.EmployeeId.HasValue ? "-" + request.EmployeeId.Value.ToString() : "");
					createPaymentDoc.PaymentMethodData.Phone = request.UserLogin;
					createPaymentDoc.Metadata.RequestId = request.Id;

					CreatePaymentRequestData data = await this.SendCreatePaymentRequest(createPaymentDoc);
					bool isError = (data.HasError || data.ResponseData.Response.Status == PaymentDoc.StatusType.Canceled);

					using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.UpdateYandexKassaRequestStatus", conn))
					{
						cmd.AddIntParam("@RequestId", request.Id);
						cmd.AddTinyIntParam("@Status", (byte)(isError ? PaymentRequestStatus.CreateFailed : PaymentRequestStatus.Created));
						cmd.AddVarCharParam("@KassaPaymentId", 50, (isError ? null : data.ResponseData.Response.Id));
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
		}

		private async Task<CreatePaymentRequestData> SendCreatePaymentRequest(CreatePaymentInDoc createPaymentDoc)
		{
			CreatePaymentRequestData data = new CreatePaymentRequestData(createPaymentDoc);
			try
			{
				await CreatePaymentRequest.Run(this._client, data, this._logger);
			}
			catch (Exception e)
			{
				this._logger.LogError(e.ToString());
			}
			finally
			{
				this._fileLogger.WriteCreateLog(createPaymentDoc.Metadata.RequestId, data);
			}

			return data;
		}

		#endregion
	}
}
