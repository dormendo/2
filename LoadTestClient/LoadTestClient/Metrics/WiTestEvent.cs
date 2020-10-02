﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class WiTestEvent
	{
		internal WiEventType Type { get; set; }

		internal DateTime Dt { get; set; }

		internal long? ElapsedMilliseconds { get; set; }

		internal long GlobalElapsedMilliseconds { get; set; }
	}
}