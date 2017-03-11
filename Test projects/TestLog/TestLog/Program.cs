using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestLog
{
    class Program
    {
        private static int _taskCount = 1000;

        private const int _msgCount = 1000;

        private static int _iterations = 1000;

        private static int _sleep = 0;

        private static string[] _messages = new string[_msgCount];

        static void Main(string[] args)
        {
            Console.Write("Введите количество потоков: ");
            _taskCount = (int)Convert.ToUInt32(Console.ReadLine());
            Console.Write("Введите количество итераций: ");
            _iterations = (int)Convert.ToUInt32(Console.ReadLine());
            Console.Write("Введите задержку в миллисекундах: ");
            _sleep = (int)Convert.ToUInt32(Console.ReadLine());
            string msg = "askljnsdckjn ;lscdkjn ;fvkln d;flvkm d;sfvdfsv dsfv dsfv dsfv ewrf qadswcx asdc sdcv";
            for (int i = 0; i < _msgCount; i++)
            {
                StringBuilder sb = new StringBuilder(msg);
                for (int j = 0; j < (i + 1) / 5; j++)
                {
                    sb.Append((i - j).ToString());
                }
                _messages[i] = sb.ToString();
            }

            NLogLog log1 = new NLogLog();
            log1.Write(_messages[0 % _msgCount]);
            MyLog log2 = new MyLog();
            log2.Write(_messages[0 % _msgCount]);
            //MyLog2 log3 = new MyLog2();
            //log3.Write(_messages[0 % _msgCount]);

            Stopwatch sw = new Stopwatch();

            sw.Start();
            Run(log1);
            sw.Stop();
            Console.WriteLine("NLog: {0}, {1}", sw.ElapsedMilliseconds, sw.ElapsedTicks);

            sw.Restart();
            Run(log2);
            log2.Dispose();
            sw.Stop();
            Console.WriteLine("MyLog: {0}, {1}, rec(w/e): {3}/{2}, waits: {4}, {5}, {6}, flushes: {7}",
                sw.ElapsedMilliseconds, sw.ElapsedTicks, log2.RecordsEnqueued, log2.RecordsWritten, log2.RefCount1, log2.RefCount2, log2.RefCount3, log2.Exchanges);

            //sw.Restart();
            //Run(log3);
            //log3.Dispose();
            //sw.Stop();
            //Console.WriteLine("MyLog2: {0}, {1}, rec(w/e): {3}/{2}, waits: {4}, {5}, {6}, cache size: {7}, flushes: {8}",
            //    sw.ElapsedMilliseconds, sw.ElapsedTicks, log3.RecordsEnqueued, log3.RecordsWritten, log3.RefCount1, log3.RefCount2, log3.RefCount3, log3.GetBufferCacheSize(), log3.Exchanges);

            //sw.Restart();
            //Run(audit3).Wait();
            //sw.Stop();
            //Console.WriteLine("Inmemory hash: {0}, {1}", sw.ElapsedMilliseconds, sw.ElapsedTicks);

            //sw.Restart();
            //Run(audit4).Wait();
            //sw.Stop();
            //Console.WriteLine("Cached inmemory hash: {0}, {1}, cache size: {2}", sw.ElapsedMilliseconds, sw.ElapsedTicks, audit4.GetCacheSize());


            Console.ReadLine();
        }

        private static void Run(LogBase log)
        {
            List<Task> tasks = new List<Task>(_taskCount);
            for (int i = 0; i < _taskCount; i++)
            {
                tasks.Add(Task.Factory.StartNew(() => { Iterations(log); }, TaskCreationOptions.LongRunning));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static void Iterations(LogBase log)
        {
            for (int i = 0; i < _iterations; i++)
            {
                log.Write(_messages[i % _msgCount]);
                if (_sleep != 0)
                {
                    Thread.Sleep((int)_sleep);
                }
            }
        }
    }
}
