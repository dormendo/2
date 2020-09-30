using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	internal class ConfigReportData
	{
		internal string ConfigFile { get; set; }
		internal string Uri { get; set; }
		internal int Threads { get; set; }
		internal string DataFile { get; set; }
		internal string RequestTemplateFile { get; set; }
		internal string TraceRequestFolder { get; set; }
		internal string TraceResponseFolder { get; set; }
		internal string TraceHttpResponseFolder { get; set; }
		internal string TraceErrorFolder { get; set; }
	}
	internal class ExecutionReportDurationData
	{
		internal long Overall { get; set; }
		internal long? Prepare { get; set; }
		internal long? RunTest { get; set; }
		internal long? Connect { get; set; }
		internal long? Request { get; set; }
		internal long? Convert { get; set; }
	}

	internal class ExecutionReportData
	{
		internal DateTime Started { get; set; }
		internal DateTime Completed { get; set; }
		internal ExecutionReportDurationData DurationInMilliseconds { get; set; }
	}

	internal class PhaseDurationData
	{
		internal DateTime FirstStartTime { get; set; }
		internal DateTime LastCompleteTime { get; set; }
		internal long FirstStartMs { get; set; }
		internal long LastCompleteMs { get; set; }
		internal long Duration { get; set; }
		internal long MinDuration { get; set; }
		internal long MaxDuration { get; set; }
		internal long MedianDuration { get; set; }
		internal decimal AverageDuration { get; set; }
	}

	internal class TestMetricsData
	{
		internal PhaseDurationData ConnectDuration { get; set; }
		internal PhaseDurationData SendDuration { get; set; }
		internal PhaseDurationData ReceiveDuration { get; set; }
		internal int SuccessOnConnect { get; set; }
		internal int SuccessOnRequest { get; set; }
		internal int ErrorsOnConnect { get; set; }
		internal int ErrorsOnRequest { get; set; }
		internal long? FirstResponseAppearedAfterTestStarted { get; set; }
		internal long? LastResponseAppearedAfterTestStarted { get; set; }
	}

	internal class ConvertMetricsData
	{
		internal PhaseDurationData ConvertDuration { get; set; }
		internal int SuccessOnConvert { get; set; }
		internal int ErrorsOnConvert { get; set; }
	}

	class ReportData
	{
		internal ConfigReportData ConfigParameters { get; set; }
		internal ExecutionReportData ExecutionReport { get; set; }
		internal TestMetricsData TestMetrics { get; set; }
		internal ConvertMetricsData ConvertMetrics { get; set; }
	}
}
