using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PayoutService.PayU
{
	public class InitiatePayoutResult
	{
		public int ResponseCode { get; set; }

		public int ResponseDescription { get; set; }

		public bool HasError { get; set; }

		public string Status { get; set; }

		public int StatusCode { get; set; }

		public int Tips24RequestId { get; set; }

		public string PayuRequestId { get; set; }

		public DateTime SentDateTime { get; set; }
	}
}
