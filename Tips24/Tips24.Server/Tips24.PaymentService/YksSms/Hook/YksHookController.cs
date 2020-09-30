using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.YksSms
{
	[ApiController]
	public class YksHookController : ControllerBase
	{
		private IHostingEnvironment _he;

		private YksSmsConfiguration Config { get { return Startup.Config.YksSms; } }

		public YksHookController(IHostingEnvironment he)
		{
			this._he = he;
		}

		[HttpPost("YandexKassa/Notification")]
		public async Task<HttpResponseMessage> YksHook()
		{
			string requestBody;
			using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
			{
				requestBody = await reader.ReadToEndAsync();
			}

			bool success = false;
			YksSms.YksSmsPaymentHostedService service = YksSms.YksSmsPaymentHostedService.Instance;
			if (service != null)
			{
				success = await service.ProcessPaymentFromHook(requestBody);
			}

			HttpResponseMessage message = new HttpResponseMessage(success ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);
			return message;
		}
	}
}