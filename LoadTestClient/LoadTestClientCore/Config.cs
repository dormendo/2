using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClientCore
{
	class Config
	{
		public TestPlanSettings TestPlan { get; set; }
		public LogsSettings Logs { get; set; }

		public TraceSettings Trace { get; set; }
	}

	class TestPlanSettings
	{
		public string Uri { get; set; }
		public string RequestTemplateFile { get; set; }
		public int Threads { get; set; }
		public int RequestsByThread { get; set; }
		public string DataCsvFile { get; set; }
	}

	class LogsSettings
	{
		public string MetricsFile { get; set; }
		public string LogCsvFile { get; set; }
	}

	class TraceSettings
	{
		public string Folder { get; set; }
		public TraceRequestMode TraceRequest { get; set; }
	}

	class TraceSamplingSettings
	{
		public double SampleRate { get; set; }
		public string DataField { get; set; }
	}

	enum TraceRequestMode
	{
		SameFile,
		Yes,
		No
	}
}
