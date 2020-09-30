using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Tips24.PaymentService
{
	public class JsonErrorResult : JsonResult
	{
		public JsonErrorResult(object value)
			: base(value)
		{
			this.StatusCode = 400;
		}
	}
}
