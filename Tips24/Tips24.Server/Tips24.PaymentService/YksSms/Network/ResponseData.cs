using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.YksSms
{
	public class ResponseData
	{
		public string RequestJson { get; set; }

		public bool HasError { get { return this.Error != null; } }

		public string ResponseJson { get; set; }

		public ErrorResponse Error { get; set; }
	}

	public class ResponseData<T> : ResponseData
	{
		public T Response { get; set; }
	}
}
