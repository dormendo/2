using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Tips24.PaymentService
{
	public static class WebHostServiceExtensions
	{
		public static void RunAsPaymentService(this IWebHost host)
		{
			PaymentWebHostService webHostService = new PaymentWebHostService(host);
			ServiceBase.Run(webHostService);
		}
	}
}
