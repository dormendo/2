using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PayoutService.PayU
{
	public class PayuConfiguration
	{
		public string PayoutUrl { get; set; }

		public string CheckStatusUrl { get; set; }

		public string MerchantCode { get; set; }

		public string SecretKey { get; set; }
	}
}
