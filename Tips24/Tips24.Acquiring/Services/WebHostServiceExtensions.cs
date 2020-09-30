using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Tips24.Acquiring
{
	public static class WebHostServiceExtensions
	{
		public static void RunAsAcquiringService(this IWebHost host)
		{
			AcquiringWebHostService webHostService = new AcquiringWebHostService(host);
			ServiceBase.Run(webHostService);
		}
	}
}
