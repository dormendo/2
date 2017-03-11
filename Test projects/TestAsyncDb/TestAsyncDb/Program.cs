using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestAsyncDb
{
	class Program
	{
		static void Main(string[] args)
		{
			Db.Run();
			Db.RunAsyncAwait().Wait();
			RunSync();
			RunAsyncAwait();
		}

		private static void RunSync()
		{
			ThreadPool.SetMinThreads(200, 200);
			Stopwatch sw = Stopwatch.StartNew();
			List<Task> list = new List<Task>();
			for (int i = 0; i < 200; i++)
			{
				int icopy = i;
				list.Add(Task.Factory.StartNew(() => SyncWorker(icopy)));
			}
			Task.WaitAll(list.ToArray());
			sw.Stop();
			Console.WriteLine("Sync: {0}, {1}", sw.ElapsedMilliseconds, sw.ElapsedTicks);
		}

		private static void SyncWorker(object taskindex)
		{
			//Console.WriteLine("Task #{0} started", taskindex);
			for (int i = 0; i < 200; i++)
			{
				Db.Run();
				MakeCpuWork();
			}
			//Console.WriteLine("Task #{0} completed", taskindex);
		}

		private static void RunAsyncAwait()
		{
			int minthcount, mincpcount, maxthcount, maxcpcount;
			ThreadPool.GetMinThreads(out minthcount, out mincpcount);
			ThreadPool.GetMaxThreads(out maxthcount, out maxcpcount);
			ThreadPool.SetMinThreads(4, 200);
			
			Stopwatch sw = Stopwatch.StartNew();
			List<Task> list = new List<Task>();
			for (int i = 0; i < 200; i++)
			{
				int icopy = i;
				list.Add(AsyncAwaitWorker(icopy));
			}
			Task.WaitAll(list.ToArray());
			sw.Stop();
			Console.WriteLine("AsyncAwait: {0}, {1}", sw.ElapsedMilliseconds, sw.ElapsedTicks);
		}

		private static async Task AsyncAwaitWorker(int taskindex)
		{
			int minthcount, mincpcount, maxthcount, maxcpcount, curthcount, curcpcount;
			ThreadPool.GetMinThreads(out minthcount, out mincpcount);
			ThreadPool.GetMaxThreads(out maxthcount, out maxcpcount);
			ThreadPool.GetAvailableThreads(out curthcount, out curcpcount);
			//Console.WriteLine("Thread #{0}, cur: {0}, {1}", taskindex, curthcount, curcpcount);
			for (int i = 0; i < 200; i++)
			{
				Task t = Db.RunAsyncAwait();
				MakeCpuWork();
				await t.ConfigureAwait(true);
			}
		}

		private static void RunIar()
		{
			int minthcount, mincpcount, maxthcount, maxcpcount;
			ThreadPool.GetMinThreads(out minthcount, out mincpcount);
			ThreadPool.GetMaxThreads(out maxthcount, out maxcpcount);
			ThreadPool.SetMinThreads(4, 200);

			Stopwatch sw = Stopwatch.StartNew();
			List<Task> list = new List<Task>();
			for (int i = 0; i < 200; i++)
			{
				int icopy = i;
				list.Add(Task.Factory.StartNew(() => IarWorker(icopy)));
			}
			Task.WaitAll(list.ToArray());
			sw.Stop();
			Console.WriteLine("IarAsync: {0}, {1}", sw.ElapsedMilliseconds, sw.ElapsedTicks);
		}

		private static void SyncWorker(object taskindex)
		{
			//Console.WriteLine("Task #{0} started", taskindex);
			for (int i = 0; i < 200; i++)
			{
				IAsyncResult iar = Db.RunIar();
				MakeCpuWork();
				iar.AsyncWaitHandle.WaitOne(0, false);
			}
			//Console.WriteLine("Task #{0} completed", taskindex);
		}

		private static void MakeCpuWork()
		{
			Thread.Sleep(0);
		}
	}
}
