using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Tips24.PayoutService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// NLog: setup the logger first to catch all errors
			NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			try
			{
				bool isService = !Debugger.IsAttached && args.Contains("--service");
				IWebHostBuilder builder = CreateWebHostBuilder(args.Where(arg => arg != "--service").ToArray());

				if (isService)
				{
					string pathToExe = Process.GetCurrentProcess().MainModule.FileName;
					string pathToContentRoot = Path.GetDirectoryName(pathToExe);
					builder.UseContentRoot(pathToContentRoot);
				}

				var host = builder.Build();

				if (isService)
				{
					host.RunAsPayoutService();
				}
				else
				{
					host.Run();
				}

				logger.Debug("init main");
			}
			catch (Exception ex)
			{
				//NLog: catch setup errors
				logger.Error(ex, "Stopped program because of exception");
				throw;
			}
			finally
			{
				logger.Debug("suht down");
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				NLog.LogManager.Shutdown();
			}
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			string pathToExe = Process.GetCurrentProcess().MainModule.FileName;
			string pathToContentRoot = Path.GetDirectoryName(pathToExe);

			return WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.SetMinimumLevel(LogLevel.Trace);
				})
				.ConfigureAppConfiguration((context, config) =>
				{
					// Configure the app here.
				})
				.UseKestrel(options =>
				{
					options.Listen(IPAddress.Any, 5500);
					options.Listen(IPAddress.Any, 5501, o =>
					{
						o.UseHttps(StoreName.My, "api.tips24.ru", false, StoreLocation.CurrentUser);
					});
				})
				.UseNLog();
		}
	}
}