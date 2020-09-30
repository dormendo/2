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
	public class CreatePaymentRequestData : NetworkRequestData<CreatePaymentInDoc, PaymentDoc>
	{
		public CreatePaymentRequestData(CreatePaymentInDoc requestData)
		{
			this.RequestData = requestData;
		}
	}

	public class CreatePaymentRequest : YksSmsRequestBase
	{
		private string _idempotenceKey;

		private CreatePaymentRequest(HttpClient client, ILogger logger)
			: base(client, logger)
		{
		}

		public async Task Run(CreatePaymentRequestData data)
		{
			try
			{
				string content = JsonConvert.SerializeObject(data.RequestData, Startup.JsonSettings);
				this._idempotenceKey = "CreateRequest_" + data.RequestData.Metadata.RequestId.ToString();
				bool isSuccessStatusCode = false;

				await RepeatedCall.ActionAsync(async () =>
				{
					using (HttpResponseMessage response = await this.GetResponse("", HttpMethod.Post, content))
					{
						string str = await response.Content.ReadAsStringAsync();
						ResponseData<PaymentDoc> result = data.CreateResponseInstance();
						result.RequestJson = content;
						result.ResponseJson = str;
						isSuccessStatusCode = response.IsSuccessStatusCode;
					}
				}, 5, 500);

				data.DeserializeResults(isSuccessStatusCode);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке запроса CreateRequest и десериализации ответа");
				data.ExceptionData = ex;
			}
		}

		protected override void AddRequestHeaders(HttpRequestMessage request)
		{
			request.Headers.Add("Idempotence-Key", this._idempotenceKey);
		}

		public static async Task Run(HttpClient client, CreatePaymentRequestData data, ILogger logger)
		{
			CreatePaymentRequest request = new CreatePaymentRequest(client, logger);
			await request.Run(data);
		}
	}
}
