using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Tips24.TelegramBot
{
	public class TurnSessionData : DialogSessionData
	{
		public class Employees
		{
			public List<TurnEmployee> InTurn { get; set; }
			public List<TurnEmployee> NotInTurn { get; set; }

			public Employees()
			{
				this.InTurn = new List<TurnEmployee>();
				this.NotInTurn = new List<TurnEmployee>();
			}
		}

		public override DialogType SessionType => DialogType.Turn;

		public TurnSessionStep Step { get; set; }

		public override Task<bool> CompleteSession(SqlServer sqlServer, Employee employee)
		{
			throw new NotImplementedException();
		}

		public override void DeserializeFromJson(string data)
		{
		}

		protected override string SerializeToJson()
		{
			return null;
		}

		public async Task<Employees> GetEmployeesStatus(Employee employee, SqlServer sqlServer)
		{
			Employees employees = new Employees();
			using (SqlConnection conn = sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = sqlServer.GetSpCommand("telegram.Turn_Employees", conn))
				{
					cmd.AddIntParam("@PlaceId", employee.Place.Id);
					cmd.AddIntParam("@EmployeeId", employee.Id);

					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						while (dr.Read())
						{
							TurnEmployee te = new TurnEmployee(dr);

							if (te.IsInTurn)
							{
								employees.InTurn.Add(te);
							}
							else
							{
								employees.NotInTurn.Add(te);
							}
						}
					}
				}
			}

			return employees;
		}

		public override ReplyKeyboardMarkup GetKeyboardMarkup(Employee employee, object data = null)
		{
			List<List<KeyboardButton>> rows = new List<List<KeyboardButton>>();
			List<KeyboardButton> cancelRow = new List<KeyboardButton>();

			if (this.Step == TurnSessionStep.Initiated)
			{
				cancelRow.Add(employee.Turn == null ? "Войти в смену" : "Выйти из смены");
			}

			cancelRow.Add(new KeyboardButton("Назад"));
			rows.Add(cancelRow);

			Employees employees = data as Employees;
			if (employees != null)
			{
				List<string> commands = new List<string>();
				foreach (TurnEmployee te in employees.InTurn)
				{
					commands.Add("-" + te.EmployeeId.ToString());
				}
				foreach (TurnEmployee te in employees.NotInTurn)
				{
					commands.Add("+" + te.EmployeeId.ToString());
				}

				for (int i = 0; i < commands.Count; i += 4)
				{
					List<KeyboardButton> kbList = new List<KeyboardButton>();
					for (int j = i; j < i + 4 && j < commands.Count; j++)
					{
						kbList.Add(commands[j]);
					}
					//if (j < i + 3)
					//{
					//	for (; j < i + 4; j++)
					//	{
					//		kbList.Add("");
					//	}
					//}

					rows.Add(kbList);
				}
			}

			ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(rows, true, true);
			return markup;
		}
	}
}
