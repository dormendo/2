using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Tips24.TelegramBot.Controllers
{
	[ApiController]
	public class TelegramController : ControllerBase
	{
		private EmployeeProvider _employeeProvider;
		private ILogger<TelegramController> _logger;
		private static HttpClient _httpClient;
		private static Telegram.Bot.TelegramBotClient _telegramClient;

		static TelegramController()
		{
			_httpClient = new HttpClient();
			_telegramClient = new Telegram.Bot.TelegramBotClient("748560635:AAHIyDsYhn4KYNeXyZ9HTYa7eExeDRyLhy4", _httpClient);
		}

		public TelegramController(EmployeeProvider cache, ILogger<TelegramController> logger)
		{
			this._employeeProvider = cache;
			this._logger = logger;
		}

		[HttpPost]
		[Route("telegram/hook")]
		public async Task<HttpResponseMessage> Hook(Update update)
		{
			try
			{
				await WriteUpdate(update);

				HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
				Employee employee;
				if (update.Message.Contact != null)
				{
					employee = await this._employeeProvider.SetPhoneAndGetEmployee(update.Message.Contact.UserId, update.Message.Contact.PhoneNumber);
					if (employee == null)
					{
						SendMessageRequest req = new SendMessageRequest(update.Message.From.Id,
							"Вы не являетесь зарегистрированным участником проекта Чаевые-24. Проект Чаевые-24 позволяет работникам сферы услуг получать безналичные чаевые. Для подключения Вашего заведения позвоните на горячую линию отдела подключения +7 (812) 2-12-85-07.");
						message.Content = req.ToHttpContent();
						return message;
					}
				}
				else
				{
					employee = await this._employeeProvider.GetEmployeeByTelegramUserIdAsync(update.Message.From.Id);
					if (employee == null)
					{
						SendMessageRequest req = new SendMessageRequest(update.Message.From.Id,
							"Для авторизации в проекте Чаевые-24 отправьте, пожалуйста, свой номер телефона");
						req.ReplyMarkup = this.GetPhoneKeyboardMarkup();

						message.Content = req.ToHttpContent();
						return message;
					}
				}

				SendMessageRequest responseMessage = this.ProcessCommand(update, employee);
				message.Content = responseMessage.ToHttpContent();
				return message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка");
				throw;
			}
		}

		private SendMessageRequest ProcessCommand(Update update, Employee employee)
		{
			string message = $"{employee.FirstName} {employee.LastName}, Вы отправили команду: {update.Message.Text}";
			SendMessageRequest req = new SendMessageRequest(update.Message.From.Id, message);
			req.ReplyMarkup = this.GetCommandsKeyboardMarkup();
			return req;
		}

		private ReplyKeyboardMarkup GetCommandsKeyboardMarkup()
		{
			List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
			List<KeyboardButton> row1 = new List<KeyboardButton>();
			List<KeyboardButton> row2 = new List<KeyboardButton>();

			row1.Add(new KeyboardButton("Баланс"));
			row1.Add(new KeyboardButton("Отчёт"));
			row1.Add(new KeyboardButton("QR-код"));

			row2.Add(new KeyboardButton("Вывод"));
			row2.Add(new KeyboardButton("Возврат"));
			row2.Add(new KeyboardButton("Инфо"));

			rows.Add(row1);
			rows.Add(row2);

			ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(rows, true, true);
			return markup;
		}

		private ReplyKeyboardMarkup GetPhoneKeyboardMarkup()
		{
			KeyboardButton button = new KeyboardButton("Телефон") { RequestContact = true };
			List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
			List<KeyboardButton> row1 = new List<KeyboardButton>();
			row1.Add(button);
			rows.Add(row1);
			ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(rows, true, true);
			return markup;
		}

		private async Task WriteUpdate(Update update)
		{
			using (StreamWriter sw = new StreamWriter("C:\\Temp\\UpdateLog.txt", true, Encoding.UTF8, 128 * 1024))
			{
				await sw.WriteLineAsync(DateTime.Now.ToString());
				await sw.WriteLineAsync(Request.ContentType);
				await sw.WriteLineAsync(JsonConvert.SerializeObject(update, Formatting.None));
				await sw.WriteLineAsync();
				await sw.WriteLineAsync();
			}
		}

		[Route("telegram/hook2")]
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			return new string[] { "value1", "value2" };
		}
	}
}