using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class MetricsProvider
	{
		List<TestEvent> _globalEvents = new List<TestEvent>();

		DateTime _globalStartDt;

		Stopwatch _globalSw;

		Stopwatch _runTestSw;

		string _configJson;

		Config _config;

		Dictionary<EventType, EventType> _eventPairs = new Dictionary<EventType, EventType>();

		List<WorkItemData> _wiDataList;

		
		internal MetricsProvider(Stopwatch globalSw, DateTime globalStartDt)
		{
			_globalSw = globalSw;
			_globalStartDt = globalStartDt;
		}

		internal void SetConfig(string configJson, Config config)
		{
			_configJson = configJson;
			_config = config;
		}

		internal void SetWorkItemDataList(List<WorkItemData> list)
		{
			_wiDataList = list;
		}

		internal void AddEvent(EventType type)
		{
			long? em = null;
			if (_runTestSw != null)
			{
				em = _runTestSw.ElapsedMilliseconds;
			}

			TestEvent e = new TestEvent { Type = type, Dt = DateTime.Now, ElapsedMilliseconds = em, GlobalElapsedMilliseconds = _globalSw.ElapsedMilliseconds };
			_globalEvents.Add(e);
		}

		internal void AddRunTestStartEvent(Stopwatch sw)
		{
			_runTestSw = sw;
			AddEvent(EventType.RunTestStart);
		}

		internal void AddWiEvent(WorkItemData widata, WiEventType type)
		{
			long? em = null;
			if (_runTestSw != null)
			{
				em = _runTestSw.ElapsedMilliseconds;
			}

			WiTestEvent e = new WiTestEvent { Type = type, Dt = DateTime.Now, ElapsedMilliseconds = em, GlobalElapsedMilliseconds = _globalSw.ElapsedMilliseconds };
			widata.Events.Add(e);
		}

		internal void ProcessMetrics()
		{
			_eventPairs.Add(EventType.LoadConfigEnd, EventType.LoadConfigStart);
			_eventPairs.Add(EventType.PrepareTestEnd, EventType.PrepareTestStart);
			_eventPairs.Add(EventType.RunTestEnd, EventType.RunTestStart);
			_eventPairs.Add(EventType.Tcp2HttpResponsesConversionEnd, EventType.Tcp2HttpResponsesConversionStart);
			_eventPairs.Add(EventType.LoadConfigEnd, EventType.LoadConfigStart);
			_eventPairs.Add(EventType.LoadConfigEnd, EventType.LoadConfigStart);
			_eventPairs.Add(EventType.LoadConfigEnd, EventType.LoadConfigStart);
			_eventPairs.Add(EventType.Complete, EventType.Start);
		}

		internal void WriteLogs(ConfigReportData configReportData)
		{
			ReportData ed = PrepareReportData(configReportData);
			WriteReportData(ed);
		}

		#region Метрики

		private ReportData PrepareReportData(ConfigReportData crd)
		{
			ReportData ed = new ReportData();
			ed.ConfigParameters = crd;

			long? prepare = GetDuration(EventType.PrepareTestStart, EventType.PrepareTestEnd);
			long? runTest = GetDuration(EventType.RunTestStart, EventType.RunTestEnd);
			long? connect = GetDuration(EventType.CreateThreadsAndConnectStart, EventType.CreateThreadsAndConnectEnd);
			long? request = GetDuration(EventType.SendRequestsStart, EventType.SendRequestsEnd);
			long? convert = GetDuration(EventType.Tcp2HttpResponsesConversionStart, EventType.Tcp2HttpResponsesConversionEnd);
			ExecutionReportDurationData dd = new ExecutionReportDurationData
			{
				Overall = _globalEvents.Last().GlobalElapsedMilliseconds,
				Prepare = prepare,
				RunTest = runTest,
				Connect = connect,
				Request = request,
				Convert = convert
			};

			ed.ExecutionReport = new ExecutionReportData
			{
				Started = _globalStartDt,
				Completed = _globalEvents.Last().Dt,
				DurationInMilliseconds = dd
			};

			int failedOnConnect = _wiDataList.Count(d => d.ConnectionFailed);
			int failedOnRequest = _wiDataList.Count(d => d.RequestFailed);
			int failedOnConvert = _wiDataList.Count(d => d.ConvertFailed);
			List<long> responsesAppearedTimes = _wiDataList.Select(d => d.Events.FirstOrDefault(e => e.Type == WiEventType.FirstBlockReceived)?.ElapsedMilliseconds)
				.Where(em => em.HasValue).Select(em => em.Value).ToList();
			long? firstResponseAppeared = (responsesAppearedTimes.Count == 0 ? (long?)null : responsesAppearedTimes.Min());
			long? lastResponseAppeared = (responsesAppearedTimes.Count == 0 ? (long?)null : responsesAppearedTimes.Max());

			PhaseDurationData connectDuration = GetPhaseDurationData(WiEventType.Connection, WiEventType.Connected);
			PhaseDurationData sendDuration = GetPhaseDurationData(WiEventType.RequestStarted, WiEventType.RequestSent);
			PhaseDurationData receiveDuration = GetPhaseDurationData(WiEventType.RequestSent, WiEventType.Received);
			PhaseDurationData convertDuration = GetPhaseDurationData(WiEventType.ConversionStarted, WiEventType.ConversionCompleted);

			ed.TestMetrics = new TestMetricsData
			{
				SuccessOnConnect = crd.Threads - failedOnConnect,
				SuccessOnRequest = crd.Threads - failedOnConnect - failedOnRequest,
				ErrorsOnConnect = failedOnConnect,
				ErrorsOnRequest = failedOnRequest,
				ConnectDuration = connectDuration,
				SendDuration = sendDuration,
				ReceiveDuration = receiveDuration,
				FirstResponseAppearedAfterTestStarted = firstResponseAppeared,
				LastResponseAppearedAfterTestStarted = lastResponseAppeared
			};

			ed.ConvertMetrics = new ConvertMetricsData
			{
				SuccessOnConvert = crd.Threads - failedOnConnect - failedOnRequest,
				ErrorsOnConvert = failedOnConvert,
				ConvertDuration = convertDuration
			};

			return ed;
		}

		private long? GetDuration(EventType start, EventType complete)
		{
			long? duration = null;
			TestEvent se = _globalEvents.FirstOrDefault(e => e.Type == start);
			TestEvent ce = _globalEvents.FirstOrDefault(e => e.Type == complete);
			if (se != null && ce != null)
			{
				duration = ce.GlobalElapsedMilliseconds - se.GlobalElapsedMilliseconds;
			}

			return duration;
		}

		private PhaseDurationData GetPhaseDurationData(WiEventType start, WiEventType complete)
		{
			DateTime? minStartTime = null;
			DateTime? maxCompleteTime = null;
			long? minStartEm = null;
			long? maxCompleteEm = null;

			List<long> durationList = new List<long>();
			foreach (WorkItemData wid in _wiDataList)
			{
				WiTestEvent se = wid.Events.FirstOrDefault(e => e.Type == start);
				WiTestEvent ce = wid.Events.FirstOrDefault(e => e.Type == complete);
				if (se != null && ce != null)
				{
					durationList.Add(ce.GlobalElapsedMilliseconds - se.GlobalElapsedMilliseconds);

					if (!minStartTime.HasValue || minStartTime.Value > se.Dt)
					{
						minStartTime = se.Dt;
					}

					if (!maxCompleteTime.HasValue || maxCompleteTime.Value < ce.Dt)
					{
						maxCompleteTime = ce.Dt;
					}

					if (!minStartEm.HasValue || minStartEm.Value > se.ElapsedMilliseconds.Value)
					{
						minStartEm = se.ElapsedMilliseconds;
					}

					if (!maxCompleteEm.HasValue || maxCompleteEm.Value < ce.ElapsedMilliseconds.Value)
					{
						maxCompleteEm = ce.ElapsedMilliseconds;
					}
				}
			}

			if (durationList.Count == 0)
			{
				return null;
			}

			durationList.Sort();
			long medianDuration = ((durationList.Count % 2) == 1 ? durationList[(durationList.Count - 1) / 2] : (durationList[durationList.Count / 2 - 1] + durationList[durationList.Count / 2]) / 2);
			decimal avgDuration = Math.Round(new decimal(durationList.Average()), 3);

			PhaseDurationData data = new PhaseDurationData
			{
				FirstStartTime = minStartTime.Value,
				LastCompleteTime = maxCompleteTime.Value,
				Duration = maxCompleteEm.Value - minStartEm.Value,
				FirstStartMs = minStartEm.Value,
				LastCompleteMs = maxCompleteEm.Value,
				MinDuration = durationList.First(),
				MaxDuration = durationList.Last(),
				MedianDuration = medianDuration,
				AverageDuration = avgDuration
			};

			return data;
		}

		private void WriteReportData(ReportData ed)
		{
			string dtPattern = "yyyy-MM-dd HH:mm:ss.fff";
			string configFolder = Path.GetDirectoryName(ed.ConfigParameters.ConfigFile);
			string reportFile = Path.IsPathRooted(_config.TestPlan.ReportFile) ?
				_config.TestPlan.ReportFile : Path.Combine(configFolder, _config.TestPlan.ReportFile);
			using (StreamWriter sw = new StreamWriter(Path.Combine(configFolder, _config.TestPlan.ReportFile)))
			{
				sw.WriteLine("Конфигурация запуска приложения для нагрузочного тестирования:");
				sw.WriteLine($"Конфигурационный файл: {ed.ConfigParameters.ConfigFile}");
				sw.WriteLine($"Адрес вызова: {ed.ConfigParameters.Uri}");
				sw.WriteLine($"Файл с шаблоном запроса: {ed.ConfigParameters.RequestTemplateFile}");
				sw.WriteLine($"Файл с данными для генерации запросов: {ed.ConfigParameters.DataFile ?? "не задан"}");
				sw.WriteLine($"Количество одновременных запросов: {ed.ConfigParameters.Threads}");
				sw.WriteLine($"Папка для файлов с текстом запросов: {ed.ConfigParameters.TraceRequestFolder}");
				sw.WriteLine($"Папка для файлов с текстом ответов (TCP): {ed.ConfigParameters.TraceResponseFolder}");
				sw.WriteLine($"Папка для файлов с телом ответов (HTTP): {ed.ConfigParameters.TraceHttpResponseFolder}");
				sw.WriteLine($"Папка для файлов с текстом ошибок при соединении или сетевом обмене: {ed.ConfigParameters.TraceErrorFolder}");
				sw.WriteLine();

				sw.WriteLine($"Общая инфомация о запуске приложения для нагрузочного тестирования:");
				sw.WriteLine($"Начало работы: {ed.ExecutionReport.Started.ToString(dtPattern)}");
				sw.WriteLine($"Завершение работы: {ed.ExecutionReport.Completed.ToString(dtPattern)}");
				sw.WriteLine($"Общая продолжительность выполнения (мс): {ed.ExecutionReport.DurationInMilliseconds.Overall}");
				sw.WriteLine($"Продолжительность фазы по подготовке к запуску теста (мс): {ed.ExecutionReport.DurationInMilliseconds.Prepare?.ToString() ?? "не выполнялась"}");
				sw.WriteLine($"Продолжительность основной фазы тестирования нагрузки (мс): {ed.ExecutionReport.DurationInMilliseconds.RunTest?.ToString() ?? "не выполнялась"}");
				sw.WriteLine($"    в т.ч. открытие соединений к сервису (мс): {ed.ExecutionReport.DurationInMilliseconds.Connect?.ToString() ?? "не выполнялась"}");
				sw.WriteLine($"    в т.ч. отправка запросов и получение ответов (мс): {ed.ExecutionReport.DurationInMilliseconds.Request?.ToString() ?? "не выполнялась"}");
				sw.WriteLine($"Продолжительность фазы по преобразованию ответов сервиса (мс): {ed.ExecutionReport.DurationInMilliseconds.Convert?.ToString() ?? "не выполнялась"}");
				sw.WriteLine();

				string firstResponseAppeared = (ed.TestMetrics.FirstResponseAppearedAfterTestStarted?.ToString() ?? "не выполнялось");
				string lastResponseAppeared = (ed.TestMetrics.LastResponseAppearedAfterTestStarted?.ToString() ?? "не выполнялось");
				sw.WriteLine($"Основные метрики нагрузочного тестирования:");
				sw.WriteLine($"Успешных подключений: {ed.TestMetrics.SuccessOnConnect}");
				sw.WriteLine($"Ошибок при подключении: {ed.TestMetrics.ErrorsOnConnect}");
				sw.WriteLine($"Успешных запросов: {ed.TestMetrics.SuccessOnRequest}");
				sw.WriteLine($"Ошибок при выполнении запроса: {ed.TestMetrics.ErrorsOnRequest}");
				sw.WriteLine($"Время с начала тестирования до получения первой порции данных первого пришедшего ответа (мс): {firstResponseAppeared}");
				sw.WriteLine($"Время с начала тестирования до получения первой порции данных последнего пришедшего ответа (мс): {lastResponseAppeared}");
				sw.WriteLine();

				if (ed.TestMetrics.ConnectDuration != null)
				{
					sw.WriteLine("Статистические метрики для фазы подключения к сервису:");
					WritePhaseData(sw, ed.TestMetrics.ConnectDuration, "начала первого подключения", "окончания последнего подключения");
				}
				else
				{
					sw.WriteLine("Статистические метрики для фазы подключения к сервису: данных нет");
				}

				sw.WriteLine();

				if (ed.TestMetrics.ConnectDuration != null)
				{
					sw.WriteLine("Статистические метрики для фазы отправки запроса к сервису:");
					WritePhaseData(sw, ed.TestMetrics.SendDuration, "начала отправки первого запроса", "окончания отправки последнего запроса");
				}
				else
				{
					sw.WriteLine("Статистические метрики для фазы отправки запроса к сервису: данных нет");
				}

				sw.WriteLine();

				if (ed.TestMetrics.ConnectDuration != null)
				{
					sw.WriteLine("Статистические метрики для фазы получения ответа от сервиса:");
					WritePhaseData(sw, ed.TestMetrics.ReceiveDuration, "начала получения первого ответа", "окончания получения последнего ответа");
				}
				else
				{
					sw.WriteLine("Статистические метрики для фазы получения ответа от сервиса: данных нет");
				}

				sw.WriteLine();

				if (ed.TestMetrics.ConnectDuration != null)
				{
					sw.WriteLine("Статистические метрики для фазы преобразования ответов сервиса:");
					sw.WriteLine($"Успешных преобразований: {ed.ConvertMetrics.SuccessOnConvert}");
					sw.WriteLine($"Ошибок при преобразованиях: {ed.ConvertMetrics.ErrorsOnConvert}");
					WritePhaseData(sw, ed.ConvertMetrics.ConvertDuration, "начала первого преобразования", "окончания последнего преобразования");
				}
				else
				{
					sw.WriteLine("Статистические метрики для фазы преобразования ответов сервиса: данных нет");
				}
			}
		}

		private void WritePhaseData(StreamWriter sw, PhaseDurationData d, string start, string complete)
		{
			string dtPattern = "yyyy-MM-dd HH:mm:ss.fff";
			sw.WriteLine($"Время {start}: {d.FirstStartTime.ToString(dtPattern)}, в мс. с начала нагрузочного теста: {d.FirstStartMs}");
			sw.WriteLine($"Время {complete}: {d.LastCompleteTime.ToString(dtPattern)}, в мс. с начала нагрузочного теста: {d.LastCompleteMs}");
			sw.WriteLine($"Время в мс. с {start} до {complete}: {d.Duration}");
			sw.WriteLine($"Минимальная продолжителость фазы для одного запроса (мс): {d.MinDuration}");
			sw.WriteLine($"Максимальная продолжителость фазы для одного запроса (мс): {d.MaxDuration}");
			sw.WriteLine($"Средняя продолжительность фазы для одного запроса (мс): {d.AverageDuration}");
			sw.WriteLine($"Медианная продолжительность фазы для одного запроса (мс): {d.MedianDuration}");
		}

		#endregion
	}
}
