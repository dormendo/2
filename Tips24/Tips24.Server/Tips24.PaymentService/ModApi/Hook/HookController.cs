using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace Tips24.PaymentService
{
	[ApiController]
	public class HookController : ControllerBase
	{
		private IHostingEnvironment _he;
		private SqlServer _sqlServer;

		private ModHookConfiguration Config { get { return Startup.Config.ModHook; } }

		public HookController(IHostingEnvironment he, SqlServer sqlServer)
		{
			this._he = he;
			this._sqlServer = sqlServer;
		}

		[HttpPost("test/Payment")]
		public async Task<IActionResult> ProcessPayment(Payment payment)
		{
			try
			{
				ShareService shareService = (ShareService)HttpContext.RequestServices.GetService(typeof(ShareService));
				bool result;
				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlTransaction tx = conn.BeginTransaction())
					{
						result = await shareService.ProceedPayment(payment, conn, tx);
						tx.Commit();
					}
				}

				if (result)
				{
					return new JsonResult(payment);
				}
				else
				{
					return new JsonErrorResult(
						new ErrorResponse("payment_exists", "В базе данных существует платёж с такими параметрами (источник данных, номер документа и внешний идентификатор платежа)"));
				}
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		[HttpPost("ModuleBank/WebHook")]
		public async Task<HttpResponseMessage> NewOperation()
		{
			if (!Config.Enabled)
			{
				return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
			}

			string requestBody;
			using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
			{
				requestBody = await reader.ReadToEndAsync();
			}


			//	HookCompanyData operation = JsonConvert.DeserializeObject<HookCompanyData>(requestBody, Startup.JsonSettings);
			//if (operation.CheckHash(Startup.Config.ModHook.Token))
			//{
				return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
			//}
			//else
			//{
			//	return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
			//}
		}

		private ErrorResponse GetExceptionResponse(Exception ex, string result = "common_error")
		{
			ErrorResponse response = new ErrorResponse(result, ex.Message);

			if (this._he.IsDevelopment())
			{
				response.ExtendedInfo = ex.ToString();
			}
			return response;
		}
	}
}