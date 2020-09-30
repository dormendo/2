using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTestClientCore
{
	class RequestData
	{
		public string Uri { get; set; }

		public string Content { get; set; }
	}

	class Program
	{
		private static void TestJson()
		{
			string json = File.ReadAllText("Config.json");
			Config config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(json);
		}

		private static void TestSampleRate()
		{
			double sampleRate = 97.11111;
			int[] source = new int[10000];
			List<int> traced = new List<int>();
			List<int> notTraced = new List<int>();

			for (int i = 0; i < source.Length; i++)
			{
				source[i] = i + 1;
			}

			for (int i = 0; i < source.Length; i++)
			{
				double rateIfChosen = (traced.Count + 1) / (i + 1) * 100.0;
				double rateIfNotChosen = traced.Count / (i + 1) * 100.0;
				double deltaIfChosen = Math.Abs(rateIfChosen - sampleRate);
				double deltaIfNotChosen = Math.Abs(rateIfNotChosen - sampleRate);
				if (deltaIfNotChosen < deltaIfChosen)
				{
					notTraced.Add(source[i]);
				}
				else
				{
					traced.Add(source[i]);
				}
			}

			Console.WriteLine($"{sampleRate}, {((double)traced.Count) / source.Length * 100}");
		}

		private static int _errors = 0;
		private static int _started = 0, _sent = 0, _processed = 0, _completed = 0;

		static async Task Main(string[] args)
		{
			int minwt, mincp, maxwt, maxcp, startwt, startcp, finishwt, finishcp;
			ThreadPool.GetMinThreads(out minwt, out mincp);
			ThreadPool.GetMaxThreads(out maxwt, out maxcp);
			ThreadPool.GetAvailableThreads(out startwt, out startcp);
			ThreadPool.SetMinThreads(1000, 1000);
			TestJson();
			TestSampleRate();

			RequestData requestData = AcquireRequestData();
			if (requestData == null)
			{
				Console.WriteLine("Задайте корректный информацию о запросе в файле Request.txt ");
				return;
			}

			if (Directory.Exists("Responses"))
			{
				Directory.Delete("Responses", true);
			}
			Directory.CreateDirectory("Responses");


			ServicePointManager.DefaultConnectionLimit = 64;
			ServicePointManager.ReusePort = true;
			ServicePoint sp = ServicePointManager.FindServicePoint(new Uri("http://nsidemo"));
			HttpClientHandler handler = new HttpClientHandler() { MaxConnectionsPerServer = 64 };
			Stopwatch sw = Stopwatch.StartNew();
			using (HttpClient client = new HttpClient(handler))
			{
				client.Timeout = TimeSpan.FromMinutes(60);
				List<Task> tasks = new List<Task>();
				for (int i = 0; i < 64; i++)
				{
					tasks.Add(RunRequest(requestData, client, i));
				}

				Task[] taskArray = tasks.ToArray();
				while (!Task.WaitAll(taskArray, 1000))
				{
					Console.WriteLine($"started: {_started}, sent: {_sent}, processed: {_processed}, completed: {_completed}, errors: {_errors}");
				}
			}
			sw.Stop();

			ThreadPool.GetAvailableThreads(out finishwt, out finishcp);
			Console.WriteLine($"{sw.ElapsedMilliseconds}, errors:{_errors}, wthreads: {minwt}/{maxwt}/{startwt}/{finishwt}, cports: {mincp}/{maxcp}/{startcp}/{finishcp}");
			Console.ReadLine();
		}

		private static async Task RunRequest(RequestData requestData, HttpClient client, int index)
		{
			for (int i = 0; i < 157; i++)
			{
				Interlocked.Increment(ref _started);
				string strIndex = index.ToString("00000");
				try
				{
					using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestData.Uri))
					{
						request.Version = new Version("1.1");
						StringContent content = new StringContent(requestData.Content, Encoding.UTF8, "application/soap+xml");
						request.Content = content;
						Task<HttpResponseMessage> sendTask = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
						Interlocked.Increment(ref _sent);
						using (HttpResponseMessage response = await sendTask)
						{
							Interlocked.Increment(ref _processed);
							//using (Stream rStream = await response.Content.ReadAsStreamAsync())
							//{
							//	//using (FileStream fs = new FileStream("Responses\\Response" + strIndex + ".txt", FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024/*, FileOptions.WriteThrough*/))
							//	//{
							//	//	rStream.CopyTo(fs);
							//	//}
							//	//using (MemoryStream ms = new MemoryStream())
							//	//{
							//	//	rStream.CopyTo(ms);
							//	//}
							//}
						}
					}
					Interlocked.Increment(ref _completed);
				}
				catch (Exception e)
				{
					Interlocked.Increment(ref _errors);
					using (StreamWriter sw = new StreamWriter("Responses\\Error" + strIndex + ".txt"))
					{
						sw.Write(e.ToString());
					}
				}
				//Console.WriteLine($"{index} completed");
			}
		}

		private static RequestData AcquireRequestData()
		{
			using (StreamReader sr = new StreamReader("Request.txt"))
			{
				string uriLine = sr.ReadLine();
				if (string.IsNullOrWhiteSpace(uriLine))
				{
					return null;
				}

				string content = sr.ReadToEnd();
				if (string.IsNullOrWhiteSpace(content))
				{
					return null;
				}

				int index = content.IndexOf("Guid.NewGuid()");
				if (index >= 0)
				{
					content = content.Substring(0, index) + Guid.NewGuid().ToString() + content.Substring(index + "Guid.NewGuid()".Length);
				}

				return new RequestData { Uri = uriLine.Trim(), Content = content.Trim() };
			}
		}
	}
}
