using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace Tips24.SmsSender
{
	class Service : ServiceBase
	{
		protected override void OnStart(string[] args)
		{
			base.OnStart(args);

			Starter.Start();
		}

		protected override void OnStop()
		{
			Starter.Stop();
			base.OnStop();
		}
	}
}
