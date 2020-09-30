using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tips24.Acquiring.Controllers.PaymentPage;

namespace Tips24.Acquiring.Controllers
{
	[ApiController]
	public class PaymentPageController : ControllerBase
	{
		private SqlServer _sqlServer;
		private AcquiringConfiguration _config;

		public PaymentPageController(SqlServer sqlServer)
		{
			this._sqlServer = sqlServer;
			this._config = Startup.Config;
		}

		[HttpGet()]
		[Route("pay/get_receiver")]
		public async Task<IActionResult> GetReceiver(string receiver)
		{
			return await new GetReceiverHandler(_sqlServer).Handle(receiver);
		}

		[HttpGet]
		[Route("pay/new_gpay_request")]
		public async Task<IActionResult> NewGpayRequest(string receiver, int sum, string ip, string method)
		{
			return await new NewRequestHandler(_sqlServer).Handle(receiver, sum, ip, PaymentType.GooglePay, method);
		}

		[HttpGet]
		[Route("pay/new_card_request")]
		public async Task<IActionResult> NewCardRequest(string receiver, int sum, string ip)
		{
			return await new PaymentPage.NewRequestHandler(_sqlServer).Handle(receiver, sum, ip, PaymentType.Card);
		}

		[HttpGet]
		[Route("pay/save_orderid")]
		public async Task<IActionResult> SaveOrderId(int requestId, string orderId)
		{
			return await new SaveOrderIdHandler(_sqlServer).Handle(requestId, orderId);
		}

		[HttpGet]
		[Route("pay/fail_request")]
		public async Task<IActionResult> FailRequest(int requestId)
		{
			return await new FailRequestHandler(_sqlServer).Handle(requestId);
		}

		[HttpGet]
		[Route("pay/succeed_request")]
		public async Task<IActionResult> SucceedRequest(int requestId)
		{
			return await new SucceedRequestHandler(_sqlServer).Handle(requestId);
		}

		[RequireHttps]
		[Route("sberbank/acquiring")]
		public async Task<IActionResult> AcquiringCallback(int orderNumber, string mdOrder, string operation, int status)
		{
			IActionResult result = null;

			if (operation == "deposited")
			{
				if (status == 0)
				{
					result = await new FailRequestHandler(_sqlServer).Handle(orderNumber);
				}
				else if (status == 1)
				{
					result = await new SucceedRequestHandler(_sqlServer).Handle(orderNumber);
				}
			}

			if (result == null)
			{
				result = new StatusCodeResult(200);
			}

			string queryString = this.HttpContext.Request.QueryString.Value;
			string filePath = Path.Combine(_config.HookLogFolder, "Hook-" + DateTime.Now.ToString("yyMMdd-HHmmss.fff") + "-" + orderNumber.ToString("0000000000") + ".log");
			await System.IO.File.WriteAllTextAsync(filePath, queryString);
			return result;
		}
	}
}
