using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Geolocation.Data;
using Newtonsoft.Json.Linq;

namespace Geolocation.Test
{
	unsafe class Program
	{

		[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
		internal static extern void CopyMemory(void* dest, void* src, int count);

		static unsafe void Main(string[] args)
		{
			int t1, t2;
			ThreadPool.GetMinThreads(out t1, out t2);
			ThreadPool.SetMinThreads(Environment.ProcessorCount, t2);
			Console.WriteLine(Environment.ProcessorCount);

			string dbFile = ConfigurationManager.AppSettings["DatabaseFile"];
			bool fastAvailability = Convert.ToBoolean(ConfigurationManager.AppSettings["DatabaseAvailableFast"]);
			Stopwatch sw = Stopwatch.StartNew();
			DataProvider provider = new DataProvider(dbFile, fastAvailability);
			provider.Initialize();
			sw.Stop();
			Console.WriteLine(sw.Elapsed.TotalMilliseconds);

			provider.GetLocationsByCity("cit_A Ar Cy Afik");

			string[] cities = provider.GetAllCities();
			HashSet<string> citySet = new HashSet<string>(cities);
			int locationsFound = 0;
			for (int i = 0; i < cities.Length; i++)
			{
				string city = cities[i];
				byte[] result = null;
				try
				{
					result = provider.GetLocationsByCity(city);
				}
				catch
				{
					Console.WriteLine("ex: " + i.ToString() + ", \"" + city + "\"");
				}
				if (result == null)
				{
					Console.WriteLine("null" + i.ToString() + ", \"" + city + "\"");
				}
				string strResult = Encoding.UTF8.GetString(result);
				JArray a = JArray.Parse(strResult);
				int count = a.Count;
				int correctCount = a.Where(x => x["city"].ToString() == city).Count();
				if (count != correctCount)
				{
					throw new Exception();
				}
				locationsFound += correctCount;
			}

			Console.WriteLine("Locations found: " + locationsFound);

			if (locationsFound != 100000)
			{
				throw new Exception();
			}
			Console.WriteLine("Все города найдены");
			for (int i = 0; i < cities.Length; i++)
			{
				string city = cities[i].Substring(0, cities[i].Length - 1) + "1";
				byte[] result = null;
				if (result != null)
				{
					Console.WriteLine("not null" + i.ToString() + ", \"" + city + "\"");
				}
			}
			Console.WriteLine("Лишние города не найдены");

			const int ITERATIONS = 1000000000;
			//Stopwatch swIp = Stopwatch.StartNew();
			//long len = 0;
			//for (int i = 0; i < ITERATIONS; i++)
			//{
			//	IPAddress addr = new IPAddress(i);
			//	len += addr.ToString().Length;
			//}
			//swIp.Stop();
			//Console.WriteLine("swIp " + swIp.Elapsed.TotalMilliseconds);

			int longIp = -1;
			int found = 0;
			int notFound = 0;
			Stopwatch sw1 = Stopwatch.StartNew();
			Parallel.For(0, ITERATIONS, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, (i) =>
			{
				int ip = Interlocked.Increment(ref longIp);
				if (ip >= ITERATIONS)
				{
					return;
				}
				if (ip % 100000000L == 0)
				{
					Console.WriteLine((ip / 1000000L).ToString() + ", " + found.ToString() + ", " + notFound.ToString());
				}
				IPAddress addr = new IPAddress(ip);
				byte[] bytes = provider.GetLocationByIp(addr.ToString());
				//if (bytes == null)
				//{
				//	Interlocked.Increment(ref notFound);
				//}
				//else
				//{
				//	Interlocked.Increment(ref found);
				//}
			});
			Console.WriteLine((longIp + 1L).ToString() + ", " + found.ToString() + ", " + notFound.ToString());
			sw1.Stop();
			Console.WriteLine(sw.Elapsed.TotalMilliseconds.ToString() + ", " + sw1.Elapsed.ToString());

			int iterations = -1;
			cities = provider.GetAllCities();
			found = 0;
			notFound = 0;
			ConcurrentDictionary<string, object> cd = new ConcurrentDictionary<string, object>();
			Stopwatch sw2 = Stopwatch.StartNew();
			Parallel.For(0, ITERATIONS, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, (i) =>
			{
				int iter = Interlocked.Increment(ref iterations);
				if (iter >= ITERATIONS)
				{
					return;
				}
				if (iter % 100000000L == 0)
				{
					Console.WriteLine((iter / 1000000L).ToString() + ", " + found.ToString() + ", " + notFound.ToString());
				}

				string city = cities[iter % cities.Length];
				byte[] bytes = provider.GetLocationsByCity(city);
				//if (bytes == null)
				//{
				//	Interlocked.Increment(ref notFound);
				//	cd.TryAdd(city, null);
				//}
				//else
				//{
				//	Interlocked.Increment(ref found);
				//}
			});
			sw2.Stop();
			Console.WriteLine((iterations + 1).ToString() + ", " + found.ToString() + ", " + notFound.ToString());

			Console.WriteLine(sw.Elapsed.TotalMilliseconds.ToString() + ", " + sw1.Elapsed.ToString() + ", " + sw2.Elapsed.ToString());
			Console.ReadLine();
		}
	}
}
