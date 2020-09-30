using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tips24.PaymentService.ModApi;
using Tips24.PaymentService.SbAcquiring;
using Tips24.PaymentService.SbReg;
using Tips24.PaymentService.YksSms;

namespace Tips24.PaymentService
{
	public class PaymentServiceConfiguration
	{
		public string SslCertPath { get; set; }

		public string PfxPassword { get; set; }

		public int HttpPort { get; set; }

		public int HttpsPort { get; set; }

		public SbRegConfiguration SbReg { get; set; }

		public ModApiConfiguration ModApi { get; set; }

		public ModHookConfiguration ModHook { get; set; }

		public YksSmsConfiguration YksSms { get; set; }

		public SbAcquiringConfiguration SbAcquiring { get; set; }
	}
}
