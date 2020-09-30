using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tips24.PaymentService.ModApi
{
	public class OperationHistoryRequest : RequestBase
	{
		public class Parameters
		{
			public string category { get; set; }
			public string from { get; set; }
			public string till { get; set; }
			public int? skip { get; set; }
			public int? records { get; set; }
		}

		public class Operation
		{
			public Guid id { get; set; }
			public string companyId { get; set; }
			public string status { get; set; }
			public string category { get; set; }
			public string contragentName { get; set; }
			public string contragentInn { get; set; }
			public string contragentKpp { get; set; }
			public string contragentBankAccountNumber { get; set; }
			public string contragentBankName { get; set; }
			public string contragentBankBic { get; set; }
			public string currency { get; set; }
			public decimal amount { get; set; }
			public string bankAccountNumber { get; set; }
			public string paymentPurpose { get; set; }
			public string executed { get; set; }
			public string created { get; set; }
			public string docNumber { get; set; }
			public string absId { get; set; }
			public string kbk { get; set; }
			public string oktmo { get; set; }
			public string paymentBasis { get; set; }
			public string taxCode { get; set; }
			public string taxDocNum { get; set; }
			public string taxDocDate { get; set; }
			public string payerStatus { get; set; }
			public string uin { get; set; }
		}

		public OperationHistoryRequest(HttpClient client)
			: base(client)
		{
		}

		public async Task<List<Operation>> Run(string bankAccountId, Parameters parameters)
		{
			string content = (parameters == null ? null : JsonConvert.SerializeObject(parameters, Startup.JsonSettings));

			using (HttpResponseMessage response = await this.GetResponse("operation-history/" + bankAccountId, content))
			{
				string str = await response.Content.ReadAsStringAsync();
				response.EnsureSuccessStatusCode();
				return JsonConvert.DeserializeObject<List<Operation>>(str, Startup.JsonSettings);
			}
		}

		public static async Task<List<Operation>> Run(HttpClient client, string bankAccountId, Parameters parameters)
		{
			return await RepeatedCall.ActionAsync(async () =>
			{
				OperationHistoryRequest request = new OperationHistoryRequest(client);
				return await request.Run(bankAccountId, parameters);
			}, 5, 1000);
		}
	}
}
