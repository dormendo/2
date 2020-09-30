using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public class Configuration
	{
		public string SslCertPath { get; set; }

		public string PfxPassword { get; set; }

		public UserBotConfiguration UserBot { get; set; }

		public BotConfiguration DiagBot { get; set; }
	}
}
