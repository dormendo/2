using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public static class DateTimeExtensions
	{
		private static CultureInfo _ruCulture = CultureInfo.GetCultureInfo("ru-RU");

		public static string ToTelegramReportString(this DateTime dt)
		{
			if (dt.Year == DateTime.Now.Year)
			{
				return dt.ToString("dd MMM, HH:mm", _ruCulture);
			}
			else
			{
				return dt.ToString("dd MMM yyyy, HH:mm", _ruCulture);
			}
		}
	}
}
