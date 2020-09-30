using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Tips24.TelegramBot
{
	public class MessageLog
	{
		public static class Types
		{
			public const string Authorize = "authorize";
			public const string NotRegistered = "not-registered";
			public const string Contact = "contact";
			public const string InvalidCommand = "invalid-command";
			public const string QrCode = "qr-code";
		}

		public bool InputMessage { get; set; }
		public long TelegramId { get; set; }
		public string Phone { get; set; }
		public int? EmployeeId { get; set; }
		public string MessageData { get; set; }
		public string Type { get; set; }
		public IEnumerable<IEnumerable<KeyboardButton>> Keyboard { get; set; }

		public MessageLog(Update update, Employee employee)
		{
			this.InputMessage = true;
			this.TelegramId = update.Message.From.Id;
			this.MessageData = (update.Message.Contact == null ? update.Message.Text : null);
			this.Type = (update.Message.Contact == null ? null : Types.Contact);

			if (employee != null)
			{
				this.Phone = employee.Phone;
				this.EmployeeId = employee.Id;
			}
		}

		public MessageLog(string message, Employee employee, ReplyKeyboardMarkup keyboard)
		{
			this.TelegramId = employee.TelegramUserId;
			this.Phone = employee.Phone;
			this.EmployeeId = employee.Id;
			this.MessageData = message;
			this.Keyboard = keyboard.Keyboard;
		}

		public MessageLog(Employee employee, string type, ReplyKeyboardMarkup keyboard)
			: this((string)null, employee, keyboard)
		{
			this.Type = type;
		}

		public MessageLog(string type, long telegramId)
		{
			this.TelegramId = telegramId;
			this.Type = type;
		}

		public string GetJson()
		{
			StringBuilder json = new StringBuilder();
			using (StringWriter sw = new StringWriter(json))
			{
				using (JsonTextWriter jw = new JsonTextWriter(sw))
				{
					jw.Formatting = Formatting.None;
					jw.WriteStartObject();

					if (this.Type != null)
					{
						jw.WritePropertyName("t");
						jw.WriteValue(this.Type);
					}

					if (this.MessageData != null)
					{
						jw.WritePropertyName("m");
						jw.WriteValue(this.MessageData);
					}

					this.SetExtendedJsonProperties(jw);

					if (this.Keyboard != null)
					{
						jw.WritePropertyName("k");
						jw.WriteStartArray();
						foreach (IEnumerable<KeyboardButton> row in this.Keyboard)
						{
							jw.WriteStartArray();
							foreach (KeyboardButton button in row)
							{
								if (button.RequestContact)
								{
									jw.WriteValue("/contact");
								}
								else if (button.RequestLocation)
								{
									jw.WriteValue("/location");
								}
								else
								{
									jw.WriteValue(button.Text);
								}
							}

							jw.WriteEndArray();
						}

						jw.WriteEndArray();
					}

					jw.WriteEndObject();
				}
			}

			return json.ToString();
		}

		protected virtual void SetExtendedJsonProperties(JsonTextWriter jw)
		{
		}
	}
}
