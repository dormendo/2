using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tips24.SmsSender
{
	public static class Starter
	{
		private static CancellationTokenSource _cts;
		private static Task _task;

		public static void Start()
		{
			_cts = new CancellationTokenSource();
			_task = Task.Factory.StartNew(StartInternal);
		}

		private static void StartInternal()
		{
			Sender sender = new Sender(_cts);
			sender.Send();
		}

		public static void Stop()
		{
			_cts.Cancel();
			_task.Wait();
		}
	}
}
