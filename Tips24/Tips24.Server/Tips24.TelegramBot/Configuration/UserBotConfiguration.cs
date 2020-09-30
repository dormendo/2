using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public class UserBotConfiguration : BotConfiguration
	{
		public string TimeToCancelTurns { get; set; }

		public int NotifyInterval { get; set; }
	}
}
