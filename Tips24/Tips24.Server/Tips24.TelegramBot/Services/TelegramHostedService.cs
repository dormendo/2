using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
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
	public partial class TelegramHostedService : IHostedService, IDisposable
	{
		#region Поля

		private CultureInfo _ruCulture = CultureInfo.GetCultureInfo("ru-RU");
		private UserBotConfiguration _config;

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

		public TelegramHostedService(EmployeeProvider employeeProvider, ILogger<TelegramHostedService> logger, SqlServer sqlServer, IConfiguration config)
		{
			this._config = Startup.Config.UserBot;

			this._employeeProvider = employeeProvider;
			this._logger = logger;
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

			if (!TimeSpan.TryParse(this._config.TimeToCancelTurns, out this._timeToCancelTurns))
			{
				this._timeToCancelTurns = TimeSpan.FromHours(6);
			}
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("TelegramHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("TelegramHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "TelegramHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("TelegramHostedService disposed");
			_cts.Cancel();
		}

		#endregion

		#region Цикл обработки запросов

		private async Task DoWork()
		{
			_logger.LogInformation("TelegramHostedService is working.");
			bool firstTime = true;
			while (!this._cts.IsCancellationRequested)
			{
				await BotProcessCycle(firstTime);
				firstTime = false;
			}
			_logger.LogInformation("TelegramHostedService is completing work.");
		}

		private async Task BotProcessCycle(bool firstTime)
		{
			try
			{
				await this.LoadBotSettings();

				DateTime lastCapturedDateTime = DateTime.UtcNow;
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
					if (lastCapturedDateTime.TimeOfDay < _timeToCancelTurns && startTime.TimeOfDay >= _timeToCancelTurns ||
						lastCapturedDateTime.TimeOfDay <= _timeToCancelTurns && startTime.TimeOfDay > _timeToCancelTurns)
					{
						await this.CancelAllTurns();
					}

					lastCapturedDateTime = startTime;

					Update[] updates = await _telegramClient.GetUpdatesAsync(_offset, 10, 0, null, _cts.Token);
					if (_cts.IsCancellationRequested)
					{
						break;
					}

					foreach (Update update in updates)
					{
						_offset = Math.Max(_offset, update.Id) + 1;
						await this.ProceedUpdate(update);
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
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "TelegramHostedService.BotProcessCycle: " + exStr, _logger);
			}
		}

		private async Task UpdateOffset(int offset)
		{
			try
			{
				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.UpdateBotOffset", conn))
					{
						cmd.AddIntParam("@BotOffset", offset);
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "TelegramHostedService.UpdateOffset: " + exStr, _logger);
			}
		}

		private async Task LoadBotSettings()
		{
			try
			{
				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.LoadSettings", conn))
					{
						using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
						{
							if (dr.Read())
							{
								this._offset = dr.GetInt32("BotOffset");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "TelegramHostedService.LoadBotSettings: " + exStr, _logger);
			}
		}

		#endregion

		#region Обработка запросов
		
		private async Task ProceedUpdate(Update update)
		{
			try
			{
				Employee employee;
				if (update.Message.Contact != null)
				{
					employee = await this._employeeProvider.SetPhoneAndGetEmployee(update.Message.Contact.UserId, update.Message.Contact.PhoneNumber);
					await this.WriteMessageLog(new MessageLog(update, employee));
					if (employee == null)
					{
						string text = "Вы не являетесь зарегистрированным участником проекта Чаевые-24. Проект Чаевые-24 позволяет работникам сферы услуг получать безналичные чаевые. Для подключения Вашего заведения свяжитесь с нашим учредителем:\r\nРинат @Rinat_G, +7 (927) 244-16-78";
						await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, null, _cts.Token);
						await this.WriteMessageLog(new MessageLog(MessageLog.Types.NotRegistered, update.Message.From.Id));
						return;
					}
				}
				else
				{
					employee = await this._employeeProvider.GetEmployeeByTelegramUserIdAsync(update.Message.From.Id);
					await this.WriteMessageLog(new MessageLog(update, employee));
					if (employee == null)
					{
						string text = "Для авторизации в проекте Чаевые-24 нажмите на кнопку ниже";
						await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, this.GetPhoneKeyboardMarkup(), _cts.Token);
						await this.WriteMessageLog(new MessageLog(MessageLog.Types.Authorize, update.Message.From.Id));
						return;
					}
				}

				await this.ProcessCommand(update, employee);
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "TelegramHostedService.ProceedUpdate: " + exStr, _logger);
			}
		}

		private async Task ProcessCommand(Update update, Employee employee)
		{
			if (employee.DialogSession != null)
			{
				if (employee.DialogSession.SessionType == DialogType.YandexKassa)
				{
//					await this.ProcessYandexKassaConversation(update, employee);
				}
				else
				{
					await this.ProcessTurnConversation(update, employee);
				}
			}
			else
			{
				await this.ProcessRegularCommand(update, employee);
			}
		}

		private async Task ProcessRegularCommand(Update update, Employee employee)
		{
			if (update.Message.Contact != null)
			{
				await this.GetAutorizedMessageAsync(update, employee);
				return;
			}

			string command = update.Message.Text.Trim().ToLower();
			if (employee.IsManager && employee.IsActive && (command == "отчет" || command == "отчёт" || command == "/s"))
			{
				await this.GetEmployeesReportAsync(update, employee);
				return;
			}
			else if (employee.IsActive && (command == "принять ₽" || command == "qr-код" || command == "/qr"))
			{
				await this.GetQrCodeAsync(update, employee);
			}
			else if (command == "/start")
			{
				await this.GreetingAsync(update, employee);
			}
			else if (command == "баланс" || command == "/b")
			{
				await this.GetBalanceAsync(update, employee);
			}
			else if (command == "вывод" || command == "вывод ₽" || command == "/v")
			{
				await this.GetPayoutRequestAsync(update, employee);
			}
			else if (command == "помощь" || command == "/i" || command == "/help")
			{
				await this.GetInfoAsync(update, employee);
			}
			else if (employee.IsActive && (command == "смена" || command == "смены" || command == "моя смена"))
			{
				await this.StartTurnSession(update, employee);
			}
			else
			{
				await this.GetCommandNotSupported(update, employee);
			}
		}

		private async Task GetAutorizedMessageAsync(Update update, Employee employee)
		{
			string text = "Добро пожаловать, " + employee.FirstName + " " + employee.LastName + "! Вы успешно авторизовались в проекте Чаевые-24. Для работы в системе используйте команды, представленные в нашем меню";
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		#endregion

		#region Команды

		private async Task GreetingAsync(Update update, Employee employee)
		{
			string text = Resources.EmployeeGreeting;
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private async Task GetBalanceAsync(Update update, Employee employee)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<b>Ваш баланс: ").Append(employee.Balance.ToString("F2", _ruCulture)).Append(" рублей</b>")
				.AppendLine();

			bool hasRecords = false;
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.GetBalance", conn))
				{
					cmd.AddBigIntParam("@EmployeeId", employee.Id);

					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						while (dr.Read())
						{
							if (!hasRecords)
							{
								sb.AppendLine().Append("<b>Последние операции:</b>");
								hasRecords = true;
							}

							DateTime logDateTime = dr.GetDateTime("LogDateTime");
							byte operationType = dr.GetByte("OperationType");
							decimal amount = dr.GetDecimal("Amount");
							long? paymentId = dr.GetInt64OrNull("PaymentId");
							decimal? originalAmount = dr.GetDecimalOrNull("OriginalAmount");

							sb.AppendLine()
								.Append("<b>").Append(amount < 0 ? "−" : "+").Append(Math.Abs(amount).ToString("F2", _ruCulture))
								.Append("</b> руб. - ").Append(logDateTime.ToTelegramReportString()).Append(", ");

							if (operationType == 1)
							{
								sb.Append("общая сумма чаевых ").Append(originalAmount.Value.ToString("F2", _ruCulture));
							}
							else if (operationType == 2)
							{
								sb.Append("вывод на карту");
							}
							else if (operationType == 3)
							{
								sb.Append("возврат чаевых по запросу гостя");
							}
						}
					}
				}
			}

			if (!hasRecords)
			{
				sb.Append("По Вашему счёту операций не зарегистрировано");
			}

			string text = sb.ToString();
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private async Task GetEmployeesReportAsync(Update update, Employee employee)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Отчёт о поступлении средств сотрудникам c ").Append(DateTime.Now.AddDays(-30).Date.ToString("dd MMMM yyyy", _ruCulture)).Append(" г. по текущий момент");

			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.GetEmployeeReport", conn))
				{
					cmd.AddIntParam("@PlaceId", employee.Place.Id);

					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						while (dr.Read())
						{
							string firstName = dr.GetString("FirstName");
							string lastName = dr.GetString("LastName");
							int groupId = dr.GetInt32("GroupId");
							string groupName = dr.GetString("GroupName");
							decimal amount = dr.GetDecimal("Amount");

							sb.AppendLine().AppendLine()
								.Append("<b>").Append(amount < 0 ? "−" : "").Append(Math.Abs(amount).ToString("F2", _ruCulture)).Append("</b> руб. - ")
								.Append(firstName).Append(" ").Append(lastName).Append(" (").Append(groupName).Append(")");
						}
					}
				}
			}

			string text = sb.ToString();
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private async Task GetQrCodeAsync(Update update, Employee employee)
		{
			string receiverCode = employee.Place.Id.ToString() + "-" + employee.Id.ToString();
			string qrString = "ST00011|Name=ООО Чаевые-24|PersonalAcc=40702810970010113722|BankName=МОСКОВСКИЙ ФИЛИАЛ АО КБ \"МОДУЛЬБАНК\"|" +
				"BIC=044525092|CorrespAcc=30101810645250000092|PayeeINN=1651083591|" +
				"Purpose=Дарение чаевых коллективу по договору-оферте tips24.ru/" + receiverCode + "|" +
				"PayerAddress=" + employee.Place.City + ", " + employee.Place.Address + "|LastName=Гость|FirstName=заведения";
			//string qrString = "ST00011|Name=ИП Галяутдинов Ринат Ибрагимович|PersonalAcc=40802810470210002677|BankName=МОСКОВСКИЙ ФИЛИАЛ АО КБ \"МОДУЛЬБАНК\"|BIC=044525092|CorrespAcc=30101810645250000092|PayeeINN=165117672519|" +
			//	"Purpose=Дарение чаевых коллективу по договору-оферте tips24.ru/" + receiverCode + "|" +
			//	"PayerAddress="+ employee.PlaceCity + ", " + employee.PlaceAddress + "|LastName=Гость|FirstName=заведения";

			byte[] hash = GetQrHash(qrString);
			if (employee.QrCode != null && employee.QrCode.IsValid(hash))
			{
				ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
				Message response = await _telegramClient.SendPhotoAsync(update.Message.From.Id, new InputOnlineFile(employee.QrCode.FileId),
					null, ParseMode.Default, false, 0, keyboard, _cts.Token);
				await this.WriteMessageLog(new QrCodeOutputMessageLog(employee, employee.QrCode.FileId, receiverCode, keyboard));
				return;
			}

			QRCodeWriter qrWriter = new QRCodeWriter();
			Dictionary<ZXing.EncodeHintType, object> hints = new Dictionary<ZXing.EncodeHintType, object>();
			hints.Add(ZXing.EncodeHintType.CHARACTER_SET, "windows-1251");
			hints.Add(ZXing.EncodeHintType.MARGIN, 1);
			ZXing.Common.BitMatrix matrix = qrWriter.encode(qrString, ZXing.BarcodeFormat.QR_CODE, 640, 640, hints);

			BarcodeWriter<Rgb24> writer = new BarcodeWriter<Rgb24>();

			string fileId = null;
			using (MemoryStream ms = new MemoryStream())
			{
				using (Image<Rgb24> image = writer.Write(matrix))
				{
					image.Save(ms, new PngEncoder() { ColorType = PngColorType.Grayscale, BitDepth = PngBitDepth.Bit8 });
				}
				ms.Position = 0;

				ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
				Message response = await _telegramClient.SendPhotoAsync(update.Message.From.Id, new InputOnlineFile(ms), null, ParseMode.Default, false, 0, keyboard, _cts.Token);
				fileId = response.Photo[0].FileId;
				await this.WriteMessageLog(new QrCodeOutputMessageLog(employee, fileId, receiverCode, keyboard));
			}

			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.UpdateQrCodeFileId", conn))
				{
					cmd.AddBigIntParam("@UserId", employee.TelegramUserId);
					cmd.AddVarCharParam("@QrCodeFileId", 64, fileId);
					cmd.AddBinaryParam("@QrCodeStringHash", 40, hash);

					await cmd.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task GetPayoutRequestAsync(Update update, Employee employee)
		{
			string text = "Отправьте запрос на вывод денег в чате поддержки @Tips24_Help_Bot";
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private async Task GetInfoAsync(Update update, Employee employee)
		{
			string text = $"Вы работаете в заведении \"{employee.Place.Name}\" ({employee.Place.City}). Ваш ИД - {employee.Id}. Перейдите в чат поддержки @Tips24_Help_Bot";
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private async Task GetCommandNotSupported(Update update, Employee employee)
		{
			string text = "Вы отправили неизвестную команду. Если Вы считаете, что произошла ошибка, обратитесь к разделу <b>Помощь</b>";
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(employee, MessageLog.Types.InvalidCommand, keyboard));
		}

		#endregion

		#region Элементы управления

		public static ReplyKeyboardMarkup GetStandardKeyboardMarkup(Employee employee)
		{
			List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
			List<KeyboardButton> row1 = new List<KeyboardButton>();
			List<KeyboardButton> row2 = new List<KeyboardButton>();

			if (employee.IsManager)
			{
				if (employee.IsActive)
				{
					row1.Add(new KeyboardButton("Отчёт"));
					row1.Add(new KeyboardButton("Принять ₽"));
					if (!employee.Place.IsSchemeIndividual)
					{
						row1.Add(new KeyboardButton("Смены"));
					}
				}

				row2.Add(new KeyboardButton("Баланс"));
				row2.Add(new KeyboardButton("Вывод ₽"));
				row2.Add(new KeyboardButton("Помощь"));
			}
			else
			{
				if (employee.IsActive)
				{
					row1.Add(new KeyboardButton("Принять ₽"));
					if (!employee.Place.IsSchemeIndividual)
					{
						row1.Add(new KeyboardButton("Моя смена"));
					}
				}

				row2.Add(new KeyboardButton("Баланс"));
				row2.Add(new KeyboardButton("Вывод ₽"));
				row2.Add(new KeyboardButton("Помощь"));
			}

			if (row1.Count > 0)
			{
				rows.Add(row1);
			}

			rows.Add(row2);

			ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(rows, true, false);
			return markup;
		}

		private ReplyKeyboardMarkup GetPhoneKeyboardMarkup()
		{
			KeyboardButton button = new KeyboardButton("Нажмите на кнопку") { RequestContact = true };
			List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
			List<KeyboardButton> row1 = new List<KeyboardButton>();
			row1.Add(button);
			rows.Add(row1);
			ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(rows, true, true);
			return markup;
		}

		#endregion

		#region Сохранение сообщений в журнале

		private async Task WriteMessageLog(MessageLog log)
		{
			try
			{
				using (SqlConnection conn = this._sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.WriteMessageLog", conn))
					{
						cmd.AddBitParam("@InputMessage", log.InputMessage);
						cmd.AddBigIntParam("@TelegramId", log.TelegramId);
						cmd.AddCharParam("@Phone", 10, log.Phone);
						cmd.AddIntParam("@EmployeeId", log.EmployeeId);
						cmd.AddNVarCharMaxParam("@MessageData", log.GetJson());

						await cmd.ExecuteNonQueryAsync();
					}
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				_logger.LogError(exStr, "Ошибка при сохранениии записи в журнале сообщений");
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "TelegramHostedService.WriteMessageLog, Ошибка при сохранениии записи в журнале сообщений: " + exStr, _logger);
			}
		}

		#endregion

		#region Прочие методы

		private static byte[] GetQrHash(string qrMessage)
		{
			byte[] utf8Ba = Encoding.UTF8.GetBytes(qrMessage);
			byte[] utf16Ba = Encoding.Unicode.GetBytes(qrMessage);
			using (SHA1 sha1 = SHA1.Create())
			{
				byte[] hash = new byte[40];
				byte[] utf8Hash = sha1.ComputeHash(utf8Ba);
				byte[] utf16Hash = sha1.ComputeHash(utf16Ba);
				Buffer.BlockCopy(utf8Hash, 0, hash, 0, 20);
				Buffer.BlockCopy(utf16Hash, 0, hash, 20, 20);

				return hash;
			}
		}

		#endregion

		#region Отправка оповещений

		private class Notification
		{
			public long TelegramId;
			public DateTime CreateDateTime;
			public string Message;

			public Notification(SqlDataReader dr)
			{
				this.TelegramId = dr.GetInt64("TelegramId");
				this.CreateDateTime = dr.GetDateTime("CreateDateTime");
				this.Message = dr.GetString("Message");
			}
		}

		private async Task SendNotifications()
		{
			using (SqlConnection conn = this._sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				List<Notification> notifications = new List<Notification>();

				using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.Notifications_Proceed", conn))
				{
					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						while (dr.Read())
						{
							Notification n = new Notification(dr);
							notifications.Add(n);
						}
					}
				}

				foreach (Notification n in notifications)
				{
					await _telegramClient.SendTextMessageAsync(n.TelegramId, n.Message, ParseMode.Html, false, false, 0, null, _cts.Token);
				}
			}
		}

		#endregion

		#region Закрытие всех смен

		private async Task CancelAllTurns()
		{
			using (SqlConnection conn = this._sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.Turn_ExitAll", conn))
				{
					await cmd.ExecuteNonQueryAsync();
				}
			}
		}

		#endregion
	}
}
