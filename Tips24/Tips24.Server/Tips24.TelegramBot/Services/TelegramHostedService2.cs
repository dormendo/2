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
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Tips24.TelegramBot.Properties;


namespace Tips24.TelegramBot
{
	public partial class TelegramHostedService
	{
		#region Смена

		private async Task StartTurnSession(Update update, Employee employee)
		{
			TurnSessionData session = new TurnSessionData();
			employee.DialogSession = session;
			await employee.DialogSession.CreateSession(_sqlServer, employee);

			StringBuilder sb = new StringBuilder();
			if (employee.Turn == null)
			{
				sb.Append("Вы находитесь вне смены");
			}
			else
			{
				sb.Append("Ваша смена началась ").Append(employee.Turn.BeginDateTime.ToTelegramReportString());
			}

			TurnSessionData.Employees employees = null;
			if (employee.IsManager)
			{
				employees = await session.GetEmployeesStatus(employee, _sqlServer);
				sb.AppendLine().AppendLine();
				GetEmployeesTurnStatusMessageForManager(employees, sb);
			}

			string text = sb.ToString();
			ReplyKeyboardMarkup keyboard = employee.DialogSession.GetKeyboardMarkup(employee, employees);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private void GetEmployeesTurnStatusMessageForManager(TurnSessionData.Employees employees, StringBuilder sb)
		{
			if (employees.InTurn.Count > 0)
			{
				sb.AppendLine("<b>Сотрудники в смене</b> (в скобках команда для вывода из смены):");

				foreach (TurnEmployee te in employees.InTurn)
				{
					sb.Append(te.FirstName).Append(" ").Append(te.LastName).Append(", ").Append(te.GroupName).Append(" (-").Append(te.EmployeeId).AppendLine(")");
				}
			}

			if (employees.NotInTurn.Count > 0)
			{
				sb.AppendLine("<b>Сотрудники вне смены</b> (в скобках команда для ввода в смену):");

				foreach (TurnEmployee te in employees.NotInTurn)
				{
					sb.Append(te.FirstName).Append(" ").Append(te.LastName).Append(", ").Append(te.GroupName).Append(" (+").Append(te.EmployeeId).AppendLine(")");
				}
			}
		}

		private async Task ProcessTurnConversation(Update update, Employee employee)
		{
			TurnSessionStep step = ((TurnSessionData)employee.DialogSession).Step;
			string command = update.Message.Text.Trim().ToLower();

			if (command == "отменить" || command == "назад")
			{
				await this.TurnCancel(update, employee);
			}
			else if (command == "войти" || command == "войти в смену")
			{
				await this.TurnEnter(update, employee);
			}
			else if (command == "выйти" || command == "выйти из смены")
			{
				await this.TurnExit(update, employee);
			}
			else if (step == TurnSessionStep.Initiated)
			{
				await this.TurnEnterExitEmployee(update, employee, command);
			}
		}

		private async Task TurnCancel(Update update, Employee employee)
		{
			await employee.DialogSession.CancelSession(_sqlServer, employee);

			string text = "Ожидаю дальнейших команд";
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private async Task TurnEnter(Update update, Employee employee)
		{
			TurnSessionData session = (TurnSessionData)employee.DialogSession;

			if (employee.Turn == null)
			{
				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.DialogSession_Turn_Enter", conn))
					{
						cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
						cmd.AddIntParam("@EmployeeId", employee.Id);
						cmd.AddIntParam("@PlaceId", employee.Place.Id);
						cmd.AddNVarCharParam("@Message", 200, employee.GetTurnEnterMessage());
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}

			string text = "Вы вошли в смену";
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private async Task TurnExit(Update update, Employee employee)
		{
			TurnSessionData session = (TurnSessionData)employee.DialogSession;

			if (employee.Turn != null)
			{
				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.DialogSession_Turn_Exit", conn))
					{
						cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
						cmd.AddIntParam("@EmployeeId", employee.Id);
						cmd.AddIntParam("@PlaceId", employee.Place.Id);
						cmd.AddNVarCharParam("@Message", 200, employee.GetTurnExitMessage());
						await cmd.ExecuteNonQueryAsync();
					}
				}
			}

			string text = "Вы вышли из смены";
			ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
			await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, keyboard, _cts.Token);
			await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		}

		private async Task TurnEnterExitEmployee(Update update, Employee employee, string command)
		{
			TurnSessionData session = (TurnSessionData)employee.DialogSession;

			bool isCorrect = true;
			if (command.Length < 2 || command[0] != '-' && command[0] != '+')
			{
				isCorrect = false;
			}

			bool isEnter = (command[0] == '+');
			int employeeId = 0;
			if (isCorrect)
			{
				for (int i = 1; i < command.Length; i++)
				{
					if (!char.IsDigit(command[i]))
					{
						isCorrect = false;
						break;
					}
				}

				if (isCorrect)
				{
					if (!int.TryParse(command.Substring(1), out employeeId))
					{
						isCorrect = false;
					}
				}
			}

			int retVal = 0;
			if (isCorrect)
			{
				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.DialogSession_Turn_EnterExitEmployee", conn))
					{
						cmd.AddIntParam("@ManagerId", employee.Id);
						cmd.AddBigIntParam("@ManagerTelegramId", employee.TelegramUserId);
						cmd.AddNVarCharParam("@ManagerName", 101, employee.GetFullName());
						cmd.AddIntParam("@EmployeeId", employeeId);
						cmd.AddIntParam("@PlaceId", employee.Place.Id);
						cmd.AddBitParam("@IsEnter", isEnter);
						SqlParameter retValParam = cmd.AddReturnValue();

						await cmd.ExecuteNonQueryAsync();
						retVal = retValParam.GetInt32OrDefault();
					}
				}

				isCorrect = (retVal >= 0);
			}

			if (isCorrect)
			{
				string text;
				if (retVal == 1)
				{
					text = "Сотрудник уже вошёл в смену";
				}
				else if (retVal == 2)
				{
					text = "Сотрудник уже вышел из смены";
				}
				else
				{
					text = "Команда успешно выполнена";
				}

				ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
				await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, keyboard, _cts.Token);
				await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				if (retVal == -1)
				{
					sb.Append("Вы не можете управлять сменами сотрудников. Нажмите кнопку <b>Назад</b>");
				}
				else if (retVal == -2)
				{
					sb.Append("Сотрудник не принадлежит вашему заведению. Исправьте или нажмите кнопку <b>Назад</b>");
				}
				else
				{
					sb.Append("Вы ввели неизвестную команду. Исправьте или нажмите кнопку <b>Назад</b>");
				}

				TurnSessionData.Employees employees = null;
				if (retVal != -1)
				{
					employees = await session.GetEmployeesStatus(employee, _sqlServer);
					sb.AppendLine().AppendLine();
					this.GetEmployeesTurnStatusMessageForManager(employees, sb);
				}

				string text = sb.ToString();
				ReplyKeyboardMarkup keyboard = employee.DialogSession.GetKeyboardMarkup(employee, employees);
				await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
				await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
			}
		}

		private ReplyKeyboardMarkup GetYandexKassaKeyboardMarkup(YanderKassaSessionStep step)
		{
			List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
			List<KeyboardButton> cancelRow = new List<KeyboardButton>();

			if (step == YanderKassaSessionStep.AmountEntered)
			{
				cancelRow.Add("Да");
			}
			else if (step == YanderKassaSessionStep.LoginEntered)
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

		#endregion

		#region Яндекс.Касса (только Сбербанк)

		//private async Task StartYandexKassaSession(Update update, Employee employee)
		//{
		//	using (SqlConnection conn = _sqlServer.GetConnection())
		//	{
		//		await conn.OpenAsync();

		//		using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.StartYandexKassaSession", conn))
		//		{
		//			cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
		//			await cmd.ExecuteNonQueryAsync();
		//		}
		//	}

		//	employee.YandexKassa = new YandexKassaSession();

		//	string text = "Пользователь Сбербанк.Онлайн может оставить Вам чаевые, подтвердив запрос по СМС. <b>Введите номер телефона гостя</b>";
		//	ReplyKeyboardMarkup keyboard = GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step);
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
		//	await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		//}

		//private async Task ProcessYandexKassaConversation(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower();
		//	if (command == "отменить")
		//	{
		//		await this.YandexKassaCancel(update, employee);
		//	}
		//	else if (employee.YandexKassa.Step == YanderKassaSessionStep.Initiated)
		//	{
		//		await this.YandexKassaSberbankPhoneEntered(update, employee);
		//	}
		//	else if (employee.YandexKassa.Step == YanderKassaSessionStep.LoginEntered)
		//	{
		//		await this.YandexKassaAmountEntered(update, employee);
		//	}
		//	else if (employee.YandexKassa.Step == YanderKassaSessionStep.AmountEntered)
		//	{
		//		await this.YandexKassaCheckInformation(update, employee);
		//	}
		//}

		//private async Task YandexKassaCancel(Update update, Employee employee)
		//{
		//	using (SqlConnection conn = _sqlServer.GetConnection())
		//	{
		//		await conn.OpenAsync();

		//		using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.CancelYandexKassaSession", conn))
		//		{
		//			cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
		//			await cmd.ExecuteNonQueryAsync();
		//		}
		//	}

		//	string text = "Запрос на выплату чаевых отменён";
		//	ReplyKeyboardMarkup keyboard = GetStandardKeyboardMarkup(employee);
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, keyboard, _cts.Token);
		//	await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		//}

		//private async Task YandexKassaSberbankPhoneEntered(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower();
		//	StringBuilder sb = new StringBuilder();
		//	for (int i = 0; i < command.Length; i++)
		//	{
		//		char ch = command[i];
		//		if (i == 0 && ch == '+' || char.IsDigit(ch))
		//		{
		//			sb.Append(ch);
		//		}
		//	}

		//	string phone = sb.ToString();
		//	if (phone.StartsWith("+7"))
		//	{
		//		phone = phone.Substring(2);
		//	}
		//	else if (phone.StartsWith("8"))
		//	{
		//		phone = phone.Substring(1);
		//	}

		//	if (phone.Length != 10)
		//	{
		//		string text1 = "Вы ввели некорректный номер телефона. <b>Введите номер телефона гостя</b>";
		//		ReplyKeyboardMarkup keyboard1 = GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step);
		//		await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text1, ParseMode.Html, false, false, 0, keyboard1, _cts.Token);
		//		await this.WriteMessageLog(new MessageLog(text1, employee, keyboard1));
		//		return;
		//	}

		//	employee.YandexKassa.Provider = YandexKassaProvider.SberbankOnline;
		//	employee.YandexKassa.UserLogin = "7" + phone;
		//	employee.YandexKassa.Step = YanderKassaSessionStep.LoginEntered;
		//	await this.UpdateYandexKassaSession(employee);

		//	string text = "Телефон гостя: " + employee.YandexKassa.GetFormattedPhone() + ". <b>Введите сумму чаевых (рубли без копеек)</b>";
		//	ReplyKeyboardMarkup keyboard = GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step);
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
		//	await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		//}

		//private async Task YandexKassaAmountEntered(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower().Replace(',', '.');
		//	if (!decimal.TryParse(command, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0M)
		//	{
		//		string text1 = "Вы ввели некорректную сумму. <b>Введите сумму чаевых (рубли без копеек)</b>";
		//		ReplyKeyboardMarkup keyboard1 = GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step);
		//		await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text1, ParseMode.Html, false, false, 0, keyboard1, _cts.Token);
		//		await this.WriteMessageLog(new MessageLog(text1, employee, keyboard1));
		//		return;
		//	}

		//	employee.YandexKassa.Amount = amount;
		//	employee.YandexKassa.Step = YanderKassaSessionStep.AmountEntered;
		//	await this.UpdateYandexKassaSession(employee);

		//	string text = "С согласия гостя с номером телефона <b>" + employee.YandexKassa.GetFormattedPhone() + "</b> Вы предлагаете гостю оставить Вам <b>" +
		//		employee.YandexKassa.Amount.Value.ToString(_ruCulture) + " рублей</b> в качестве чаевых с помощью СМС-счёта, отправленного через Сбербанк.Онлайн. Всё верно?";
		//	ReplyKeyboardMarkup keyboard = GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step);
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Html, false, false, 0, keyboard, _cts.Token);
		//	await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		//}

		//private async Task YandexKassaCheckInformation(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower();
		//	if (command != "да")
		//	{
		//		string text1 = "С согласия гостя с номером телефона <b>" + employee.YandexKassa.GetFormattedPhone() + "</b> Вы предлагаете гостю оставить Вам <b>" +
		//			employee.YandexKassa.Amount.Value.ToString(_ruCulture) +
		//			" рублей</b> в качестве чаевых с помощью СМС-счёта, отправленного через Сбербанк.Онлайн. Всё верно? Нажмите кнопку \"Да\" либо кнопку \"Отменить\"";
		//		ReplyKeyboardMarkup keyboard1 = GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step);
		//		await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text1, ParseMode.Html, false, false, 0, keyboard1, _cts.Token);
		//		await this.WriteMessageLog(new MessageLog(text1, employee, keyboard1));
		//		return;
		//	}

		//	await this.CompleteYandexKassaSession(employee);

		//	string text = "Запрос на выплату чаевых успешно отправлен";
		//	ReplyKeyboardMarkup keyboard = GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step);
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text, ParseMode.Default, false, false, 0, keyboard, _cts.Token);
		//	await this.WriteMessageLog(new MessageLog(text, employee, keyboard));
		//}

		//private async Task<bool> UpdateYandexKassaSession(Employee employee)
		//{
		//	using (SqlConnection conn = this._sqlServer.GetConnection())
		//	{
		//		await conn.OpenAsync();

		//		using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.UpdateYandexKassaSession", conn))
		//		{
		//			cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
		//			cmd.AddVarCharParam("@UserLogin", 50, employee.YandexKassa.UserLogin);
		//			cmd.AddDecimalParam("@Amount", 18, 2, employee.YandexKassa.Amount);
		//			cmd.AddTinyIntParam("@ProviderId", (byte)employee.YandexKassa.Provider);
		//			cmd.AddTinyIntParam("@Step", (byte)employee.YandexKassa.Step);
		//			SqlParameter retValParam = cmd.AddReturnValue();

		//			await cmd.ExecuteNonQueryAsync();
		//			int retVal = retValParam.GetInt32OrDefault();
		//			return retVal == 0;
		//		}
		//	}
		//}

		//private async Task<bool> CompleteYandexKassaSession(Employee employee)
		//{
		//	using (SqlConnection conn = this._sqlServer.GetConnection())
		//	{
		//		await conn.OpenAsync();

		//		using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.CompleteYandexKassaSession", conn))
		//		{
		//			cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
		//			cmd.AddIntParam("@PlaceId", employee.PlaceId);
		//			cmd.AddIntParam("@EmployeeId", employee.Id);
		//			SqlParameter retValParam = cmd.AddReturnValue();

		//			await cmd.ExecuteNonQueryAsync();
		//			int retVal = retValParam.GetInt32OrDefault();
		//			return retVal == 0;
		//		}
		//	}
		//}

		//private ReplyKeyboardMarkup GetYandexKassaKeyboardMarkup(YanderKassaSessionStep step)
		//{
		//	List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
		//	List<KeyboardButton> cancelRow = new List<KeyboardButton>();

		//	if (step == YanderKassaSessionStep.AmountEntered)
		//	{
		//		cancelRow.Add("Да");
		//	}
		//	else if (step == YanderKassaSessionStep.LoginEntered)
		//	{
		//		List<KeyboardButton> amountRow1 = new List<KeyboardButton>();
		//		amountRow1.Add("50");
		//		amountRow1.Add("100");
		//		amountRow1.Add("150");
		//		amountRow1.Add("200");
		//		rows.Add(amountRow1);

		//		List<KeyboardButton> amountRow2 = new List<KeyboardButton>();
		//		amountRow2.Add("250");
		//		amountRow2.Add("300");
		//		amountRow2.Add("400");
		//		amountRow2.Add("500");
		//		rows.Add(amountRow2);
		//	}

		//	cancelRow.Add(new KeyboardButton("Отменить"));
		//	rows.Add(cancelRow);

		//	ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(rows, true, true);
		//	return markup;
		//}

		#endregion

		#region Яндекс.Касса (c Альфа-клик)

		//private async Task StartYandexKassaSession(Update update, Employee employee)
		//{
		//	using (SqlConnection conn = _sqlServer.GetConnection())
		//	{
		//		await conn.OpenAsync();

		//		using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.StartYandexKassaSession", conn))
		//		{
		//			cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
		//			await cmd.ExecuteNonQueryAsync();
		//		}
		//	}

		//	employee.YandexKassa = new YandexKassaSession();

		//	string text = "Пользователь Сбербанк.Онлайн и Альфа-Клик может оставить Вам чаевые, подтвердив запрос по СМС. <b>Введите номер телефона гостя</b>";
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text,
		//		ParseMode.Html, false, false, 0, this.GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step), _cts.Token);
		//}

		//private async Task ProcessYandexKassaConversation(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower();
		//	if (command == "отменить")
		//	{
		//		await this.YandexKassaCancel(update, employee);
		//	}
		//	else if (employee.YandexKassa.Step == YanderKassaSessionStep.Initiated)
		//	{
		//		await this.YandexKassaPhoneEntered(update, employee);
		//	}
		//	else if (employee.YandexKassa.Step == YanderKassaSessionStep.PhoneEntered)
		//	{
		//		await this.YandexKassaAmountEntered(update, employee);
		//	}
		//	else if (employee.YandexKassa.Step == YanderKassaSessionStep.AmountEntered)
		//	{
		//		await this.YandexKassaProviderEntered(update, employee);
		//	}
		//	else if (employee.YandexKassa.Step == YanderKassaSessionStep.ProviderEntered)
		//	{
		//		await this.YandexKassaCheckInformation(update, employee);
		//	}
		//}

		//private async Task YandexKassaCancel(Update update, Employee employee)
		//{
		//	using (SqlConnection conn = _sqlServer.GetConnection())
		//	{
		//		await conn.OpenAsync();

		//		using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.CancelYandexKassaSession", conn))
		//		{
		//			cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
		//			await cmd.ExecuteNonQueryAsync();
		//		}
		//	}

		//	string text2 = "Запрос на выплату чаевых отменён";
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text2, ParseMode.Default, false, false, 0, this.GetCommandsKeyboardMarkup(employee), _cts.Token);
		//	return;
		//}

		//private async Task YandexKassaPhoneEntered(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower();
		//	StringBuilder sb = new StringBuilder();
		//	for (int i = 0; i < command.Length; i++)
		//	{
		//		char ch = command[i];
		//		if (i == 0 && ch == '+' || char.IsDigit(ch))
		//		{
		//			sb.Append(ch);
		//		}
		//	}

		//	string phone = sb.ToString();
		//	if (phone.StartsWith("+7"))
		//	{
		//		phone = phone.Substring(2);
		//	}
		//	else if (phone.StartsWith("8"))
		//	{
		//		phone = phone.Substring(1);
		//	}

		//	if (phone.Length != 10)
		//	{
		//		string text1 = "Вы ввели некорректный номер телефона. <b>Введите номер телефона гостя</b>";
		//		await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text1,
		//			ParseMode.Html, false, false, 0, this.GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step), _cts.Token);
		//		return;
		//	}

		//	employee.YandexKassa.Phone = phone;
		//	employee.YandexKassa.Step = YanderKassaSessionStep.PhoneEntered;
		//	await this.UpdateYandexKassaSession(employee);

		//	string text2 = "Телефон гостя: " + employee.YandexKassa.GetFormattedPhone() + ". <b>Введите сумму чаевых (рубли без копеек)</b>";
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text2,
		//		ParseMode.Html, false, false, 0, this.GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step), _cts.Token);
		//	return;
		//}

		//private async Task YandexKassaAmountEntered(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower().Replace(',', '.');
		//	if (!decimal.TryParse(command, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0M)
		//	{
		//		string text1 = "Вы ввели некорректную сумму. <b>Введите сумму чаевых (рубли без копеек)</b>";
		//		await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text1,
		//			ParseMode.Html, false, false, 0, this.GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step), _cts.Token);
		//		return;
		//	}

		//	employee.YandexKassa.Amount = amount;
		//	employee.YandexKassa.Step = YanderKassaSessionStep.AmountEntered;
		//	await this.UpdateYandexKassaSession(employee);

		//	string text2 = "Сумма чаевых: " + employee.YandexKassa.Amount.Value.ToString(_ruCulture) + " рублей. <b>Выберите банк</b>";
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text2,
		//		ParseMode.Html, false, false, 0, this.GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step), _cts.Token);
		//	return;
		//}

		//private async Task YandexKassaProviderEntered(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower();
		//	YandexKassaProvider provider = YandexKassaProvider.Default;
		//	if (command == "сбербанк")
		//	{
		//		provider = YandexKassaProvider.SberbankOnline;
		//	}
		//	else if (command == "альфабанк")
		//	{
		//		provider = YandexKassaProvider.AlfaClick;
		//	}

		//	if (provider == YandexKassaProvider.Default)
		//	{
		//		string text1 = "Вы ввели некорректное название банка <b>Выберите банк</b>";
		//		await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text1,
		//			ParseMode.Html, false, false, 0, this.GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step), _cts.Token);
		//		return;
		//	}

		//	employee.YandexKassa.Provider = provider;
		//	employee.YandexKassa.Step = YanderKassaSessionStep.ProviderEntered;
		//	await this.UpdateYandexKassaSession(employee);

		//	string text2 = "С согласия гостя с номером телефона <b>" + employee.YandexKassa.GetFormattedPhone() + "</b> Вы предлагаете гостю оставить Вам <b>" +
		//		employee.YandexKassa.Amount.Value.ToString(_ruCulture) + " рублей</b> в качестве чаевых с помощью СМС-счёта, отправленного через <b>" +
		//		employee.YandexKassa.GetBankName() + "</b>. Всё верно?";

		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text2,
		//		ParseMode.Html, false, false, 0, this.GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step), _cts.Token);
		//	return;
		//}

		//private async Task YandexKassaCheckInformation(Update update, Employee employee)
		//{
		//	string command = update.Message.Text.Trim().ToLower();
		//	if (command != "да")
		//	{
		//		string text1 = "С согласия гостя с номером телефона <b>" + employee.YandexKassa.GetFormattedPhone() + "</b> Вы предлагаете гостю оставить Вам <b>" +
		//			employee.YandexKassa.Amount.Value.ToString(_ruCulture) + " рублей</b> в качестве чаевых с помощью СМС-счёта, отправленного через <b>" +
		//			employee.YandexKassa.GetBankName() + "</b>. Всё верно? Нажмите кнопку \"Да\" либо кнопку \"Отменить\"";
		//		await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text1,
		//			ParseMode.Html, false, false, 0, this.GetYandexKassaKeyboardMarkup(employee.YandexKassa.Step), _cts.Token);
		//		return;
		//	}

		//	await this.CompleteYandexKassaSession(employee);

		//	string text2 = "Запрос на выплату чаевых успешно отправлен";
		//	await _telegramClient.SendTextMessageAsync(update.Message.From.Id, text2, ParseMode.Default, false, false, 0, this.GetCommandsKeyboardMarkup(employee), _cts.Token);
		//	return;
		//}

		//private async Task<bool> UpdateYandexKassaSession(Employee employee)
		//{
		//	using (SqlConnection conn = this._sqlServer.GetConnection())
		//	{
		//		await conn.OpenAsync();

		//		using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.UpdateYandexKassaSession", conn))
		//		{
		//			cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
		//			cmd.AddCharParam("@Phone", 10, employee.YandexKassa.Phone);
		//			cmd.AddDecimalParam("@Amount", 18, 2, employee.YandexKassa.Amount);
		//			cmd.AddTinyIntParam("@ProviderId", (byte)employee.YandexKassa.Provider);
		//			cmd.AddTinyIntParam("@Step", (byte)employee.YandexKassa.Step);
		//			SqlParameter retValParam = cmd.AddReturnValue();

		//			await cmd.ExecuteNonQueryAsync();
		//			int retVal = retValParam.GetInt32OrDefault();
		//			return retVal == 0;
		//		}
		//	}
		//}

		//private async Task<bool> CompleteYandexKassaSession(Employee employee)
		//{
		//	using (SqlConnection conn = this._sqlServer.GetConnection())
		//	{
		//		await conn.OpenAsync();

		//		using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.CompleteYandexKassaSession", conn))
		//		{
		//			cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
		//			cmd.AddIntParam("@PlaceId", employee.PlaceId);
		//			cmd.AddIntParam("@EmployeeId", employee.Id);
		//			SqlParameter retValParam = cmd.AddReturnValue();

		//			await cmd.ExecuteNonQueryAsync();
		//			int retVal = retValParam.GetInt32OrDefault();
		//			return retVal == 0;
		//		}
		//	}
		//}

		//private ReplyKeyboardMarkup GetYandexKassaKeyboardMarkup(YanderKassaSessionStep step)
		//{
		//	List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
		//	List<KeyboardButton> cancelRow = new List<KeyboardButton>();

		//	if (step == YanderKassaSessionStep.AmountEntered)
		//	{
		//		List<KeyboardButton> bankRow = new List<KeyboardButton>();
		//		bankRow.Add("Сбербанк");
		//		bankRow.Add("Альфабанк");
		//		rows.Add(bankRow);
		//	}
		//	else if (step == YanderKassaSessionStep.PhoneEntered)
		//	{
		//		List<KeyboardButton> amountRow1 = new List<KeyboardButton>();
		//		amountRow1.Add("50");
		//		amountRow1.Add("100");
		//		amountRow1.Add("150");
		//		amountRow1.Add("200");
		//		rows.Add(amountRow1);

		//		List<KeyboardButton> amountRow2 = new List<KeyboardButton>();
		//		amountRow2.Add("250");
		//		amountRow2.Add("300");
		//		amountRow2.Add("400");
		//		amountRow2.Add("500");
		//		rows.Add(amountRow2);
		//	}
		//	else if (step == YanderKassaSessionStep.ProviderEntered)
		//	{
		//		cancelRow.Add("Да");
		//	}

		//	cancelRow.Add(new KeyboardButton("Отменить"));
		//	rows.Add(cancelRow);

		//	ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(rows, true, true);
		//	return markup;
		//}

		#endregion
	}
}
