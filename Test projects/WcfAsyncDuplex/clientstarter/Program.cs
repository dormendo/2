using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace clientstarter
{
	class Program
	{
		const int CLIENT_COUNT = 40;
		const string CLIENT_PATH =
#if DEBUG
			"..\\..\\..\\..\\client\\bin\\x64\\Debug\\client.exe";
#else
			"..\\..\\..\\..\\client\\bin\\x64\\Release\\client.exe";
#endif
		static void Main(string[] args)
		{
			Process[] killList = Process.GetProcessesByName("client");
			for (int i = 0; i < killList.Length; i++)
			{
				killList[i].Kill();
			}

			
			List<Process> list = new List<Process>();
			for (int i = 0; i < CLIENT_COUNT; i++)
			{
				Process p = new Process();
				ProcessStartInfo psi = new ProcessStartInfo(CLIENT_PATH);
				p.StartInfo = psi;
				p.Start();
				list.Add(p);
				Thread.Sleep(100);
			}

			Console.WriteLine("Нажмите ENTER для закрытия всех запущенных клиентов");
			Console.ReadLine();

			for (int i = 0; i < list.Count; i++)
			{
				list[i].Kill();
			}
		}
	}
}
