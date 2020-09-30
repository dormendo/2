using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestBlobs
{
	class Program
	{
		const int SIZE = 100 * 1024 * 1024;

		static int _started = 0, _finished = 0, _errors = 0;

		static void Main(string[] args)
		{
			if (Directory.Exists("Files"))
			{
				Directory.Delete("Files", true);
			}
			Directory.CreateDirectory("Files");

			byte[] source = new byte[SIZE];
			for (int i = 0; i < SIZE / 16; i++)
			{
				Guid.NewGuid().ToByteArray().CopyTo(source, i * 16);
			}

			Stopwatch sw = Stopwatch.StartNew();
			Task[] tasks = new Task[100];
			for (int i = 0; i < 100; i++)
			{
				tasks[i] = Copy(source, i);
			}

			while (!Task.WaitAll(tasks, 1000))
			{
				Console.WriteLine($"{sw.ElapsedMilliseconds}, started: {_started}, finished: {_finished}, errors: {_errors}");
			}
			sw.Stop();
			Console.WriteLine($"{sw.ElapsedMilliseconds}, started: {_started}, finished: {_finished}, errors: {_errors}");
			Console.ReadLine();
		}

		private static async Task Copy(byte[] source, int i)
		{
			Interlocked.Increment(ref _started);
			using (FileStream fs = new FileStream("Files\\File" + i.ToString("00") + ".bin", FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.None))
			{
				await fs.WriteAsync(source, 0, source.Length);
			}
			Interlocked.Increment(ref _finished);
		}
	}
}
