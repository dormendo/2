using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTcp.Client
{
	class Program
	{
		static int Iterations = 250;
		static int Threads = 256;
		static int errors = 0;
        static IPEndPoint ipLocalEndPoint;
        static int _tcpErrors = 0;
        static List<WcfCaller> callerList = new List<WcfCaller>(Threads);
        static byte[][] messages = new byte[1000][];


		static void Main(string[] args)
		{
            Threads = Convert.ToInt32(ConfigurationManager.AppSettings["threads"]);
            Iterations = Convert.ToInt32(ConfigurationManager.AppSettings["iterations"]);
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < messages.Length; i++)
            {
                byte[] msg = new byte[104];
                GenerateMessage(rnd, msg);
                messages[i] = msg;
            }

            //byte[] ip = new byte[16] { 0xfe, 0x80, 0, 0, 0, 0, 0, 0, 0x3c, 0x8c, 0x8e, 0xf6, 0x86, 0x7d, 0x5e, 0xc3 };
            //IPAddress ipAddress = new IPAddress(ip, 12);
            byte[] ip = new byte[4] {127, 0, 0, 1 };
            IPAddress ipAddress = new IPAddress(ip);
            ipLocalEndPoint = new IPEndPoint(ipAddress, 8001);

            TestTcpClient();
            Console.WriteLine("Задача завершена. Нажмите ENTER");
            Console.ReadLine();
		}

        #region Tcp

        private static void TestTcpClient()
        {
            Console.WriteLine("Для начала тестирования TcpClient нажмите ENTER");
            Console.ReadLine();

            Stopwatch sw = Stopwatch.StartNew();
            Task[] tasks = new Task[Threads];
            for (int i = 0; i < Threads; i++)
            {
                tasks[i] = WorkerTcp(i);
            }

            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine("{0}, {1}, {2}, {3}", sw.ElapsedMilliseconds, sw.ElapsedTicks, _tcpErrors, ClientConnection.Writes);
        }

		private static async Task WorkerTcp(int workerId)
		{
            byte[] response = new byte[104];
            using (ClientConnection connection = new ClientConnection(ipLocalEndPoint))
            {
				try
                {
                    await connection.Start();
                }
                catch
                {
                    Interlocked.Increment(ref _tcpErrors);
                }
                //Console.WriteLine(workerId);

                for (int i = 0; i < Iterations; i++)
                {
                    byte[] msg = messages[i % messages.Length];
                    if (!(await connection.WriteEx(msg, response)))
                    {
                        Interlocked.Increment(ref _tcpErrors);
                    }
                }
                //Console.WriteLine(workerId);
            }
        }

        #endregion

        #region Wcf

        private static void TestWcfClient()
        {
            Console.WriteLine("Для начала тестирования WCF нажмите ENTER");
            Console.ReadLine();

            for (int i = 0; i < Threads; i++)
            {
                WcfCaller caller = new WcfCaller();
                caller.AcquireChannelFactory(null);
                callerList.Add(caller);
            }

            Stopwatch sw = Stopwatch.StartNew();
            Task[] tasks = new Task[Threads];
            for (int i = 0; i < Threads; i++)
            {
                tasks[i] = WorkerWcf(i);
            }

            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine("{0}, {1}, {2}", sw.ElapsedMilliseconds, sw.ElapsedTicks, errors);

            for (int i = 0; i < Threads; i++)
            {
                WcfCaller caller = callerList[i];
                caller.ReleaseChannelFactory();
            }
            callerList.Clear();
        }

        private static async Task WorkerWcf(int workerId)
        {
            //Random rnd = new Random(DateTime.Now.Millisecond);
            //WcfCaller caller = new WcfCaller();
            //caller.AcquireChannelFactory(null);
            //for (int i = 0; i < Iterations; i++)
            //{
            //    byte[] message = GenerateMessage(rnd);

            //    byte[] response = await caller.ExecuteWcfProcess(message);
            //    //if (!ValidateBytes(message, response))
            //    //{
            //    //    //Console.WriteLine("{0}, {1}", workerId, i);
            //    //    Interlocked.Increment(ref errors);
            //    //}
            //    //Console.WriteLine(i);
            //}
        }

        #endregion

		private static void GenerateMessage(Random rnd, byte[] msg)
		{
			rnd.NextBytes(msg);
            msg[0] = 100;
            msg[1] = 0;
            msg[2] = 0;
            msg[3] = 0;
		}

		private static bool ValidateBytes(byte[] a1, byte[] a2)
		{
			if (a1 == null || a2 == null || a1.Length != a2.Length)
			{
				return false;
			}

			return FastCmp(a1, a2);
		}

		private static unsafe bool FastCmp(byte[] a1, byte[] a2)
		{
            int mask = 255;
			int count = a1.Length;
			fixed(byte* b1 = a1, b2 = a2)
			{
				byte* p1 = b1, p2 = b2;
				for (int i = 0; i < count; i++, p1++, p2++)
				{
                    if (*p2 != (byte)(((int)*p1) ^ mask))
					{
						return false;
					}
				}
			}

			return true;
		}

        #region TestCopyMemory

        private static void TestCopyMemory(int size)
        {
            Console.WriteLine(size);
            Random rnd = new Random(DateTime.Now.Millisecond);
            int iterations = 1000000;
            byte[] source = new byte[size];
            rnd.NextBytes(source);
            byte[] target;
            Stopwatch sw = new Stopwatch();

            target = new byte[size];
            rnd.NextBytes(target);
            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                UnsafeCopyMemory(source, target);
            }
            sw.Stop();
            if (!ValidateBytes(source, target))
            {
                Console.WriteLine("Ошибка");
            }
            Console.WriteLine(sw.ElapsedMilliseconds);

            target = new byte[size];
            rnd.NextBytes(target);
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                BufferCopyMemory(source, target);
            }
            sw.Stop();
            if (!ValidateBytes(source, target))
            {
                Console.WriteLine("Ошибка");
            }
            Console.WriteLine(sw.ElapsedMilliseconds);

            target = new byte[size];
            rnd.NextBytes(target);
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                WinApiCopyMemory(source, target);
            }
            sw.Stop();
            if (!ValidateBytes(source, target))
            {
                Console.WriteLine("Ошибка");
            }
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.ReadLine();
        }
        
        private static unsafe void UnsafeCopyMemory(byte[] source, byte[] target)
        {
            int dataCount = source.Length;
            fixed (byte* bTarget = target, bSource = source)
            {
                byte* pTarget = bTarget;
                byte* pSource = bSource;
                for (int i = 0; i < dataCount; i++, pTarget++, pSource++)
                {
                    *pTarget = *pSource;
                }
            }
        }

        private static void BufferCopyMemory(byte[] source, byte[] target)
        {
            Buffer.BlockCopy(source, 0, target, 0, source.Length);
        }

        private unsafe static void WinApiCopyMemory(byte[] source, byte[] target)
        {
            fixed (byte* fSource = source, fTarget = target)
            {
                IntPtr pSource = new IntPtr(fSource);
                IntPtr pTarget = new IntPtr(fTarget);
                CopyMemory(pTarget, pSource, (uint)source.Length);
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        #endregion
	}
}
