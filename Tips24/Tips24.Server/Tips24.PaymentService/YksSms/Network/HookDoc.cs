using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tips24.PaymentService.YksSms
{
	public class HookDoc
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("event")]
		public string Event { get; set; }

		[JsonProperty("object")]
		public PaymentDoc Payment { get; set; }
	}
}
