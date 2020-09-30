using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public enum YanderKassaSessionStep : byte
	{
		Initiated = 0,
		ProviderEntered = 1,
		LoginEntered = 2,
		AmountEntered = 3,
		Complete = 4
	}
}
