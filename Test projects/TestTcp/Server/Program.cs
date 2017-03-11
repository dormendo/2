using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTcp.Server
{
	class Program
	{
		static void Main(string[] args)
		{
            int th, cp;
            ThreadPool.GetAvailableThreads(out th, out cp);
            Console.WriteLine("{0}, {1}", th, cp);

            //Console.WriteLine("Запускается TCP-сервер");
            //using (Listener listener = new Listener(8001))
            //{
            //    listener.Start();
            //    Console.WriteLine("TCP-сервер запущен");
            //    Console.WriteLine("Нажмите ENTER для завершения...");
            //    Console.ReadLine();
            //}
            //ThreadPool.GetAvailableThreads(out th, out cp);
            //Console.WriteLine("{0}, {1}, {2}, {3}", th, cp, Listener.Accepts, MainService.Calls);

            Console.WriteLine("Запускается Socket-сервер");
            TestTcp.Server.Sockets.SocketServer ss = new Sockets.SocketServer(3000, 8192);
            {
                ss.Init();
                ss.Start(new IPEndPoint(IPAddress.Any, 8001));
                Console.WriteLine("Socket-сервер запущен");
                Console.WriteLine("Нажмите ENTER для завершения...");
                Console.ReadLine();
                
                ss.Close();
            }
            ThreadPool.GetAvailableThreads(out th, out cp);
            Console.WriteLine("{0}, {1}", th, cp);

            //Console.WriteLine("Запускается WCF-сервер");
            //using (ServiceHost mainHost = new ServiceHost(typeof(MainService)))
            //{
            //    mainHost.Open();
            //    Console.WriteLine("WCF-сервер запущен");
            //    Console.WriteLine("Нажмите ENTER для завершения...");
            //    Console.ReadLine();
            //}
            //ThreadPool.GetAvailableThreads(out th, out cp);
            //Console.WriteLine("{0}, {1}", th, cp);
            //Console.ReadLine();
        }
	}
}
