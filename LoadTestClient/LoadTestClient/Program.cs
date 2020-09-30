using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class Program
	{
		private static void WriteErrorMessage(string message)
		{
			Console.WriteLine("Ошибка: " + message);
			Console.WriteLine();
			Console.WriteLine("Формат запуска:");
			Console.WriteLine("LoadTestClient.exe <config.json>");
			Console.WriteLine("<config.json> - путь до файла c конфигурацией в формате JSON");
			return;
		}

		static void Main(string[] args)
		{
			Stopwatch globalSw = Stopwatch.StartNew();
			DateTime globalStartTime = DateTime.Now;

			MetricsProvider metrics = new MetricsProvider(globalSw, globalStartTime);
			metrics.AddEvent(EventType.Start);

			Console.WriteLine("Средство нагрузочного тестирования веб-сервисов \"Норма\". Ланит, 2020");
			Console.WriteLine();

			if (args.Length == 0)
			{
				WriteErrorMessage("Не задан параметр командной строки");
				return;
			}

			if (args.Length > 1)
			{
				WriteErrorMessage("Слишком много параметров командной строки");
				return;
			}

			if (string.IsNullOrWhiteSpace(args[0]) || !File.Exists(args[0]))
			{
				WriteErrorMessage("Не найден файл \"" + args[0] + "\"");
				return;
			}

			
			Console.WriteLine("Проверка конфигурации и подготовка к проведению теста...");
			
			
			// Загрузка конфигурации
			metrics.AddEvent(EventType.LoadConfigStart);

			string json = File.ReadAllText(args[0]);
			Config config = null;
			try
			{
				config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(json);
			}
			catch
			{
				WriteErrorMessage("Файл \"" + args[0] + "\" содержит некорректные данные");
				return;
			}

			metrics.AddEvent(EventType.LoadConfigEnd);
			metrics.SetConfig(json, config);


			if (config == null)
			{
				WriteErrorMessage("Файл \"" + args[0] + "\" содержит некорректные данные");
				return;
			}


			// Проведение теста
			ConfigReportData crd;
			using (Engine engine = new Engine(config, args[0], metrics))
			{
				try
				{
					if (!engine.Prepare())
					{
						WriteErrorMessage(engine.GetError());
						return;
					}

					Console.WriteLine("Подготовка к тесту завершена");
					Console.WriteLine();

					engine.Run();
					crd = engine.GetConfigReportData();
				}
				catch (Exception ex)
				{
					WriteErrorMessage(ex.ToString());
					return;
				}
			}

			metrics.AddEvent(EventType.Complete);

			Console.WriteLine("Сохранение отчётов...");
			metrics.WriteLogs(crd);

			Console.WriteLine("Нажмите ENTER для завершения");
			Console.ReadLine();
		}
	}
}
