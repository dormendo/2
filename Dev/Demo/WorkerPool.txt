using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lanit.Norma.AppServer.Configuration;

namespace Lanit.Norma.AppServer.Agent
{
	/// <summary>
	/// Пул рабочих серверов
	/// </summary>
	public class WorkerPool : IWorkerPool
	{
		#region Поля

		private WorkerAgentElement _confElement;
		private int _minSize;
		private int _maxSize;
		private int _incrementSize;
		private int _incrementThreshold;

		private NetworkPortPool _portPool;
		private ConcurrentQueue<WorkerMetadata> _availableWorkers = new ConcurrentQueue<WorkerMetadata>();
		private object _consumeLock = new object();
		private object _increaseLock = new object();

		private ConcurrentQueue<WorkerMetadata> _workersToFree = new ConcurrentQueue<WorkerMetadata>();
		private object _freeLock = new object();

		private string _pluginsFolder;

		#endregion

		#region Старт

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="pluginsFolder">Папка с плагинами</param>
		public WorkerPool(string pluginsFolder)
		{
			_pluginsFolder = pluginsFolder;
		}

		/// <summary>
		/// Запускает компонент
		/// </summary>
		public void Start()
		{
			Stopwatch sw = Stopwatch.StartNew();
			this.AcquirePortPool();
			sw.Stop();
			Console.WriteLine($"AcquirePortPool: {sw.ElapsedMilliseconds}");

			sw.Restart();
			this.AcquireParameters();
			sw.Stop();
			Console.WriteLine($"AcquireParameters: {sw.ElapsedMilliseconds}");

			sw.Restart();
			this.EnsureAllWorkersAreClosed();
			sw.Stop();
			Console.WriteLine($"EnsureAllWorkersAreClosed: {sw.ElapsedMilliseconds}");

			sw.Restart();
			this.GenerateConfigFiles();
			sw.Stop();
			Console.WriteLine($"GenerateConfigFiles: {sw.ElapsedMilliseconds}");

			sw.Restart();
			this.CreateInitialPool();
			sw.Stop();
			Console.WriteLine($"CreateInitialPool: {sw.ElapsedMilliseconds}");

			Thread increasePoolThread = new Thread(IncreasePoolCycle);
			increasePoolThread.IsBackground = true;
			increasePoolThread.Name = "InProcessAgent. Increase worker pool thread";
			increasePoolThread.Start();

			Thread freeWorkerThread = new Thread(FreeWorkersCycle);
			freeWorkerThread.IsBackground = true;
			freeWorkerThread.Name = "InProcessAgent. Free used workers thread";
			freeWorkerThread.Start();
		}

		private void AcquireParameters()
		{
			MainServerConfigurationSection section = MainServerConfigurationSection.GetSection();
			_confElement = section.WorkerAgent;
			if (_confElement.Pooling != null)
			{
				_minSize = _confElement.Pooling.MinSize;
				_maxSize = _confElement.Pooling.MaxSize;
				_incrementSize = _confElement.Pooling.IncrementSize;
				_incrementThreshold = _confElement.Pooling.IncrementThreshold;

				if (_minSize < 0)
				{
					_minSize = 0;
				}
				else if (_minSize > 100)
				{
					_minSize = 100;
				}

				if (_maxSize < 1)
				{
					_maxSize = 1;
				}
				else if (_maxSize > 1000)
				{
					_maxSize = 1000;
				}
				else if (_maxSize < _minSize)
				{
					_maxSize = _minSize;
				}

				if (_incrementSize <= 0)
				{
					_incrementSize = 1;
				}

				if (_incrementThreshold < 0)
				{
					_incrementThreshold = 0;
				}
				else if (_incrementThreshold >= _minSize)
				{
					_incrementThreshold = _minSize - 1;
				}
			}
			else
			{
				_minSize = 5;
				_maxSize = 100;
				_incrementSize = 2;
				_incrementThreshold = 2;
			}

			if (_portPool.Count < _maxSize)
			{
				_maxSize = _portPool.Count;
				if (_minSize > _maxSize)
				{
					_minSize = _maxSize;
				}
			}
		}

		private void EnsureAllWorkersAreClosed()
		{
			Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_confElement.GetExecutablePath()));
			foreach (Process p in processes)
			{
				try
				{
					if (string.Compare(p.MainModule.FileName, _confElement.GetExecutablePath(), true) == 0)
					{
						p.Kill();
					}
				}
				catch { }
			}
		}

		private void AcquirePortPool()
		{
			_portPool = new NetworkPortPool();
			_portPool.Start();
		}

		private void GenerateConfigFiles()
		{
			string execFile = _confElement.GetExecutablePath();
			string execFolder = Path.GetDirectoryName(execFile);
			string configFolder = _confElement.GetConfigFolder();
			string logFolder = _confElement.GetLogFolder();

			DirectoryInfo di = new DirectoryInfo(configFolder);
			if (di.Exists)
			{
				foreach (FileInfo file in di.EnumerateFiles())
				{
					file.Delete();
				}
				foreach (DirectoryInfo dir in di.EnumerateDirectories())
				{
					dir.Delete(true);
				}
			}
			else
			{
				di = Directory.CreateDirectory(configFolder);
			}

			string ldcTemplate = File.ReadAllText(Path.Combine(execFolder, "LoggingDistributorConfiguration.config"));
			string acTemplate = File.ReadAllText(execFile + ".config");
			List<ushort> ports = _portPool.GetPortCollectionCopy();
			Parallel.ForEach(ports, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, (port) =>
			{
				string portStr = port.ToString();
				DirectoryInfo subdir = Directory.CreateDirectory(Path.Combine(configFolder, portStr));
				File.Copy(Path.Combine(execFolder, "LoggingConfiguration.config"), Path.Combine(subdir.FullName, "LoggingConfiguration.config"));

				string ldcText = ldcTemplate.Replace("{FOLDER}", Path.Combine(logFolder, portStr));
				File.WriteAllText(Path.Combine(subdir.FullName, "LoggingDistributorConfiguration.config"), ldcText);

				string acText = acTemplate.Replace("{PORT}", portStr).Replace("{CONFIG_FOLDER}", subdir.FullName);
				File.WriteAllText(Path.Combine(subdir.FullName, Path.GetFileName(execFile) + ".config"), acText);
			});
		}

		private void CreateInitialPool()
		{
			Parallel.For(0, _minSize, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, _ =>
			{
				NewWorker();
			});
		}

		/// <summary>
		/// Останавливает компонент
		/// </summary>
		public void Stop()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Создание нового рабочего сервера

		private bool NewWorker()
		{
			ushort port = _portPool.AcquirePort();
			bool success = false;

			try
			{
				WorkerMetadata worker = new WorkerMetadata(port);
				success = StartNewWorker(worker);
				if (success)
				{
					lock (_consumeLock)
					{
						_availableWorkers.Enqueue(worker);
						Monitor.Pulse(_consumeLock);
					}
				}
				else
				{
					_portPool.ReleasePort(port);
				}
			}
			catch (Exception ex)
			{
				success = false;
				_portPool.ReleasePort(port);
				Trace.WriteLine(ex.ToString());
			}

			return success;
		}

		private bool StartNewWorker(WorkerMetadata worker)
		{
			string port = worker.Port.ToString();
			Guid markerGuid = Guid.NewGuid();
			string configFile = Path.Combine(_confElement.GetConfigFolder(), port, Path.GetFileName(_confElement.GetExecutablePath())) + ".config";
			string args = $"{port} {markerGuid} \"{configFile}\" \"{_pluginsFolder}\"";
			string markerLine = $"Worker {port},{markerGuid} started";

			Process process = new Process();
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.FileName = _confElement.GetExecutablePath();
			process.StartInfo.WorkingDirectory = Path.GetDirectoryName(_confElement.GetExecutablePath());
			process.StartInfo.Arguments = args;
			process.StartInfo.CreateNoWindow = false;
			process.Start();

			bool error = false;
			while (true)
			{
				if (process.HasExited)
				{
					error = true;
					break;
				}

				string str = null;
				try
				{
					str = process.StandardOutput.ReadLine();
				}
				catch
				{
					error = true;
					break;
				}

				if (string.Compare(str, markerLine, true) == 0)
				{
					worker.Worker = process;
					ServerRemoteManager.GetWorker(worker).Poll();
					break;
				}
			}

			return !error;
		}

		#endregion

		#region Использование и освобождение рабочих серверов

		/// <summary>
		/// Выбирает рабочий сервер из пула для выполнения процесса
		/// </summary>
		/// <returns>Метаданные рабочего сервера</returns>
		public WorkerMetadata AcquireWorker()
		{
			lock (_consumeLock)
			{
				while (true)
				{
					while (_availableWorkers.IsEmpty)
					{
						Monitor.Wait(this._consumeLock);
					}

					WorkerMetadata worker;
					if (_availableWorkers.TryDequeue(out worker))
					{
						lock (_increaseLock)
						{
							Monitor.Pulse(_increaseLock);
						}

						if (worker.Worker.HasExited)
						{
							_portPool.ReleasePort(worker.Port);
							continue;
						}

						return worker;
					}
				}
			}
		}

		/// <summary>
		/// Освобожает рабочий сервер
		/// </summary>
		/// <param name="wm">Метаданные рабочего сервера</param>
		public void FreeWorker(WorkerMetadata wm)
		{
			lock (_freeLock)
			{
				_workersToFree.Enqueue(wm);
				Monitor.Pulse(_freeLock);
			}
		}

		#endregion

		#region Автоматическое увеличение пула и освобождение рабочих процессов

		private void IncreasePoolCycle()
		{
			while (true)
			{
				lock (_increaseLock)
				{
					while (_availableWorkers.Count > _incrementThreshold)
					{
						Monitor.Wait(_increaseLock);
					}
				}

				while (_availableWorkers.Count <= _incrementThreshold)
				{
					int increaseSize = _incrementSize + _incrementThreshold - _availableWorkers.Count;
					int dop = Math.Max(Environment.ProcessorCount, increaseSize);
					Parallel.For(0, increaseSize, new ParallelOptions { MaxDegreeOfParallelism = dop }, _ =>
					{
						NewWorker();
					});
				}
			}
		}

		private void FreeWorkersCycle()
		{
			while (true)
			{
				lock (_freeLock)
				{
					while (_workersToFree.IsEmpty)
					{
						Monitor.Wait(_freeLock);
					}
				}

				while (!_workersToFree.IsEmpty)
				{
					WorkerMetadata worker;
					if (_workersToFree.TryDequeue(out worker))
					{
						CloseWorker(worker);
					}
				}
			}
		}

		/// <summary>
		/// Освобожает рабочий сервер
		/// </summary>
		/// <param name="wm">Метаданные рабочего сервера</param>
		private void CloseWorker(WorkerMetadata wm)
		{
			if (wm.Worker != null)
			{
				try
				{
					wm.Worker.StandardInput.WriteLine();
					wm.Worker.WaitForExit();
				}
				catch (Exception ex)
				{
					Trace.WriteLine(ex);
					try
					{
						wm.Worker.Kill();
					}
					catch (Exception ex2)
					{
						Trace.WriteLine(ex2);
					}
				}
				finally
				{
					_portPool.ReleasePort(wm.Port);
				}
			}
		}

		#endregion

		#region Реакция на обновление состава плагинов

		/// <summary>
		/// Обрабатывает событие по изменению набора плагинов
		/// </summary>
		/// <param name="pluginsFolder">Новая папка с плагинами</param>
		public void OnNewPlugins(string pluginsFolder)
		{
			_pluginsFolder = pluginsFolder;

			lock (_increaseLock)
			{
				lock (_consumeLock)
				{
					WorkerMetadata worker;
					while (_availableWorkers.TryDequeue(out worker))
					{
						this.FreeWorker(worker);
					}

					Monitor.Pulse(_consumeLock);
					Monitor.Pulse(_increaseLock);
				}
			}
		}

		#endregion
	}
}
