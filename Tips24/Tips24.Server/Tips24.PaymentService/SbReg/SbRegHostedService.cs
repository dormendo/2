using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tips24.PaymentService.SbReg
{

	public class SbRegHostedService : IHostedService, IDisposable
	{
		#region Поля

		private readonly ILogger _logger;
		private CancellationTokenSource _cts;
		private SqlServer _sqlServer;
		private Share.ShareService _shareService;
		private SbRegConfiguration _config;

		#endregion

		#region Конструктор, запуск и остановка

		public SbRegHostedService(ILogger<SbRegHostedService> logger, SqlServer sqlServer, IConfiguration config, Share.ShareService shareService)
		{
			this._logger = logger;
			this._sqlServer = sqlServer;

			this._cts = new CancellationTokenSource();

			this._config = Startup.Config.SbReg;

			this._shareService = shareService;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("SbRegHostedService started");
			Task.Run(async () => await DoWork());
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("SbRegHostedService stopped");
			Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "SbRegHostedService.StopAsync", _logger);
			_cts.Cancel();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_logger.LogInformation("SbRegHostedService disposed");
			_cts.Cancel();
		}

		#endregion

		#region Цикл обработки запросов

		private async Task DoWork()
		{
			_logger.LogInformation("SbRegHostedService is working.");
			bool firstTime = true;
			while (!this._cts.IsCancellationRequested)
			{
				await ProcessCycle(firstTime);
				firstTime = false;
			}
			_logger.LogInformation("SbRegHostedService is completing work.");
		}

		private async Task ProcessCycle(bool firstTime)
		{
			try
			{
				TimeSpan interval = TimeSpan.FromSeconds(3);
				TimeSpan delay = (firstTime ? TimeSpan.Zero : interval);
				while (!_cts.IsCancellationRequested)
				{
					if (delay > TimeSpan.Zero)
					{
						_logger.LogTrace(delay.ToString());
						await Task.Delay(delay, _cts.Token);
						if (_cts.IsCancellationRequested)
						{
							break;
						}
					}

					DateTime startTime = DateTime.UtcNow;

					await this.ConsumeNextSbRegFile();

					if (_cts.IsCancellationRequested)
					{
						break;
					}

					DateTime endTime = DateTime.UtcNow;
					TimeSpan timeElapsed = endTime - startTime;
					delay = (timeElapsed > interval ? TimeSpan.Zero : interval - timeElapsed);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
			}
		}

		private async Task ConsumeNextSbRegFile()
		{
			string sbRegFile = Directory.EnumerateFiles(this._config.InputFolder).FirstOrDefault();
			if (sbRegFile == null)
			{
				return;
			}

			_logger.LogInformation("Начинается обработка файла " + Path.GetFileName(sbRegFile));
			FileProcessor processor = null;
			try
			{
				processor = new FileProcessor(sbRegFile, this._config.OutputFolder, this._shareService, _sqlServer);
				await processor.Run();
			}
			catch (Exception ex)
			{
				_logger.LogError("Ошибка при обработке файла " + Path.GetFileName(sbRegFile) + ": " + ex.ToString());
			}
			finally
			{
				_logger.LogInformation("Завершена обработка файла " + Path.GetFileName(sbRegFile));
			}
		}

		#endregion
	}
}
