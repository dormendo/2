using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAudit
{
    class Program
    {
        private static int _taskCount = 1000;

        private const int _msgCount = 1000;

        private static string[] _messages = new string[_msgCount];

        static void Main(string[] args)
        {
            string msg = "askljnsdckjn ;lscdkjn ;fvkln d;flvkm d;sfvdfsv dsfv dsfv dsfv ewrf qadswcx asdc sdcv";
            for (int i = 0; i < _msgCount; i++)
            {
                StringBuilder sb = new StringBuilder(msg);
                for (int j = 0; j < i / 5; j++)
                {
                    sb.Append((i - j).ToString());
                }
                _messages[i] = sb.ToString();
            }

            DiskBasedAudit audit1 = new DiskBasedAudit();
            audit1.WriteMessage(1, 0, Guid.NewGuid(), Guid.NewGuid(), _messages[0 % _msgCount]).Wait();
            InmemoryAudit audit2 = new InmemoryAudit();
            audit2.WriteMessage(1, 0, Guid.NewGuid(), Guid.NewGuid(), _messages[0 % _msgCount]).Wait();
            InmemoryHashAudit audit3 = new InmemoryHashAudit();
            audit3.WriteMessage(1, 0, Guid.NewGuid(), Guid.NewGuid(), _messages[0 % _msgCount]).Wait();
            CachedInmemoryHashAudit audit4 = new CachedInmemoryHashAudit();
            audit4.WriteMessage(1, 0, Guid.NewGuid(), Guid.NewGuid(), _messages[0 % _msgCount]).Wait();

            Stopwatch sw = new Stopwatch();

            //sw.Start();
            //Run(DiskBased).Wait();
            //sw.Stop();
            //Console.WriteLine("Disk based: {0}, {1}", sw.ElapsedMilliseconds, sw.ElapsedTicks);

            //sw.Restart();
            //Run(Inmemory).Wait();
            //sw.Stop();
            //Console.WriteLine("Inmemory: {0}, {1}", sw.ElapsedMilliseconds, sw.ElapsedTicks);

            sw.Restart();
            Run(audit3).Wait();
            sw.Stop();
            Console.WriteLine("Inmemory hash: {0}, {1}", sw.ElapsedMilliseconds, sw.ElapsedTicks);

            sw.Restart();
            Run(audit4).Wait();
            sw.Stop();
            Console.WriteLine("Cached inmemory hash: {0}, {1}, cache size: {2}", sw.ElapsedMilliseconds, sw.ElapsedTicks, audit4.GetCacheSize());

            Console.ReadLine();
        }

        private static async Task Run(AuditBase audit)
        {
            List<Task> tasks = new List<Task>(_taskCount);
            for (int i = 0; i < _taskCount; i++)
            {
                tasks.Add(Task.Run(async () => { await Iterations(audit); }));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task Iterations(AuditBase audit)
        {
            for (int i = 0; i < 3500; i++)
            {
                await audit.WriteMessage(1, i, Guid.NewGuid(), Guid.NewGuid(), _messages[i % _msgCount]);
            }
        }
    }
}
