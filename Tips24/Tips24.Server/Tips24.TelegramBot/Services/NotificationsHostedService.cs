using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Tips24.TelegramBot.Properties;
using ZXing.ImageSharp;
using ZXing.QrCode;

namespace Tips24.TelegramBot
{

	public class NotificationsHostedService : IHostedService, IDisposable
	{
		#region Типы данных

		private class Message
		{
			public class PaymentData
			{
				public long PaymentId { get; set; }
				public decimal Amount { get; set; }
			}

			public long TelegramId { get; set; }
			public int EmployeeId { get; set; }
			public long EmployeeBalanceLogId { get; set; }
			public DateTime LogDateTime { get; set; }
			public byte OperationType { get; set; }
			public decimal Amount { get; set; }
			public decimal Balance { get; set; }
			public PaymentData Payment { get; set; }

			public Message(SqlDataReader dr)
			{
				this.TelegramId = dr.GetInt64("TelegramId");
				this.EmployeeId = dr.GetInt32("EmployeeId");
				this.EmployeeBalanceLogId = dr.GetInt64("EmployeeBalanceLogId");
				this.LogDateTime = dr.GetDateTime("LogDateTime");
				this.OperationType = dr.GetByte("OperationType");
				this.Amount = dr.GetDecimal("Amount");
				this.Balance = dr.GetDecimal("Balance");
				long? paymentId = dr.GetInt64OrNull("PaymentId");
				if (paymentId.HasValue)
				{
					this.Payment = new PaymentData();
					this.Payment.PaymentId = paymentId.Value;
					this.Payment.Amount = dr.GetDecimal("OriginalAmount");
				}
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				if (this.OperationType == 1)
				{
					sb.Append("Поступили денежные средства в размере <b>").Append(this.Amount.ToString("F2")).AppendLine(" руб.</b>")
						.Append("Общая сумма чаевых: <b>").Append(this.Payment.Amount.ToString("F2")).AppendLine(" руб.</b>")
						.Append("Баланс лицевого счёта: <b>").Append(this.Balance.ToString("F2")).Append(" руб.</b>");
				}
				else
				{
					sb.Append("Изменение баланса лицевого счёта. Запросите баланс");
				}

				return sb.ToString();
			}
		}

		#endregion

		#region Поля

		private CultureInfo _ruCulture = CultureInfo.GetCultureInfo("ru-RU");
		private UserBotConfiguration _config;

		private HttpClient _httpClient;
		private Telegram.Bot.TelegramBotClient _telegramClient;

		private readonly ILogger _logger;
		private CancellationTokenSource _cts;
		private SqlServer _sqlServer;

		private EmployeeProvider _employeeProvider;

		#endregion

		#region Конструктор, запуск и остановка

		public NotificationsHostedService(EmployeeProvider employeeProvider, ILogger<NotificationsHostedService> logger, SqlServer sqlServer, IConfiguration config)
		{
			this._config = Startup.Config.UserBot;

			this._employeeProvider = employeeProvider;
			this._logger = logger;
			this._sqlServer = sqlServer;

			this._cts = new CancellationTokenSource();
			_httpClient = new HttpClient();
			_telegramClient = new Telegram.Bot.TelegramBotClient(this._config.Token, _httpClient);
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("NotificationsHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("NotificationsHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "NotificationsHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("NotificationsHostedService disposed");
			_cts.Cancel();
		}

		#endregion

		#region Цикл обработки запросов

		private async Task DoWork()
		{
			_logger.LogInformation("NotificationsHostedService is working.");
			while (!this._cts.IsCancellationRequested)
			{
				await BotProcessCycle();
			}
			_logger.LogInformation("NotificationsHostedService is completing work.");
		}

		private async Task BotProcessCycle()
		{
			try
			{
				TimeSpan interval = TimeSpan.FromSeconds(2);
				TimeSpan delay = TimeSpan.Zero;
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

					DateTime startTime = DateTime.UtcNow;
					await this.SendNotifications();

					if (_cts.IsCancellationRequested)
					{
						break;
					}

					DateTime endTime = DateTime.UtcNow;
					TimeSpan timeElapsed = endTime - startTime;
					delay = (timeElapsed > interval ? TimeSpan.Zero : interval - timeElapsed);
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "TelegramHostedService2.BotProcessCycle: " + exStr, _logger);
			}
		}

		private async Task SendNotifications()
		{
			using (SqlConnection conn = this._sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				List<Message> messages = new List<Message>();

				using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.GetUsersToNotify", conn))
				{
					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						while (dr.Read())
						{
							Message m = new Message(dr);
							messages.Add(m);
						}
					}
				}

				foreach (Message m in messages)
				{
					string text = m.ToString();

					using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.UpdateLastBalanceLogId", conn))
					{
						cmd.AddBigIntParam("@UserId", m.TelegramId);
						cmd.AddBigIntParam("@LastBalanceLogId", m.EmployeeBalanceLogId);
						cmd.AddNVarCharParam("@Message", 200, text);

						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
		}

		#endregion
	}
}
