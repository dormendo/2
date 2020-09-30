using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class Config
	{
		public TestPlanSettings TestPlan { get; set; }

		public TraceFolderSettings TraceFolders { get; set; }
	}

	class TestPlanSettings
	{
		public string Uri { get; set; }
		public string RequestTemplateFile { get; set; }
		public int Threads { get; set; }
		public string DataFile { get; set; }
		public string ReportFile { get; set; }
	}

	class TraceFolderSettings
	{
		public string Request { get; set; }
		public string Response { get; set; }
		public string HttpResponse { get; set; }
		public string Error { get; set; }
	}

	enum TraceRequestMode
	{
		SameFile,
		Yes,
		No
	}
}
