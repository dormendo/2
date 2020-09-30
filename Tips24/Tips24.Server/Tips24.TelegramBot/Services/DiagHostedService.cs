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
	public partial class DiagHostedService : IHostedService, IDisposable
	{
		#region Поля

		private BotConfiguration _config;

		private HttpClient _httpClient;
		private Telegram.Bot.TelegramBotClient _telegramClient;

		private readonly ILogger _logger;
		private CancellationTokenSource _cts;
		private SqlServer _sqlServer;

		private int _offset = 0;
		private EmployeeProvider _employeeProvider;

		private TimeSpan _timeToCancelTurns;

		#endregion

		#region Конструктор, запуск и остановка

		public DiagHostedService(ILogger<DiagHostedService> logger, SqlServer sqlServer, IConfiguration config)
		{
			this._logger = logger;
			this._config = Startup.Config.DiagBot;
			this._sqlServer = sqlServer;

			if (this._config.ApiUrl != null)
			{
				_httpClient = new HttpClient(new ProxyMessageHandler(this._config.ApiUrl));
			}
			else
			{
				_httpClient = new HttpClient();
			}

			this._cts = new CancellationTokenSource();
			_telegramClient = new Telegram.Bot.TelegramBotClient(this._config.Token, _httpClient);
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("DiagHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("DiagHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "DiagHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("DiagHostedService disposed");
			_cts.Cancel();
		}

		#endregion

		#region Цикл обработки запросов

		private async Task DoWork()
		{
			_logger.LogInformation("DiagHostedService is working.");
			bool firstTime = true;
			while (!this._cts.IsCancellationRequested)
			{
				await BotProcessCycle(firstTime);
				firstTime = false;
			}
			_logger.LogInformation("DiagHostedService is completing work.");
		}

		private async Task BotProcessCycle(bool firstTime)
		{
			try
			{
				await this.LoadBotSettings();


				TimeSpan interval = TimeSpan.FromMilliseconds(this._config.Interval);
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

					DateTime startTime = DateTime.UtcNow;
					Update[] updates = await _telegramClient.GetUpdatesAsync(_offset, 10, 0, null, _cts.Token);
					if (_cts.IsCancellationRequested)
					{
						break;
					}

					foreach (Update update in updates)
					{
						_offset = Math.Max(_offset, update.Id) + 1;
						if (_cts.IsCancellationRequested)
						{
							break;
						}

						await this.UpdateOffset(_offset);
					}

					if (_cts.IsCancellationRequested)
					{
						break;
					}

					await this.SendNotifications();

					DateTime endTime = DateTime.UtcNow;
					TimeSpan timeElapsed = endTime - startTime;
					delay = (timeElapsed > interval ? TimeSpan.Zero : interval - timeElapsed);
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "DiagHostedService.BotProcessCycle: " + exStr, _logger);
			}
		}

		private async Task UpdateOffset(int offset)
		{
			try
			{
				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.Diag_UpdateBotOffset", conn))
					{
						cmd.AddIntParam("@DiagBotOffset", offset);
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "DiagHostedService.UpdateOffset: " + exStr, _logger);
			}
		}

		private async Task LoadBotSettings()
		{
			try
			{
				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.Diag_LoadSettings", conn))
					{
						using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
						{
							if (dr.Read())
							{
								this._offset = dr.GetInt32("DiagBotOffset");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "DiagHostedService.LoadBotSettings: " + exStr, _logger);
			}
		}

		#endregion

		#region Отправка оповещений

		private class DiagBotUser
		{
			public long TelegramId;
			public int Options;
			public int MessageCount;
			private StringBuilder _messages;

			public DiagBotUser(SqlDataReader dr)
			{
				this.TelegramId = dr.GetInt64("TelegramId");
				this.Options = dr.GetInt32("Options");
			}

			public void AddMessageIfNeeded(DiagNotification n)
			{
				if ((this.Options & n.Options) != 0)
				{
					if (_messages == null)
					{
						_messages = new StringBuilder();
					}

					this.MessageCount++;
					_messages.Append("<b>").Append(this.MessageCount).Append("</b>. (").Append(n.CreateDateTime.ToString("yyyy-MM-dd hh:mm:ss.fff")).Append("): ").AppendLine(n.Message).AppendLine();
				}
			}

			public string GetMessages()
			{
				if (this.MessageCount > 0)
				{
					return _messages.ToString();
				}
				else
				{
					return null;
				}
			}
		}

		private class DiagNotification
		{
			public long MessageId;
			public DateTime CreateDateTime;
			public int Options;
			public string Message;

			public DiagNotification(SqlDataReader dr)
			{
				this.MessageId = dr.GetInt64("MessageId");
				this.CreateDateTime = dr.GetDateTime("CreateDateTime");
				this.Options = dr.GetInt32("Options");
				this.Message = dr.GetString("Message");
			}
		}

		private async Task SendNotifications()
		{
			List<DiagBotUser> users = new List<DiagBotUser>();
			List<DiagNotification> notifications = new List<DiagNotification>();

			using (SqlConnection conn = this._sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.Diag_Notifications_Proceed", conn))
				{
					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						while (dr.Read())
						{
							users.Add(new DiagBotUser(dr));
						}

						if (users.Count > 0)
						{
							dr.NextResult();

							while (dr.Read())
							{
								notifications.Add(new DiagNotification(dr));
							}
						}
					}
				}
			}

			if (notifications.Count > 0)
			{
				foreach (DiagNotification n in notifications)
				{
					foreach (DiagBotUser u in users)
					{
						u.AddMessageIfNeeded(n);
					}
				}

				foreach (DiagBotUser u in users)
				{
					if (u.MessageCount > 0)
					{
						try
						{
							await _telegramClient.SendTextMessageAsync(u.TelegramId, u.GetMessages(), ParseMode.Html, false, false, 0, null, _cts.Token);
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, "Ошибка при отправке оповещения");
						}
					}
				}
			}
		}

		#endregion
	}
}
