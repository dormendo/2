using System;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Tips24.SmsSender
{
	class Program
	{
		public static IConfigurationRoot Config { get; private set; }

		static void Main(string[] args)
		{
			ConfigurationBuilder builder = new ConfigurationBuilder();
			Config = builder
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appconfig.json", false)
				.Build();

			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
			TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

			if (args.Length > 0 && string.Compare(args[0], "-service", true) == 0)
			{
				StartAsService();
			}
			else
			{
				StartAsConsoleApp();
			}

			Console.WriteLine("Hello World!");
		}

		private static void StartAsConsoleApp()
		{
			try
			{
				Starter.Start();
				Console.WriteLine("Сервер запущен");
				Console.WriteLine("Нажмите ENTER для остановки");
				Console.ReadLine();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				Starter.Stop();
				Console.WriteLine("Сервер остановлен");
			}
		}

		private static void StartAsService()
		{
			ServiceBase.Run(new ServiceBase[] { new Service() });
		}

		private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
			try
			{
				//Log.Logger.FatalException("Domain unhandled exception",
				//	(Exception)unhandledExceptionEventArgs.ExceptionObject, LogType.System);
				Console.WriteLine(unhandledExceptionEventArgs.ExceptionObject.ToString());
			}
			catch
			{
				WriteFatalError(unhandledExceptionEventArgs.ExceptionObject.ToString());
			}
		}

		private static void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
		{
			try
			{
				unobservedTaskExceptionEventArgs.SetObserved();

				//Log.Logger.FatalException("Task unhandled exception", unobservedTaskExceptionEventArgs.Exception,
				//	LogType.System);
				Console.WriteLine(unobservedTaskExceptionEventArgs.Exception.ToString());
			}
			catch
			{
				WriteFatalError(unobservedTaskExceptionEventArgs.Exception.ToString());
			}
		}

		/// <summary>
		/// метод для сохранения информации о ситуации когда даже логер не инициализируется
		/// </summary>
		private static void WriteFatalError(string message)
		{
			string directoryName = "logs\\fatal";
			string fileName = "FatalError.txt";

			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}

			string filePath = Path.Combine(directoryName, fileName);
			using (StreamWriter file = new StreamWriter(filePath, true))
			{
				file.WriteLine(message);
			}
		}
	}
}
