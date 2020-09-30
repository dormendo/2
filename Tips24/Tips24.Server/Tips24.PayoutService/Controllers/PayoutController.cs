using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tips24.PayoutService.Controllers
{
	[ApiController]
	public class PayoutController : ControllerBase
	{
		[HttpPost("Payout/Through/PayU")]
		public async Task<IActionResult> Payout(PayoutRequestData data)
		{
			if (data.Provider == "PAYU00")
			{
				PayU.PayuHandler handler = this.HttpContext.RequestServices.GetService(typeof(PayU.PayuHandler)) as PayU.PayuHandler;
				await handler.InitiatePayout(data);
			}

			return new ContentResult() { Content = "Ошибка. Не задана платёжная система", StatusCode = 400 };
		}
	}
}