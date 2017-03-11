using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace server
{
	class Program
	{
		static void Main(string[] args)
		{
			ServiceHost pollingHost = new ServiceHost(typeof(SampleService));
			ServiceHost publishHost = new ServiceHost(typeof(PublishService));
			pollingHost.Open();
			publishHost.Open();

			Console.WriteLine("Сервисы запущены. Нажмите ENTER для выхода");

			Console.ReadLine();
			pollingHost.Close();
			publishHost.Close();
		}
	}
}
