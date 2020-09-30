using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Tips24.TelegramBot
{
	public static class WebHostServiceExtensions
	{
		public static void RunAsTelegramService(this IWebHost host)
		{
			TelegramWebHostService webHostService = new TelegramWebHostService(host);
			ServiceBase.Run(webHostService);
		}
	}
}
