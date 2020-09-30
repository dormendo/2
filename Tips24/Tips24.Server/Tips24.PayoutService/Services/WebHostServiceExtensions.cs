using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Tips24.PayoutService
{
	public static class WebHostServiceExtensions
	{
		public static void RunAsPayoutService(this IWebHost host)
		{
			PayoutWebHostService webHostService = new PayoutWebHostService(host);
			ServiceBase.Run(webHostService);
		}
	}
}
