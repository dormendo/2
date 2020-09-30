using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService
{
	public class ModHookConfiguration
	{
		public bool Enabled { get; set; }
		public string Token { get; set; }
		public string AccountId { get; set; }
		public string LogFolder { get; set; }
	}
}
