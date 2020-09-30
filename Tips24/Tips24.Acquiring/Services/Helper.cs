using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring
{
	public static class Helper
	{
		private static string PurposeCodePrefix = "tips24.ru/";

		public static (int, int?) ParsePurposeCode(string purpose)
		{
			int lastSpaceIndex = purpose.LastIndexOf(' ');
			string receiver = purpose.Substring(lastSpaceIndex + 1, purpose.Length - lastSpaceIndex - 1);
			int dashIndex = receiver.IndexOf('-');

			string placeStr = null, employeeStr = null;
			if (dashIndex < 0)
			{
				placeStr = receiver;
			}
			else
			{
				placeStr = receiver.Substring(0, dashIndex);
				employeeStr = receiver.Substring(dashIndex + 1, receiver.Length - dashIndex - 1);
			}

			if (placeStr.StartsWith(PurposeCodePrefix, StringComparison.OrdinalIgnoreCase))
			{
				placeStr = placeStr.Substring(PurposeCodePrefix.Length);
			}

			int placeId = int.Parse(placeStr);
			int? employeeId = null;
			if (employeeStr != null)
			{
				employeeId = int.Parse(employeeStr);
			}

			return (placeId, employeeId);
		}

		public static void SaveDiagMessage(SqlServer sqlServer, DiagOptions o, string message, ILogger logger)
		{
			if (o <= 0 || sqlServer == null || string.IsNullOrEmpty(message))
			{
				return;
			}

			try
			{
				using (SqlConnection conn = sqlServer.GetConnection())
				{
					conn.Open();
					using (SqlCommand cmd = sqlServer.GetSpCommand("admin.DiagMessage_Save", conn))
					{
						cmd.AddIntParam("@Options", (int)o);
						cmd.AddNVarCharMaxParam("@Message", message);
						cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				if (logger != null)
				{
					logger.LogError(ex, "Ошибка при добавлении диагностического сообщения");
				}
			}
		}
	}

	/// <summary>
	/// Расширения класса System.Enum
	/// </summary>
	public static class EnumExtensions
	{
		/// <summary>
		/// Проверяет, равно ли значение экземпляра перечисления одному из предоставленных значений
		/// </summary>
		/// <param name="e">Экезмпляр</param>
		/// <param name="test">Тестовые значения</param>
		/// <returns>true, если значение равно одному из тестовых значений, иначе false</returns>
		public static bool IsAny(this Enum e, params Enum[] test)
		{
			foreach (Enum testval in test)
			{
				if (testval.Equals(e))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Проверяет, включает ли значение экземпляра перечисления один из предоставленных наборов битовых флагов
		/// </summary>
		/// <param name="e">Экземпляр</param>
		/// <param name="test">Тестовые значения битовых флагов</param>
		/// <returns>true, если значение включает один из наборов битовых флагов, иначе false</returns>
		public static bool HasAnyFlag(this Enum e, params Enum[] test)
		{
			foreach (Enum testval in test)
			{
				if (e.HasFlag(testval))
				{
					return true;
				}
			}
			return false;
		}
	}

	[Flags]
	public enum DiagOptions
	{
		Biz = 1,
		Tech = 2
	}
}
