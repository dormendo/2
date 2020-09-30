using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring.Controllers.PaymentPage
{
	public class Response
	{
		[JsonProperty("error_code")]
		public int ErrorCode { get; set; }

		[JsonProperty("error")]
		public string ErrorMessage { get; set; }

		[JsonIgnore]
		public bool HasError => ErrorCode == 0;

		public void SetError(int errorCode, string errorMessage)
		{
			this.ErrorCode = errorCode;
			this.ErrorMessage = errorMessage;
		}

		public string GetErrorMessage()
		{
			if (ErrorCode == 0)
			{
				return null;
			}

			return "Код ошибки:" + ErrorCode.ToString() + ", ошибка: " + (ErrorMessage == null ? "<null>" : ErrorMessage);
		}
	}
}
