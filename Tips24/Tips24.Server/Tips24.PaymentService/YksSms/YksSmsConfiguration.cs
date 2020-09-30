using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.YksSms
{
	public class YksSmsConfiguration
	{
		public bool Enabled { get; set; }

		public string ApiUrl { get; set; }

		public string ShopId { get; set; }

		public string SecretKey { get; set; }

		public string LogFolder { get; set; }
	}
}
