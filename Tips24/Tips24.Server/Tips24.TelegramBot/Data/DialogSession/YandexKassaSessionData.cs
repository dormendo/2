using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Tips24.TelegramBot
{
	public class YandexKassaSessionData : DialogSessionData
	{
		private class Data
		{
			public string UserLogin { get; set; }

			public decimal? Amount { get; set; }

			public YandexKassaProvider Provider { get; set; }
		}

		private Data _data;

		public string UserLogin
		{
			get { return _data.UserLogin; }
			set { _data.UserLogin = value; }
		}

		public decimal? Amount
		{
			get { return _data.Amount; }
			set { _data.Amount = value; }
		}

		public YandexKassaProvider Provider
		{
			get { return _data.Provider; }
			set { _data.Provider = value; }
		}

		public override DialogType SessionType => DialogType.YandexKassa;

		public YanderKassaSessionStep Step
		{
			get { return (YanderKassaSessionStep)this.StepByte; }
			set { this.StepByte = (byte)value; }
		}

		public string GetFormattedPhone()
		{
			return "+7 (" + this.UserLogin.Substring(1, 3) + ") " + this.UserLogin.Substring(4, 3) + "-" + this.UserLogin.Substring(7, 2) + "-" + this.UserLogin.Substring(9, 2);
		}

		public string GetBankName()
		{
			if (this.Provider == YandexKassaProvider.SberbankOnline)
			{
				return "Сбербанк";
			}
			else if (this.Provider == YandexKassaProvider.AlfaClick)
			{
				return "Альфабанк";
			}
			else
			{
				return "(не задан)";
			}
		}

		public override async Task<bool> CompleteSession(SqlServer sqlServer, Employee employee)
		{
			using (SqlConnection conn = sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = sqlServer.GetSpCommand("telegram.DialogSession_YandexKassa_Complete", conn))
				{
					cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
					cmd.AddIntParam("@PlaceId", employee.Place.Id);
					cmd.AddIntParam("@EmployeeId", employee.Id);
					cmd.AddVarCharParam("@UserLogin", 50, this.UserLogin);
					cmd.AddDecimalParam("@Amount", 18, 2, this.Amount.Value);
					cmd.AddTinyIntParam("@ProviderId", (byte)this.Provider);
					
					SqlParameter retValParam = cmd.AddReturnValue();

					await cmd.ExecuteNonQueryAsync();
					int retVal = retValParam.GetInt32OrDefault();
					return retVal == 0;
				}
			}
		}

		protected override string SerializeToJson()
		{
			return JsonConvert.SerializeObject(this._data);
		}

		public override void DeserializeFromJson(string data)
		{
			this._data = JsonConvert.DeserializeObject<Data>(data);
		}

		public override ReplyKeyboardMarkup GetKeyboardMarkup(Employee employee, object data = null)
		{
			List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
			List<KeyboardButton> cancelRow = new List<KeyboardButton>();

			if (this.Step == YanderKassaSessionStep.AmountEntered)
			{
				cancelRow.Add("Да");
			}
			else if (this.Step == YanderKassaSessionStep.LoginEntered)
			{
				List<KeyboardButton> amountRow1 = new List<KeyboardButton>();
				amountRow1.Add("50");
				amountRow1.Add("100");
				amountRow1.Add("150");
				amountRow1.Add("200");
				rows.Add(amountRow1);

				List<KeyboardButton> amountRow2 = new List<KeyboardButton>();
				amountRow2.Add("250");
				amountRow2.Add("300");
				amountRow2.Add("400");
				amountRow2.Add("500");
				rows.Add(amountRow2);
			}

			cancelRow.Add(new KeyboardButton("Отменить"));
			rows.Add(cancelRow);

			ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(rows, true, true);
			return markup;
		}
	}
}
