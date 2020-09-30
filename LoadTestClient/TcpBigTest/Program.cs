using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpBigTest
{
	class Program
	{
		static int _started = 0, _connected = 0, _sent = 0, _received = 0, _closed = 0, _errors = 0, _iterations = 0, _port = 0;
		static string _host;
		static async Task Main(string[] args)
		{
			_iterations = Convert.ToInt32(ConfigurationManager.AppSettings["iterations"]);
			_host = ConfigurationManager.AppSettings["host"];
			_port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);

			if (Directory.Exists("Responses"))
			{
				Directory.Delete("Responses", true);
			}
			Directory.CreateDirectory("Responses");

			string headersStr = File.ReadAllText("Headers.txt");
			byte[] headers = Encoding.UTF8.GetBytes(headersStr);

			List<Task> tasks = new List<Task>();
			Stopwatch sw = Stopwatch.StartNew();
			for (int i = 0; i < _iterations; i++)
			{
				tasks.Add(Send(headers, i));
			}

			Task[] taskArray = tasks.ToArray();
			while (!Task.WaitAll(taskArray, 1000))
			{
				Console.WriteLine($"started: {_started}, connected: {_connected}, sent: {_sent}, processed: {_received}, completed: {_closed}, errors: {_errors}");
			}
			sw.Stop();
			Console.WriteLine($"{sw.ElapsedMilliseconds}, started: {_started}, connected: {_connected}, sent: {_sent}, processed: {_received}, completed: {_closed}, errors: {_errors}");
			Console.ReadLine();
		}

		private static async Task Send(byte[] ba, int i)
		{
			Interlocked.Increment(ref _started);
			string pattern = new string('0', (_iterations - 1).ToString().Length);
			string iStr = i.ToString(pattern);
			try
			{
				TcpClient tcpClient = new TcpClient();
				tcpClient.ReceiveBufferSize = 160 * 1024;
				await tcpClient.ConnectAsync(_host, _port);
				Interlocked.Increment(ref _connected);
				using (Stream s = tcpClient.GetStream())
				{

					await s.WriteAsync(ba, 0, ba.Length);
					Interlocked.Increment(ref _sent);
					using (FileStream fs = new FileStream("Responses\\Response" + iStr + ".txt", FileMode.Create, FileAccess.Write))
					{
						await s.CopyToAsync(fs);
					}
					Interlocked.Increment(ref _received);
				}

				tcpClient.Close();
				tcpClient.Dispose();
				Interlocked.Increment(ref _closed);
			}
			catch (Exception e)
			{
				Interlocked.Increment(ref _errors);
				using (FileStream fs = new FileStream("Responses\\Error" + iStr + ".txt", FileMode.Create, FileAccess.Write))
				{
					byte[] eb = Encoding.UTF8.GetBytes(e.ToString());
					fs.Write(eb, 0, eb.Length);
				}
			}
		}
	}
}
