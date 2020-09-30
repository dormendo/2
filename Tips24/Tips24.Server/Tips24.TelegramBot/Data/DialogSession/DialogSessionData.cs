using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Tips24.TelegramBot
{
	public abstract class DialogSessionData
	{
		public abstract DialogType SessionType { get; }

		public byte StepByte { get; set; }

		public async Task CreateSession(SqlServer sqlServer, Employee employee)
		{
			using (SqlConnection conn = sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = sqlServer.GetSpCommand("telegram.DialogSession_Start", conn))
				{
					cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
					cmd.AddTinyIntParam("@SessionType", (byte)this.SessionType);
					cmd.AddTinyIntParam("@Step", this.StepByte);
					cmd.AddNVarCharParam("@Data", 200, this.SerializeToJson());
					await cmd.ExecuteNonQueryAsync();
				}
			}
		}

		public async Task CancelSession(SqlServer sqlServer, Employee employee)
		{
			using (SqlConnection conn = sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = sqlServer.GetSpCommand("telegram.DialogSession_Cancel", conn))
				{
					cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
					await cmd.ExecuteNonQueryAsync();
				}
			}
		}

		public async Task UpdateSession(SqlServer sqlServer, Employee employee)
		{
			using (SqlConnection conn = sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = sqlServer.GetSpCommand("telegram.DialogSession_Update", conn))
				{
					cmd.AddBigIntParam("@TelegramId", employee.TelegramUserId);
					cmd.AddTinyIntParam("@Step", this.StepByte);
					cmd.AddNVarCharParam("@Data", 200, this.SerializeToJson());
					await cmd.ExecuteNonQueryAsync();
				}
			}
		}

		public abstract Task<bool> CompleteSession(SqlServer sqlServer, Employee employee);

		protected abstract string SerializeToJson();

		public abstract void DeserializeFromJson(string data);

		public abstract ReplyKeyboardMarkup GetKeyboardMarkup(Employee employee, object data = null);
	}
}
