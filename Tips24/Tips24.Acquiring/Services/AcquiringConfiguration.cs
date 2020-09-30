using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring
{
	public class AcquiringConfiguration
	{
		public string SslCertPath { get; set; }

		public string PfxPassword { get; set; }

		public int HttpPort { get; set; }

		public int HttpsPort { get; set; }

		public string HookLogFolder { get; set; }
	}
}
