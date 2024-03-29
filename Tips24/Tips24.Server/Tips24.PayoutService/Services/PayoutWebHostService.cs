﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tips24.PayoutService
{
	internal class PayoutWebHostService : WebHostService
	{
		private ILogger _logger;

		public PayoutWebHostService(IWebHost host) : base(host)
		{
			_logger = host.Services
				.GetRequiredService<ILogger<PayoutWebHostService>>();
		}

		protected override void OnStarting(string[] args)
		{
			_logger.LogDebug("OnStarting method called.");
			base.OnStarting(args);
		}

		protected override void OnStarted()
		{
			_logger.LogDebug("OnStarted method called.");
			base.OnStarted();
		}

		protected override void OnStopping()
		{
			_logger.LogDebug("OnStopping method called.");
			base.OnStopping();
		}
	}
}
