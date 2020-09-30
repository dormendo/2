using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class WorkItemData
	{
		internal DataSourceRow Row { get; set; }

		internal int ThreadNumber { get; set; }

		internal DateTime StartTime { get; set; }

		internal string ThreadNumberStr { get; set; }

		internal List<WiTestEvent> Events { get; } = new List<WiTestEvent>();

		internal bool Success { get; set; }

		internal bool ConnectionFailed { get; set; }

		internal bool RequestFailed { get; set; }

		internal bool ConvertFailed { get; set; }
	}
}
