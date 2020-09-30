using LoadTestClient.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTestClient
{
	internal class Engine : IDisposable
	{
		#region Конфигурация

		private Config _config;

		private string _configJsonFolder;

		private string _configJsonPath;

		private Uri _requestUri;

		private string _requestTemplateFile;

		private int _threads;

		private string _dataFilePath;

		private string _traceRequestFolder;

		private string _traceResponseFolder;

		private string _traceHttpResponseFolder;

		private string _traceErrorFolder;

		private string _host;

		private int _port;

		#endregion

		#region Рабочие процессы

		private List<Task> _tasks;

		private DateTime _startTime;

		private Stopwatch _testSw;

		private int _connected = 0;

		private int _connectErrors = 0;

		private int _sent = 0;

		private int _received = 0;

		private int _errors = 0;

		private List<WorkItem> _wiList = new List<WorkItem>();

		#endregion

		#region Преобразование результатов

		private int _converted = 0;

		private int _convertIndex = 0;

		private List<WorkItem> _toBeConverted;

		private const int CR = 13;

		private const int LF = 10;

		#endregion

		#region Прочие поля

		private string _error;

		private DataSource _dataSource;

		private string _headersTemplate;
		private string _requestTemplate;
		private byte[] _headersBytes;

		private ContentCompiler _contentCompiler;

		private MetricsProvider _metrics;

		private int _bufferSize;

		#endregion

		#region Конструктор

		internal Engine(Config config, string configJsonPath, MetricsProvider metrics)
		{
			_config = config;
			_configJsonPath = Path.GetFullPath(configJsonPath);
			_configJsonFolder = Path.GetDirectoryName(_configJsonPath);
			_metrics = metrics;
		}

		public void Dispose()
		{
			Free();
		}

		private void Free()
		{
			if (_wiList == null)
			{
				return;
			}

			for (int i = 0; i < _wiList.Count; i++)
			{
				WorkItem wi = _wiList[i];
				if (wi != null)
				{
					wi.Dispose();
				}
			}

			_wiList.Clear();
		}

		#endregion

		#region Проверка корректности конфигурации и подготовка данных к запуску

		internal bool Prepare()
		{
			try
			{
				_metrics.AddEvent(EventType.PrepareTestStart);
				if (!CheckConfig())
				{
					return false;
				}

				if (_threads > 10000)
				{
					_bufferSize = 32 * 1024;
				}
				else if (_threads > 1000)
				{
					_bufferSize = 80 * 1024;
				}
				else if (_threads > 500)
				{
					_bufferSize = 128 * 1024;
				}
				else if (_threads > 200)
				{
					_bufferSize = 256 * 1024;
				}
				else if (_threads > 100)
				{
					_bufferSize = 512 * 1024;
				}
				else
				{
					_bufferSize = 1024 * 1024;
				}

				try
				{
					_metrics.AddEvent(EventType.LoadDataFileStart);
					LoadDataFile();
				}
				finally
				{
					_metrics.AddEvent(EventType.LoadDataFileEnd);
				}

				try
				{
					_metrics.AddEvent(EventType.LoadTemplatesStart);
					LoadTemplates();
				}
				finally
				{
					_metrics.AddEvent(EventType.LoadTemplatesEnd);
				}

				PrepareAllTraceFolders();

				return true;
			}
			finally
			{
				_metrics.AddEvent(EventType.PrepareTestEnd);
			}
		}

		private bool CheckConfig()
		{
			try
			{
				_metrics.AddEvent(EventType.CheckConfigStart);
				if (_config == null)
				{
					_error = "Конфигурация не задана или имеет некорректный формат";
					return false;
				}

				if (_config.TestPlan == null)
				{
					_error = "Не задана секция конфигурации TestPlan";
					return false;
				}

				if (_config.TestPlan.Uri == null)
				{
					_error = "Не задана настройка конфигурации TestPlan.Uri";
					return false;
				}

				if (!Uri.TryCreate(_config.TestPlan.Uri, UriKind.Absolute, out Uri uri) || !uri.IsAbsoluteUri || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
				{
					_error = "Настройка конфигурации TestPlan.Uri не содержит корректный URL, соответствующий схеме http:// или https://";
					return false;
				}
				_requestUri = uri;
				_host = _requestUri.Host;
				_port = _requestUri.Port;

				if (_config.TestPlan.RequestTemplateFile == null)
				{
					_error = "Не задана настройка конфигурации TestPlan.RequestTemplateFile";
					return false;
				}

				_requestTemplateFile = Path.IsPathRooted(_config.TestPlan.RequestTemplateFile) ?
					_config.TestPlan.RequestTemplateFile : Path.Combine(_configJsonFolder, _config.TestPlan.RequestTemplateFile);
				if (!File.Exists(_requestTemplateFile))
				{
					_error = "Не найден файл с шаблоном запроса из настройки конфигурации TestPlan.RequestTemplateFile";
					return false;
				}

				_threads = _config.TestPlan.Threads;
				if (_threads < 1 || _threads > 10000)
				{
					_error = "Настройка конфигурации TestPlan.Threads должна иметь целочисленное значение в пределах [1; 10000]";
					return false;
				}

				if (_config.TestPlan.DataFile != null)
				{
					_dataFilePath = Path.IsPathRooted(_config.TestPlan.DataFile) ?
						_config.TestPlan.DataFile : Path.Combine(_configJsonFolder, _config.TestPlan.DataFile);
					if (!File.Exists(_dataFilePath))
					{
						_error = "Не найден файл с данными для генерации запросов из настройки конфигурации TestPlan.DataFile";
						return false;
					}
				}

				if (_config.TraceFolders != null && _config.TraceFolders.Request != null)
				{
					_traceRequestFolder = Path.IsPathRooted(_config.TraceFolders.Request) ?
						_config.TraceFolders.Request : Path.Combine(_configJsonFolder, _config.TraceFolders.Request);
				}
				else
				{
					_error = "Не задана папка из настройки конфигурации TraceFolders.Request для сохранения файлов с текстом запросов";
					return false;
				}

				if (_config.TraceFolders != null && _config.TraceFolders.Response != null)
				{
					_traceResponseFolder = Path.IsPathRooted(_config.TraceFolders.Response) ?
						_config.TraceFolders.Response : Path.Combine(_configJsonFolder, _config.TraceFolders.Response);
				}
				else
				{
					_error = "Не задана папка из настройки конфигурации TraceFolders.Response для сохранения файлов с текстом ответов";
					return false;
				}

				if (_config.TraceFolders != null && _config.TraceFolders.HttpResponse != null)
				{
					_traceHttpResponseFolder = Path.IsPathRooted(_config.TraceFolders.HttpResponse) ?
						_config.TraceFolders.HttpResponse : Path.Combine(_configJsonFolder, _config.TraceFolders.HttpResponse);
				}
				else
				{
					_error = "Не задана папка из настройки конфигурации TraceFolders.HttpResponse для сохранения файлов с телом ответов";
					return false;
				}

				if (_config.TraceFolders != null && _config.TraceFolders.Error != null)
				{
					_traceErrorFolder = Path.IsPathRooted(_config.TraceFolders.Error) ?
						_config.TraceFolders.Error : Path.Combine(_configJsonFolder, _config.TraceFolders.Error);
				}
				else
				{
					_error = "Не задана папка из настройки конфигурации TraceFolders.Error для сохранения файлов с сообщениями об ошибках при подключении или сетевом обмене";
					return false;
				}

				return true;
			}
			finally
			{
				_metrics.AddEvent(EventType.CheckConfigEnd);
			}
		}

		private void LoadTemplates()
		{
			_headersTemplate = Resources.HttpHeaders;
			_headersTemplate = _headersTemplate.Replace("{URL}", _requestUri.ToString()).Replace("{HOST}", _host);
			_headersBytes = Encoding.UTF8.GetBytes(_headersTemplate);

			_requestTemplate = File.ReadAllText(_requestTemplateFile);
			_contentCompiler = new ContentCompiler(_requestTemplate, _dataSource?.Columns);
			_contentCompiler.Compile();
		}

		private void LoadDataFile()
		{
			if (_dataFilePath == null)
			{
				return;
			}

			try
			{
				_metrics.AddEvent(EventType.LoadTemplatesStart);
				_dataSource = new DataSource(_dataFilePath);
				_dataSource.Load();
			}
			finally
			{
				_metrics.AddEvent(EventType.LoadTemplatesEnd);
			}
		}

		private void PrepareAllTraceFolders()
		{
			Console.WriteLine("Подготовка каталогов для трассировки...");

			try
			{
				_metrics.AddEvent(EventType.PrepareTraceFolderStart);
				PrepareTraceFolder(_traceRequestFolder);
				PrepareTraceFolder(_traceResponseFolder);
				PrepareTraceFolder(_traceHttpResponseFolder);
				PrepareTraceFolder(_traceErrorFolder);
			}
			finally
			{
				_metrics.AddEvent(EventType.PrepareTraceFolderEnd);
			}
		}

		private void PrepareTraceFolder(string folder)
		{
			if (Directory.Exists(folder))
			{
				try
				{
					_metrics.AddEvent(EventType.DeleteTraceFolderStart);
					Directory.Delete(folder, true);
				}
				finally
				{
					_metrics.AddEvent(EventType.DeleteTraceFolderEnd);
				}
			}
			Directory.CreateDirectory(folder);
		}

		internal string GetError()
		{
			return this._error;
		}

		#endregion

		#region Выполнение теста

		public void Run()
		{
			try
			{
				_testSw = Stopwatch.StartNew();
				_startTime = DateTime.Now;
				Console.WriteLine($"Время начала теста: {_startTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

				_metrics.AddRunTestStartEvent(_testSw);


				try
				{
					_metrics.AddEvent(EventType.CreateThreadsAndConnectStart);
					
					CreateThreads();
				}
				finally
				{
					_metrics.AddEvent(EventType.CreateThreadsAndConnectEnd);
				}

				try
				{
					_metrics.AddEvent(EventType.SendRequestsStart);
					
					StartThreads();
				}
				finally
				{
					_metrics.AddEvent(EventType.SendRequestsEnd);
				}
			}
			finally
			{
				_metrics.AddEvent(EventType.RunTestEnd);
			}

			ConvertResponses();
		}

		private void CreateThreads()
		{
			_tasks = new List<Task>();

			List<WorkItemData> wiDataList = new List<WorkItemData>();
			string toStrPattern = new string('0', _threads.ToString().Length);
			for (int i = 0; i < _threads; i++)
			{
				DataSourceRow row = null;
				if (_dataSource != null)
				{
					int rowIndex = i % _dataSource.Count;
					row = _dataSource.GetRow(rowIndex);
				}

				WorkItemData wiData = new WorkItemData()
				{
					ThreadNumber = i + 1,
					ThreadNumberStr = (i + 1).ToString(toStrPattern),
					Row = row,
					StartTime = _startTime
				};
				wiDataList.Add(wiData);

				WorkItem wi = new WorkItem
				{
					Data = wiData
				};

				_wiList.Add(wi);

				_tasks.Add(StartThreadAndConnectClient(wi));
			}

			_metrics.SetWorkItemDataList(wiDataList);

			Task[] taskArray = _tasks.ToArray();
			Console.WriteLine();

			while (!Task.WaitAll(taskArray, 1000))
			{
				Console.WriteLine($"{_testSw.ElapsedMilliseconds} мс. Подклчючено: {_connected}, ошибок подключения: {_connectErrors}, всего: {_connected + _connectErrors}");
			}

			Console.WriteLine($"{_testSw.ElapsedMilliseconds} мс. Подклчючено: {_connected}, ошибок подключения: {_connectErrors}, всего: {_connected + _connectErrors}");
			Console.WriteLine($"{_testSw.ElapsedMilliseconds} мс. Сетевые подключения созданы. Ошибок подключения: {_connectErrors}");
		}

		private void StartThreads()
		{
			Console.WriteLine();
			Console.WriteLine($"{_testSw.ElapsedMilliseconds} мс. Потоки в количестве {_connected} отправляют запросы на сервер");

			for (int i = 0; i < _threads; i++)
			{
				WorkItem wi = _wiList[i];
				if (wi == null || wi.Data.ConnectionFailed)
				{
					continue;
				}

				_tasks.Add(SendRequest(wi));
			}

			Task[] taskArray = _tasks.ToArray();
			Console.WriteLine();

			while (!Task.WaitAll(taskArray, 1000))
			{
				Console.WriteLine($"{_testSw.ElapsedMilliseconds} мс. Отправлено: {_sent}/{_connected}, получено: {_received}, ошибок: {_errors} ");
			}

			_testSw.Stop();
			Console.WriteLine($"{_testSw.ElapsedMilliseconds} мс. Отправлено: {_sent}/{_connected}, получено: {_received}, ошибок: {_errors} ");

			Console.WriteLine();
			Console.WriteLine($"Тест завершён. Время выполнения теста: {_testSw.ElapsedMilliseconds} мс. ");
			Console.WriteLine($"Всего подключений: {_threads}, из них успешных: {_connected}");
			Console.WriteLine($"Отправлено запросов: {_sent}, получено ответов: {_received}");
			Console.WriteLine($"Ошибок при отправке или обработке запроса: {_errors}");
		}

		#endregion

		#region Рабочие процессы

		private async Task StartThreadAndConnectClient(WorkItem wi)
		{
			await Task.Yield();
			_metrics.AddWiEvent(wi.Data, WiEventType.Started);

			try
			{
				wi.Client = new TcpClient();
				wi.Client.ReceiveBufferSize = 80 * 1024;
				_metrics.AddWiEvent(wi.Data, WiEventType.Connection);
				await wi.Client.ConnectAsync(_host, _port);
				Interlocked.Increment(ref _connected);
				_metrics.AddWiEvent(wi.Data, WiEventType.Connected);
			}
			catch (Exception ex)
			{
				_metrics.AddWiEvent(wi.Data, WiEventType.ConnectionError);
				wi.FailConnection();
				Interlocked.Increment(ref _connectErrors);
				WriteStringToFile(GetErrorOnConnectionFileName(wi), ex.ToString());
			}
		}

		private async Task SendRequest(WorkItem wi)
		{
			await Task.Yield();

			byte[] request = GetRequest(wi.Data);

			try
			{
				_metrics.AddWiEvent(wi.Data, WiEventType.RequestStarted);
				using (Stream s = wi.Client.GetStream())
				{
					await s.WriteAsync(request, 0, request.Length);
					s.Flush();
					_metrics.AddWiEvent(wi.Data, WiEventType.RequestSent);
					Interlocked.Increment(ref _sent);

					using (FileStream fs = new FileStream(GetResponseFileName(wi), FileMode.Create, FileAccess.Write, FileShare.None, 80 * 1024, FileOptions.WriteThrough))
					{
						bool firstBlock = true;
						byte[] buffer = new byte[80 * 1024];
						while (true)
						{
							int bytesRead = await s.ReadAsync(buffer, 0, buffer.Length);
							if (bytesRead == 0)
							{
								break;
							}

							if (firstBlock)
							{
								_metrics.AddWiEvent(wi.Data, WiEventType.FirstBlockReceived);
								firstBlock = false;
							}

							await fs.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
						}
					}

					_metrics.AddWiEvent(wi.Data, WiEventType.Received);

					Interlocked.Increment(ref _received);
					wi.Data.Success = true;
				}
			}
			catch (Exception ex)
			{
				_metrics.AddWiEvent(wi.Data, WiEventType.Error);
				Interlocked.Increment(ref _errors);
				wi.FailRequest();
				WriteStringToFile(GetErrorFileName(wi), ex.ToString());
			}
			finally
			{
				wi.CloseClient();
				_metrics.AddWiEvent(wi.Data, WiEventType.Disconnected);

				WriteByteArrayToFile(GetRequestFileName(wi), request);
				_metrics.AddWiEvent(wi.Data, WiEventType.RequestSaved);
			}
		}

		private byte[] GetRequest(WorkItemData wi)
		{
			byte[] request;
			using (MemoryStream ms = new MemoryStream())
			{
				_contentCompiler.WriteContent(ms, wi);
				request = ms.ToArray();
			}

			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(_headersBytes, 0, _headersBytes.Length);
				byte[] contentLength = Encoding.UTF8.GetBytes(request.Length.ToString() + "\r\n\r\n");
				ms.Write(contentLength, 0, contentLength.Length);

				ms.Write(request, 0, request.Length);
				return ms.ToArray();
			}
		}

		#endregion

		#region Преобразование ответов

		private void ConvertResponses()
		{
			if (_received == 0)
			{
				return;
			}

			Console.WriteLine();
			Console.WriteLine($"Преобразование TCP-ответов в HTTP-ответы. Время начала: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}");

			_toBeConverted = _wiList.Where(wi => wi.Data.Success).ToList();
			_convertIndex = -1;

			try
			{
				_metrics.AddEvent(EventType.Tcp2HttpResponsesConversionStart);
				Stopwatch sw = Stopwatch.StartNew();
				int taskCount = Math.Min(_toBeConverted.Count, 4/*Environment.ProcessorCount * 2*/);
				Task[] tasks = new Task[taskCount];
				for (int i = 0; i < tasks.Length; i++)
				{
					tasks[i] = ConvertResponsesWorker();
				}

				while (!Task.WaitAll(tasks, 1000))
				{
					Console.WriteLine($"{sw.ElapsedMilliseconds} мс. Преобразовано {_converted}/{_toBeConverted.Count}");
				}

				Console.WriteLine($"{sw.ElapsedMilliseconds} мс. Преобразовано {_converted}/{_toBeConverted.Count}. Преобразование завершено");
			}
			finally
			{
				_metrics.AddEvent(EventType.Tcp2HttpResponsesConversionEnd);
			}
		}

		private async Task ConvertResponsesWorker()
		{
			await Task.Yield();

			while (true)
			{
				int index = Interlocked.Increment(ref _convertIndex);
				if (index >= _toBeConverted.Count)
				{
					break;
				}

				WorkItem wi = _toBeConverted[index];

				try
				{
					_metrics.AddWiEvent(wi.Data, WiEventType.ConversionStarted);
					await ConvertSingleResponse(wi);
					_metrics.AddWiEvent(wi.Data, WiEventType.ConversionCompleted);
				}
				catch
				{
					wi.FailConvert();
					_metrics.AddWiEvent(wi.Data, WiEventType.Error);
				}

				Interlocked.Increment(ref _converted);
			}
		}

		private async Task ConvertSingleResponse(WorkItem wi)
		{
			List<string> httpHeaders = new List<string>();

			bool isChunked = false;
			using (FileStream fs = new FileStream(GetResponseFileName(wi), FileMode.Open, FileAccess.Read, FileShare.None, _bufferSize, FileOptions.SequentialScan))
			{
				while (true)
				{
					string line = ReadHttpHeaderLine(fs);
					if (line.Length == 0)
					{
						break;
					}

					if (string.Compare(line, "Transfer-Encoding: chunked", true) == 0)
					{
						isChunked = true;
					}
				}

				if (isChunked)
				{
					await ConvertAndSaveChunkedResponse(wi, fs);
				}
				else
				{
					await SaveStreamResponse(wi, fs);
				}
			}
		}

		private string ReadHttpHeaderLine(FileStream fs)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				while (true)
				{
					int b = fs.ReadByte();
					if (b == LF)
					{
						break;
					}
					else if (b != CR)
					{
						ms.WriteByte((byte)b);
					}
				}

				return Encoding.ASCII.GetString(ms.ToArray());
			}
		}

		private async Task ConvertAndSaveChunkedResponse(WorkItem wi, FileStream source)
		{
			using (FileStream destination = new FileStream(GetHttpResponseFileName(wi), FileMode.Create, FileAccess.Write, FileShare.None, _bufferSize, FileOptions.WriteThrough))
			{
				byte[] buffer = new byte[_bufferSize];
				while (true)
				{
					string line = ReadHttpHeaderLine(source);
					int chunkSize = int.Parse(line.Trim(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
					if (chunkSize == 0)
					{
						break;
					}

					int bytesToRead = chunkSize;
					while (bytesToRead > 0)
					{
						int bytesRead = await source.ReadAsync(buffer, 0, Math.Min(buffer.Length, bytesToRead));
						if (bytesRead == 0)
						{
							break;
						}

						await destination.WriteAsync(buffer, 0, bytesRead);
						bytesToRead -= bytesRead;
					}

					ReadHttpHeaderLine(source);
				}
			}
		}

		private async Task SaveStreamResponse(WorkItem wi, FileStream source)
		{
			using (FileStream destination = new FileStream(GetHttpResponseFileName(wi), FileMode.Create, FileAccess.Write, FileShare.None))
			{
				await source.CopyToAsync(destination);
			}
		}

		#endregion

		#region Имена файлов трассировки

		private string GetRequestFileName(WorkItem wi)
		{
			return Path.Combine(_traceRequestFolder, "Request" + wi.Data.ThreadNumberStr + ".txt");
		}

		private string GetResponseFileName(WorkItem wi)
		{
			return Path.Combine(_traceResponseFolder, "Response" + wi.Data.ThreadNumberStr + ".txt");
		}

		private string GetErrorOnConnectionFileName(WorkItem wi)
		{
			return Path.Combine(_traceErrorFolder, "ErrorOnConnect" + wi.Data.ThreadNumberStr + ".txt");
		}

		private string GetErrorFileName(WorkItem wi)
		{
			return Path.Combine(_traceErrorFolder, "Error" + wi.Data.ThreadNumberStr + ".txt");
		}

		private string GetHttpResponseFileName(WorkItem wi)
		{
			return Path.Combine(_traceHttpResponseFolder, "HttpResponse" + wi.Data.ThreadNumberStr + ".txt");
		}

		#endregion

		#region Запись в файлы

		private void WriteStringToFile(string filePath, string str)
		{
			byte[] eb = Encoding.UTF8.GetBytes(str);
			WriteByteArrayToFile(filePath, eb);
		}

		private static void WriteByteArrayToFile(string filePath, byte[] eb)
		{
			using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4 * 1024, FileOptions.WriteThrough))
			{
				fs.Write(eb, 0, eb.Length);
			}
		}

		#endregion

		#region Возврат конфигурации

		internal ConfigReportData GetConfigReportData()
		{
			ConfigReportData config = new ConfigReportData();
			config.ConfigFile = _configJsonPath;
			config.DataFile = _dataFilePath;
			config.RequestTemplateFile = _requestTemplateFile;
			config.Threads = _threads;
			config.TraceRequestFolder = _traceRequestFolder;
			config.TraceResponseFolder = _traceResponseFolder;
			config.TraceHttpResponseFolder = _traceHttpResponseFolder;
			config.TraceErrorFolder = _traceErrorFolder;
			config.Uri = _requestUri.ToString();

			return config;
		}

		#endregion
	}
}
