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

namespace Tips24.PaymentService.ModApi
{
	public class ModApiHostedService : IHostedService, IDisposable
	{
		#region Поля

		private readonly ILogger _logger;
		private CancellationTokenSource _cts;
		private SqlServer _sqlServer;
		private Share.ShareService _shareService;
		private ModApiConfiguration _config;
		private HttpClient _client;

		private static SqlMetaData[] _idToCheckMetadata;
		private static SqlMetaData[] _paymentToStoreMetadata;

		#endregion

		#region Конструктор, запуск и остановка

		static ModApiHostedService()
		{
			_idToCheckMetadata = new SqlMetaData[]
			{
				new SqlMetaData("SeqNum", SqlDbType.Int),
				new SqlMetaData("Id", SqlDbType.UniqueIdentifier)
			};

			_paymentToStoreMetadata = new SqlMetaData[]
			{
				new SqlMetaData("SeqNum", SqlDbType.Int),
				new SqlMetaData("Id", SqlDbType.UniqueIdentifier),
				new SqlMetaData("ExecutedDateTime", SqlDbType.DateTime2, 0, 7),
				new SqlMetaData("RawData", SqlDbType.NVarChar, -1)
			};
		}

		public ModApiHostedService(ILogger<ModApiHostedService> logger, SqlServer sqlServer, IConfiguration config, Share.ShareService shareService)
		{
			this._logger = logger;
			this._sqlServer = sqlServer;

			this._cts = new CancellationTokenSource();

			this._config = Startup.Config.ModApi;

			this._shareService = shareService;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("ModApiHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("ModApiHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "ModApiHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("ModApiHostedService disposed");
			_cts.Cancel();
			this.CloseClient();
		}

		private void CreateClient()
		{
			this.CloseClient();
			this._client = new HttpClient();
			if (this._config.Token == "sandboxtoken")
			{
				this._client.DefaultRequestHeaders.Add("sandbox", "on");
			}

			this._client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this._config.Token);
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
				_logger.LogInformation("ModApiHostedService is working.");
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
				_logger.LogInformation("ModApiHostedService is completing work.");
			}
		}

		private async Task ProcessCycle(bool firstTime)
		{
			try
			{
				TimeSpan interval = TimeSpan.FromMinutes(5);
				TimeSpan delay = (firstTime ? TimeSpan.Zero : interval);
				DateTime curIterationBeginTime = DateTime.MinValue;
				DateTime prevIterationBeginTime = DateTime.MinValue;
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
					prevIterationBeginTime = curIterationBeginTime;
					curIterationBeginTime = DateTime.Now;

					await this.CheckPayments(prevIterationBeginTime, curIterationBeginTime);

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

		private async Task CheckPayments(DateTime prevIterationBeginTime, DateTime curIterationBeginTime)
		{
			if (prevIterationBeginTime == DateTime.MinValue)
			{
				await CheckNDays(curIterationBeginTime, this._config.NDaysOnStartup);
			}
			else if (this.NeedToCheck5Days(prevIterationBeginTime, curIterationBeginTime))
			{
				await CheckNDays(curIterationBeginTime, this._config.NDaysNightly);
			}
			else if (this.NeedToCheckThisDayAndYesterday(prevIterationBeginTime, curIterationBeginTime))
			{
				await this.NeedToCheckThisDayAndYesterday(curIterationBeginTime);
			}
			else if (this.NeedToCheckThisDay(prevIterationBeginTime, curIterationBeginTime))
			{
				await this.CheckThisDay(curIterationBeginTime);
			}
			else
			{
				await this.CheckNewPayments(curIterationBeginTime);
			}
		}

		private async Task CheckNewPayments(DateTime curIterationBeginTime)
		{
			string from = curIterationBeginTime.ToString("yyyy-MM-dd");
			await this.StoreThisDayPayments(from, true);
		}

		private async Task CheckThisDay(DateTime curIterationBeginTime)
		{
			string from = curIterationBeginTime.ToString("yyyy-MM-dd");
			await this.StoreThisDayPayments(from, false);
		}

		private async Task NeedToCheckThisDayAndYesterday(DateTime curIterationBeginTime)
		{
			string from = (curIterationBeginTime.Subtract(TimeSpan.FromDays(1))).ToString("yyyy-MM-dd");
			string till = from;
			await this.StorePayments(from, till);

			await this.CheckThisDay(curIterationBeginTime);
		}

		private async Task CheckNDays(DateTime curIterationBeginTime, int nDays)
		{
			string from = (curIterationBeginTime.Subtract(TimeSpan.FromDays(nDays))).ToString("yyyy-MM-dd");
			string till = (curIterationBeginTime.Subtract(TimeSpan.FromDays(1))).ToString("yyyy-MM-dd");
			await this.StorePayments(from, till);

			await this.CheckThisDay(curIterationBeginTime);
		}

		private bool NeedToCheckThisDay(DateTime p, DateTime c)
		{
			return p.Minute > c.Minute;
		}

		private bool NeedToCheckThisDayAndYesterday(DateTime p, DateTime c)
		{
			return (p.Hour == 2 && c.Hour == 3 ||
				p.Hour == 5 && c.Hour == 6 ||
				p.Hour == 8 && c.Hour == 9 ||
				p.Hour == 11 && c.Hour == 12 ||
				p.Hour == 14 && c.Hour == 15 ||
				p.Hour == 17 && c.Hour == 18 ||
				p.Hour == 20 && c.Hour == 21 ||
				p.Hour == 23 && c.Hour == 0);
		}

		private bool NeedToCheck5Days(DateTime p, DateTime c)
		{
			return (p.Hour == 5 && c.Hour == 6);
		}

		#endregion

		#region Логика получения информации о платежах

		private async Task StorePayments(string from, string till)
		{
			int offset = 0;
			int records = this._config.FetchSize;
			int increment = records;
			OperationHistoryRequest.Parameters p = new OperationHistoryRequest.Parameters { category = "Debet", from = from, till = till, skip = offset, records = records };
			while (true)
			{
				List<OperationHistoryRequest.Operation> list = await OperationHistoryRequest.Run(this._client, this._config.AccountId, p);
				if (list.Count == 0)
				{
					return;
				}

				await this.SavePaymentRecords(list);

				if (list.Count < records)
				{
					return;
				}

				p.skip += increment;
			}
		}

		private async Task StoreThisDayPayments(string from, bool untilFirstFoundRecord)
		{
			int offset = 0;
			int records = this._config.FetchSize;
			int increment = records / 2;

			OperationHistoryRequest.Parameters p = new OperationHistoryRequest.Parameters { category = "Debet", from = from, skip = offset, records = records };
			while (true)
			{
				List<OperationHistoryRequest.Operation> list = await OperationHistoryRequest.Run(this._client, this._config.AccountId, p);
				if (list.Count == 0)
				{
					return;
				}

				bool firstRecordAlreadyInDb = await this.SavePaymentRecords(list);
				if (untilFirstFoundRecord && firstRecordAlreadyInDb)
				{
					return;
				}

				if (list.Count < records)
				{
					return;
				}

				p.skip += increment;
			}
		}

		#endregion

		#region Сохранение данных о платежах

		private async Task<bool> SavePaymentRecords(List<OperationHistoryRequest.Operation> list)
		{
			DataAccess.StructuredParamValue idList = new DataAccess.StructuredParamValue(_idToCheckMetadata, list.Count);
			for (int i = list.Count - 1, j = 1; i >= 0; i--, j++)
			{
				OperationHistoryRequest.Operation o = list[i];
				idList.NewRecord();
				idList.AddInt32(j);
				idList.AddGuid(o.id);
			}

			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				HashSet<Guid> paymentsToSave = new HashSet<Guid>();
				bool? firstRecordAlreadyInDb = null;
				using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.CheckModApiPaymentList", conn))
				{
					cmd.AddStructuredParam("@IdList", "payment.OrderedGuidList", idList);

					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						while (dr.Read())
						{
							if (!firstRecordAlreadyInDb.HasValue)
							{
								firstRecordAlreadyInDb = (dr.GetInt32("SeqNum") != 1);
							}

							paymentsToSave.Add(dr.GetGuid("Id"));
						}
					}
				}

				if (paymentsToSave.Count > 0)
				{
					DataAccess.StructuredParamValue paymentList = new DataAccess.StructuredParamValue(_paymentToStoreMetadata, paymentsToSave.Count);
					for (int i = list.Count - 1, j = 1; i >= 0; i--, j++)
					{
						OperationHistoryRequest.Operation o = list[i];
						if (paymentsToSave.Contains(o.id))
						{
							paymentList.NewRecord();
							paymentList.AddInt32(j);
							paymentList.AddGuid(o.id);
							paymentList.AddDateTime(DateTime.ParseExact(o.executed, "s", CultureInfo.InvariantCulture));
							paymentList.AddString(JsonConvert.SerializeObject(o));
						}
					}

					using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.SaveModApiPaymentList", conn))
					{
						cmd.AddStructuredParam("@List", "payment.ModApiPaymentList", paymentList);
						await cmd.ExecuteNonQueryAsync();
					}
				}

				return !firstRecordAlreadyInDb.HasValue || firstRecordAlreadyInDb.Value;
			}
		}

		#endregion
	}
}
