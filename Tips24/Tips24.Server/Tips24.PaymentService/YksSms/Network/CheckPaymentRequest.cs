using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tips24.PaymentService.YksSms
{
	public class CheckPaymentRequestData : NetworkRequestData<string, PaymentDoc>
	{
		public CheckPaymentRequestData(string requestData)
		{
			this.RequestData = requestData;
		}
	}

	public class CheckPaymentRequest : YksSmsRequestBase
	{
		private CheckPaymentRequest(HttpClient client, ILogger logger)
			: base(client, logger)
		{
		}

		public async Task Run(CheckPaymentRequestData data)
		{
			try
			{
				bool isSuccessStatusCode = false;

				await RepeatedCall.ActionAsync(async () =>
				{
					using (HttpResponseMessage response = await this.GetResponse(data.RequestData, HttpMethod.Get, null))
					{
						string str = await response.Content.ReadAsStringAsync();
						ResponseData<PaymentDoc> result = data.CreateResponseInstance();
						result.RequestJson = "";
						result.ResponseJson = str;
						isSuccessStatusCode = response.IsSuccessStatusCode;
					}
				}, 5, 500);

				data.DeserializeResults(isSuccessStatusCode);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке запроса CheckRequest и десериализации ответа");
				data.ExceptionData = ex;
			}
		}

		public static async Task Run(HttpClient client, CheckPaymentRequestData data, ILogger logger)
		{
			CheckPaymentRequest request = new CheckPaymentRequest(client, logger);
			await request.Run(data);
		}
	}
}
