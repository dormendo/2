using ImportSqlSpeedUp.SaveBlobOra;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	class Program
	{
		static string connString;
		static string oraConnString;
		static void Main(string[] args)
		{
			connString = ConfigurationManager.ConnectionStrings["Main"].ConnectionString;
			oraConnString = ConfigurationManager.ConnectionStrings["OraManaged"].ConnectionString;
			List<TestBase> tests = new List<TestBase>();
			tests.Add(new SaveBlobOra1("SaveBlobOra1", 10, 100, oraConnString));
			tests.Add(new SaveBlobOra2("SaveBlobOra2", 10, 100, oraConnString));
			tests.Add(new SaveNclobOra1("SaveNclobOra1", 10, 100, oraConnString));
			tests.Add(new SaveNclobOra2("SaveNclobOra2", 10, 100, oraConnString));


			#region 
			//ReadLinkOra(tests);

			//tests.Add(new SaveOriginalOra("Save.Ora", 20000, oraConnString, true));
			//tests.Add(new SaveNologgingOra("Save.Nologging.Ora", 20000, oraConnString, true));

			//tests.Add(new Save1("Save1.1", 20000 / 1, 1, connString));
			//tests.Add(new Save1("Save1.2", 20000 / 2, 2, connString));
			//tests.Add(new Save1("Save1.10", 20000 / 10, 10, connString));
			//tests.Add(new Save1("Save1.20", 20000 / 20, 20, connString));
			//tests.Add(new Save1("Save1.50", 20000 / 50, 50, connString));
			//tests.Add(new Save1("Save1.100", 20000 / 100, 100, connString));
			//tests.Add(new Save1("Save1.200", 20000 / 200, 200, connString));
			//tests.Add(new Save1("Save1.500", 20000 / 500, 500, connString));
			//tests.Add(new Save1("Save1.1000", 20000 / 1000, 1000, connString));

			//tests.Add(new Save2Ora("Save2.Ora.1", 20000 / 1, 1, oraConnString));
			//tests.Add(new Save2Ora("Save2.Ora.2", 20000 / 2, 2, oraConnString));
			//tests.Add(new Save2Ora("Save2.Ora.10", 20000 / 10, 10, oraConnString));
			//tests.Add(new Save2Ora("Save2.Ora.20", 20000 / 20, 20, oraConnString));
			//tests.Add(new Save2Ora("Save2.Ora.50", 20000 / 50, 50, oraConnString));
			//tests.Add(new Save2Ora("Save2.Ora.100", 20000 / 100, 100, oraConnString));
			//tests.Add(new Save2Ora("Save2.Ora.200", 20000 / 200, 200, oraConnString));
			//tests.Add(new Save2Ora("Save2.Ora.500", 20000 / 500, 500, oraConnString));
			//tests.Add(new Save2Ora("Save2.Ora.1000", 20000 / 1000, 1000, oraConnString));


			//tests.Add(new Save3Ora("Save3.Ora.2", 20000 / 2, 2, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.10", 20000 / 10, 10, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.20", 20000 / 20, 20, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.50", 20000 / 50, 50, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.100", 20000 / 100, 100, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.200", 20000 / 200, 200, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.500", 20000 / 500, 500, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.1000", 20000 / 1000, 1000, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.2000", 20000 / 2000, 2000, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.5000", 20000 / 5000, 5000, oraConnString));
			//tests.Add(new Save3Ora("Save3.Ora.10000", 20000 / 10000, 10000, oraConnString));


			//for (int i = 200; i <= 300; i++)
			//{
			//	tests.Add(new Save3Ora("Save3.Ora." + i.ToString(), 20000 / i, i, oraConnString));
			//}

			//tests.Add(new Save4Ora("Save4.Ora.2", 20000 / 2, 2, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.10", 20000 / 10, 10, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.20", 20000 / 20, 20, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.50", 20000 / 50, 50, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.100", 20000 / 100, 100, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.200", 20000 / 200, 200, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.500", 20000 / 500, 500, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.1000", 20000 / 1000, 1000, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.2000", 20000 / 2000, 2000, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.5000", 20000 / 5000, 5000, oraConnString));
			//tests.Add(new Save4Ora("Save4.Ora.10000", 20000 / 10000, 10000, oraConnString));
			#endregion

			foreach (TestBase test in tests)
			{
				test.Run();
				test.PrintReport();
				test.Dispose();
			}

			Console.WriteLine("Для выхода нажмите любую клавишу...");
			Console.ReadLine();
		}


		private static void ReadLinkOra(List<TestBase> tests)
		{
			//tests.Add(new ReadLinkOraOriginal("RL.Ora", 100000, oraConnString));
			//tests.Add(new ReadLinkOraOriginal1("RL1.Ora", 100000, oraConnString));


			tests.Add(new ReadLinkOra03("RL03.Ora-10", 100000 / 10, 10, oraConnString));
			tests.Add(new ReadLinkOra03("RL03.Ora-20", 100000 / 20, 20, oraConnString));
			tests.Add(new ReadLinkOra03("RL03.Ora-50", 100000 / 50, 50, oraConnString));
			tests.Add(new ReadLinkOra03("RL03.Ora-100", 100000 / 100, 100, oraConnString));
			tests.Add(new ReadLinkOra03("RL03.Ora-200", 100000 / 200, 200, oraConnString));
			//tests.Add(new ReadLinkOra03("RL03.Ora-500", 100000 / 500, 500, oraConnString));
			//tests.Add(new ReadLinkOra03("RL03.Ora-1000", 100000 / 1000, 1000, oraConnString));

			tests.Add(new ReadLinkOra031("RL031.Ora-10", 100000 / 10, 10, oraConnString));
			tests.Add(new ReadLinkOra031("RL031.Ora-20", 100000 / 20, 20, oraConnString));
			tests.Add(new ReadLinkOra031("RL031.Ora-50", 100000 / 50, 50, oraConnString));
			tests.Add(new ReadLinkOra031("RL031.Ora-100", 100000 / 100, 100, oraConnString));
			tests.Add(new ReadLinkOra031("RL031.Ora-200", 100000 / 200, 200, oraConnString));
			//tests.Add(new ReadLinkOra031("RL031.Ora-500", 100000 / 500, 500, oraConnString));
			//tests.Add(new ReadLinkOra031("RL031.Ora-1000", 100000 / 1000, 1000, oraConnString));

			tests.Add(new ReadLinkOra032("RL032.Ora-10", 100000 / 10, 10, oraConnString));
			tests.Add(new ReadLinkOra032("RL032.Ora-20", 100000 / 20, 20, oraConnString));
			tests.Add(new ReadLinkOra032("RL032.Ora-50", 100000 / 50, 50, oraConnString));
			tests.Add(new ReadLinkOra032("RL032.Ora-100", 100000 / 100, 100, oraConnString));
			tests.Add(new ReadLinkOra032("RL032.Ora-200", 100000 / 200, 200, oraConnString));
			//tests.Add(new ReadLinkOra032("RL032.Ora-500", 100000 / 500, 500, oraConnString));
			//tests.Add(new ReadLinkOra032("RL032.Ora-1000", 100000 / 1000, 1000, oraConnString));
		}

		private static void ReadLink(List<TestBase> tests)
		{

			//tests.Add(new ReadLinkOriginal("RL", 100000, connString));
			//tests.Add(new ReadLinkOriginal1("RL1", 100000, connString));

			////tests.Add(new ReadLink01("RL01-10", 100000 / 10, 10, connString));
			////tests.Add(new ReadLink01("RL01-20", 100000 / 20, 20, connString));
			////tests.Add(new ReadLink01("RL01-50", 100000 / 50, 50, connString));
			////tests.Add(new ReadLink01("RL01-100", 100000 / 100, 100, connString));

			////tests.Add(new ReadLink01("RL01-200", 100000 / 200, 200, connString));
			////tests.Add(new ReadLink01("RL01-500", 100000 / 500, 500, connString));
			////tests.Add(new ReadLink01("RL01-1000", 100000 / 1000, 1000, connString));

			//tests.Add(new ReadLink02("RL02-10", 100000 / 10, 10, connString));
			//tests.Add(new ReadLink02("RL02-20", 100000 / 20, 20, connString));
			//tests.Add(new ReadLink02("RL02-50", 100000 / 50, 50, connString));
			//tests.Add(new ReadLink02("RL02-100", 100000 / 100, 100, connString));

			//tests.Add(new ReadLink02p("RL02p-10", 100000 / 10, 10, connString));
			//tests.Add(new ReadLink02p("RL02p-20", 100000 / 20, 20, connString));
			//tests.Add(new ReadLink02p("RL02p-50", 100000 / 50, 50, connString));

			//tests.Add(new ReadLink021("RL021-10", 100000 / 10, 10, connString));
			//tests.Add(new ReadLink021("RL021-20", 100000 / 20, 20, connString));
			//tests.Add(new ReadLink021("RL021-50", 100000 / 50, 50, connString));
			//tests.Add(new ReadLink021("RL021-100", 100000 / 100, 100, connString));

			//tests.Add(new ReadLink021p("RL021p-10", 100000 / 10, 10, connString));
			//tests.Add(new ReadLink021p("RL021p-20", 100000 / 20, 20, connString));
			//tests.Add(new ReadLink021p("RL021p-50", 100000 / 50, 50, connString));


			//tests.Add(new ReadLink03("RL03-10", 100000 / 10, 10, connString));
			//tests.Add(new ReadLink03("RL03-20", 100000 / 20, 20, connString));
			//tests.Add(new ReadLink03("RL03-50", 100000 / 50, 50, connString));
			//tests.Add(new ReadLink03("RL03-100", 100000 / 100, 100, connString));
			//tests.Add(new ReadLink03("RL03-200", 100000 / 200, 200, connString));
			//tests.Add(new ReadLink03("RL03-500", 100000 / 500, 500, connString));
			//tests.Add(new ReadLink03("RL03-1000", 100000 / 1000, 1000, connString));

			//tests.Add(new ReadLink031("RL031-10", 100000 / 10, 10, connString));
			//tests.Add(new ReadLink031("RL031-20", 100000 / 20, 20, connString));
			//tests.Add(new ReadLink031("RL031-50", 100000 / 50, 50, connString));
			//tests.Add(new ReadLink031("RL031-100", 100000 / 100, 100, connString));
			//tests.Add(new ReadLink031("RL031-200", 100000 / 200, 200, connString));
			//tests.Add(new ReadLink031("RL031-500", 100000 / 500, 500, connString));
			//tests.Add(new ReadLink031("RL031-1000", 100000 / 1000, 1000, connString));

			//tests.Add(new ReadLink032("RL032-10", 100000 / 10, 10, connString));
			//tests.Add(new ReadLink032("RL032-20", 100000 / 20, 20, connString));
			//tests.Add(new ReadLink032("RL032-50", 100000 / 50, 50, connString));
			//tests.Add(new ReadLink032("RL032-100", 100000 / 100, 100, connString));
			//tests.Add(new ReadLink032("RL032-200", 100000 / 200, 200, connString));
			//tests.Add(new ReadLink032("RL032-500", 100000 / 500, 500, connString));
			//tests.Add(new ReadLink032("RL032-1000", 100000 / 1000, 1000, connString));

			tests.Add(new ReadLink05("RL05-10", 100000 / 10, 10, connString));
			tests.Add(new ReadLink05("RL05-20", 100000 / 20, 20, connString));
			tests.Add(new ReadLink05("RL05-50", 100000 / 50, 50, connString));
			tests.Add(new ReadLink05("RL05-100", 100000 / 100, 100, connString));
			tests.Add(new ReadLink05("RL05-200", 100000 / 200, 200, connString));
			tests.Add(new ReadLink05("RL05-500", 100000 / 500, 500, connString));
			tests.Add(new ReadLink05("RL05-500-1", 100000 / 500, 200, connString));
			tests.Add(new ReadLink05("RL05-1000", 100000 / 1000, 1000, connString));
			tests.Add(new ReadLink05("RL05-2000", 100000 / 2000, 2000, connString));
			tests.Add(new ReadLink05("RL05-5000", 100000 / 5000, 5000, connString));
			tests.Add(new ReadLink05("RL05-10000", 100000 / 10000, 10000, connString));

			tests.Add(new ReadLink051("RL051-10", 100000 / 10, 10, connString));
			tests.Add(new ReadLink051("RL051-20", 100000 / 20, 20, connString));
			tests.Add(new ReadLink051("RL051-50", 100000 / 50, 50, connString));
			tests.Add(new ReadLink051("RL051-100", 100000 / 100, 100, connString));
			tests.Add(new ReadLink051("RL051-200", 100000 / 200, 200, connString));
			tests.Add(new ReadLink051("RL051-500", 100000 / 500, 500, connString));
			tests.Add(new ReadLink051("RL051-500-1", 100000 / 500, 200, connString));
			tests.Add(new ReadLink051("RL051-1000", 100000 / 1000, 1000, connString));
			tests.Add(new ReadLink051("RL051-2000", 100000 / 2000, 2000, connString));
			tests.Add(new ReadLink051("RL051-5000", 100000 / 5000, 5000, connString));
			tests.Add(new ReadLink051("RL051-10000", 100000 / 10000, 10000, connString));





			//tests.Add(new ReadLinkOriginal("RL", 1, connString));
			//tests.Add(new ReadLinkOriginal1("RL1", 1, connString));

			//tests.Add(new ReadLink01("RL01-10", 1, 10, connString));
			//tests.Add(new ReadLink01("RL01-20", 1, 20, connString));
			//tests.Add(new ReadLink01("RL01-50", 1, 50, connString));
			//tests.Add(new ReadLink01("RL01-100", 1, 100, connString));

			////tests.Add(new ReadLink01("RL01-200", 100000 / 200, 200, connString));
			////tests.Add(new ReadLink01("RL01-500", 100000 / 500, 500, connString));
			////tests.Add(new ReadLink01("RL01-1000", 100000 / 1000, 1000, connString));

			//tests.Add(new ReadLink02("RL02-10", 1, 10, connString));
			//tests.Add(new ReadLink02("RL02-20", 1, 20, connString));
			//tests.Add(new ReadLink02("RL02-50", 1, 50, connString));
			//tests.Add(new ReadLink02("RL02-100", 1, 100, connString));

			////tests.Add(new ReadLink02("RL02-200", 100000 / 200, 200, connString));
			////tests.Add(new ReadLink02("RL02-500", 100000 / 500, 500, connString));
			////tests.Add(new ReadLink02("RL02-1000", 100000 / 1000, 1000, connString));

			//tests.Add(new ReadLink021("RL021-10", 1, 10, connString));
			//tests.Add(new ReadLink021("RL021-20", 1, 20, connString));
			//tests.Add(new ReadLink021("RL021-50", 1, 50, connString));
			//tests.Add(new ReadLink021("RL021-100", 1, 100, connString));



			//tests.Add(new ReadLink03("RL03-10", 1, 10, connString));
			//tests.Add(new ReadLink03("RL03-20", 1, 20, connString));
			//tests.Add(new ReadLink03("RL03-50", 1, 50, connString));
			tests.Add(new ReadLink03("RL03-100", 1, 100, connString));
			//tests.Add(new ReadLink03("RL03-200", 1, 200, connString));
			//tests.Add(new ReadLink03("RL03-500", 1, 500, connString));
			//tests.Add(new ReadLink03("RL03-1000", 1, 1000, connString));

			//tests.Add(new ReadLink031("RL031-10", 1, 10, connString));
			//tests.Add(new ReadLink031("RL031-20", 1, 20, connString));
			//tests.Add(new ReadLink031("RL031-50", 1, 50, connString));
			tests.Add(new ReadLink031("RL031-100", 1, 100, connString));
			//tests.Add(new ReadLink031("RL031-200", 1, 200, connString));
			//tests.Add(new ReadLink031("RL031-500", 1, 500, connString));
			//tests.Add(new ReadLink031("RL031-1000", 1, 1000, connString));

			//tests.Add(new ReadLink032("RL032-10", 1, 10, connString));
			//tests.Add(new ReadLink032("RL032-20", 1, 20, connString));
			//tests.Add(new ReadLink032("RL032-50", 1, 50, connString));
			tests.Add(new ReadLink032("RL032-100", 1, 100, connString));
			//tests.Add(new ReadLink032("RL032-200", 1, 200, connString));
			//tests.Add(new ReadLink032("RL032-500", 1, 500, connString));
			//tests.Add(new ReadLink032("RL032-1000", 1, 1000, connString));

			//tests.Add(new ReadLink05("RL05-10", 1, 10, connString));
			//tests.Add(new ReadLink05("RL05-20", 1, 20, connString));
			//tests.Add(new ReadLink05("RL05-50", 1, 50, connString));
			//tests.Add(new ReadLink05("RL05-100", 1, 100, connString));
			//tests.Add(new ReadLink05("RL05-200", 1, 200, connString));
			//tests.Add(new ReadLink05("RL05-500", 1, 500, connString));
			//tests.Add(new ReadLink05("RL05-1000", 1, 1000, connString));

			//tests.Add(new ReadLink051("RL051-10", 1, 10, connString));
			//tests.Add(new ReadLink051("RL051-20", 1, 20, connString));
			//tests.Add(new ReadLink051("RL051-50", 1, 50, connString));
			//tests.Add(new ReadLink051("RL051-100", 1, 100, connString));
			//tests.Add(new ReadLink051("RL051-200", 1, 200, connString));
			//tests.Add(new ReadLink051("RL051-500", 1, 500, connString));
			//tests.Add(new ReadLink051("RL051-1000", 1, 1000, connString));
		}
	}
}
