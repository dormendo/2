using Lanit.Norma.AppServer.Cache;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FixCouchbase
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Mongo m = new Mongo();
				m.TestMongo();
				//Test();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			Console.ReadLine();
		}

		#region Mongo



		#endregion

		#region Couchbase

		private static void Test()
		{
			string s = "1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`1234567890-=qwertyuiop[]asdfghjkl;'\\zxcvbnm,./`";
			const int q = 100000;
			int iteration = 0;
			List<Task> taskList = new List<Task>();
			long sum = 0;

			Stopwatch sw = Stopwatch.StartNew();
			for (int t = 0; t < 16; t++)
			{
				taskList.Add(Task.Factory.StartNew(() =>
				{
					while (true)
					{
						int ci = Interlocked.Increment(ref iteration);
						if (ci > q)
						{
							break;
						}

						try
						{
							//Item item = GetItem(ci, (ci % 16) + 1);
							string result = CacheManager.Storage.GetFromMapRaw("1111111111_1111111112", "MapElement_" + ci.ToString());
							if (result == null)
							{
								Interlocked.Increment(ref sum);
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.ToString());
						}

						if ((ci % 1000) == 0)
						{
							Console.WriteLine("GetFromMapRaw: " + ci.ToString() + ", отсутствует: " + sum.ToString());
						}
					}
				}));
			}

			Task.WaitAll(taskList.ToArray());
			taskList.Clear();
			sw.Stop();
			Console.WriteLine(sw.Elapsed);
			Console.ReadLine();



			iteration = 0;
			sw.Restart();
			for (int t = 0; t < 1; t++)
			{
				taskList.Add(Task.Factory.StartNew(() =>
				{
					while (true)
					{
						int ci = Interlocked.Increment(ref iteration);
						if (ci > q)
						{
							break;
						}
						if ((ci % 1000) == 0)
						{
							Console.WriteLine("StoreInMapRaw: " + ci.ToString());
						}

						try
						{
							//Item item = GetItem(ci, (ci % 16) + 1);
							CacheManager.Storage.StoreInMapRaw("1111111111_1111111112", "MapElement_" + ci.ToString(), s);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.ToString());
						}
					}
				}));
			}

			Task.WaitAll(taskList.ToArray());
			taskList.Clear();
			sw.Stop();
			Console.WriteLine(sw.Elapsed);
			Console.ReadLine();
			return;


			//for (int t = 0; t < 16; t++)
			//{
			//	taskList.Add(Task.Factory.StartNew(() =>
			//	{
			//		while (true)
			//		{
			//			int ci = Interlocked.Increment(ref iteration);
			//			if (ci > q)
			//			{
			//				break;
			//			}
			//			if ((ci % 10000) == 0)
			//			{
			//				Console.WriteLine("Store: " + ci.ToString());
			//			}

			//			try
			//			{
			//				dynamic item = GetItem(ci, (ci % 16) + 1);
			//				CacheManager.Cache.Store(ci.ToString(), item);
			//			}
			//			catch (Exception ex)
			//			{
			//				Console.WriteLine(ex.ToString());
			//			}
			//		}
			//	}));
			//}

			//Task.WaitAll(taskList.ToArray());
			//taskList.Clear();

			Console.ReadLine();

			iteration = 0;
			for (int t = 0; t < 16; t++)
			{
				taskList.Add(Task.Factory.StartNew(() =>
				{
					while (true)
					{
						int ci = Interlocked.Increment(ref iteration);
						if (ci > q)
						{
							break;
						}
						if ((ci % 10000) == 0)
						{
							Console.WriteLine("Get: " + ci.ToString());
						}

						try
						{
							Item item = CacheManager.Cache.Get<Item>(ci.ToString());
							int c = (item != null && item.Items1 != null ? item.Items1.Count : 0) + (item != null && item.Items2 != null ? item.Items2.Count : 0);
							Interlocked.Add(ref sum, c);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.ToString());
						}
					}
				}));
			}

			Task.WaitAll(taskList.ToArray());
			sw.Stop();
			Console.WriteLine(sw.Elapsed);
			Console.ReadLine();

			taskList.Clear();

			iteration = 0;
			sw.Restart();
			for (int t = 0; t < 16; t++)
			{
				taskList.Add(Task.Factory.StartNew(() =>
				{
					while (true)
					{
						int ci = Interlocked.Increment(ref iteration);
						if (ci > q)
						{
							break;
						}
						if ((ci % 10000) == 0)
						{
							Console.WriteLine("Get: " + ci.ToString());
						}

						try
						{
							Item item = CacheManager.Cache.Get<Item>(ci.ToString());
							int c = (item != null && item.Items1 != null ? item.Items1.Count : 0) + (item != null && item.Items2 != null ? item.Items2.Count : 0);
							Interlocked.Add(ref sum, c);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.ToString());
						}
					}
				}));
			}

			Task.WaitAll(taskList.ToArray());
			sw.Stop();
			Console.WriteLine(sw.Elapsed);
			Console.ReadLine();


		}

		private static Item GetItem(int ci, int i)
		{
			Item item = new Item
			{
				Id = i,
				Name = "Name " + ci.ToString(),
				Description = "Description " + ci.ToString(),
				Items1 = new Dictionary<string, Item2>(),
				Items2 = new Dictionary<string, Item2>()
			};

			for (int t = 0; t < i * 200; t++)
			{
				item.Items1.Add("Items1.Item " + ci.ToString() + " " + t.ToString(), new Item2 { Id = t, Name = "Name " + t.ToString(), Description = "Description " + t.ToString() });
				item.Items2.Add("Items2.Item " + ci.ToString() + " " + t.ToString(), new Item2 { Id = t, Name = "Name " + t.ToString(), Description = "Description " + t.ToString() });
			}

			return item;
		}

		#endregion
	}
}