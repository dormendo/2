using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogEngine;

namespace Runner
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

            Logger.RegisterLog(LogType.System, "SystemDev");
            Logger.RegisterLog(LogType.Operations, "OperationsDev");

            Stopwatch sw = Stopwatch.StartNew();
            Run(LogType.System).Wait();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            Thread.Sleep(1000);
            
            sw.Restart();
            Run(LogType.Operations).Wait();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            Logger.Stop();

            Console.ReadLine();
        }

        private static async Task Run(LogType logType)
        {
            List<Task> tasks = new List<Task>(_taskCount);
            for (int i = 0; i < _taskCount; i++)
            {
                tasks.Add(Task.Run(async () => { await Iterations(logType); }));
            }

            await Task.WhenAll(tasks);
        }

        private async static Task Iterations(LogType logType)
        {
            for (int i = 0; i < _iterations; i++)
            {
                Logger.Trace(_messages[i % _msgCount], logType);
                if (_sleep != 0)
                {
                    await Task.Delay(_sleep);
                }
            }
        }
    }
}
