using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tips24.PaymentService.YksSms
{
	public class NetworkRequestData<REQUEST, RESPONSE>
	{
		public REQUEST RequestData { get; set; }

		public ResponseData<RESPONSE> ResponseData { get; set; }

		public Exception ExceptionData { get; set; }

		public bool HasError => this.ExceptionData != null || this.ResponseData != null && this.ResponseData.HasError;

		public ResponseData<RESPONSE> CreateResponseInstance()
		{
			this.ResponseData = new ResponseData<RESPONSE>();
			return this.ResponseData;
		}

		public void DeserializeResults(bool isSuccessStatusCode)
		{
			if (isSuccessStatusCode)
			{
				this.ResponseData.Response = JsonConvert.DeserializeObject<RESPONSE>(this.ResponseData.ResponseJson, Startup.JsonSettings);
			}
			else
			{
				this.ResponseData.Error = JsonConvert.DeserializeObject<ErrorResponse>(this.ResponseData.ResponseJson, Startup.JsonSettings);
			}
		}
	}
}
