using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24
{
	public class ErrorResponse
	{
		public string Code { get; set; }

		public string Message { get; set; }

		public string ExtendedInfo { get; set; }

        /// <summary>
        /// Используется при десериализации\.
        /// </summary>
        private ErrorResponse()
        {
        }

		public ErrorResponse(string code)
		{
			this.Code = code;
		}

		public ErrorResponse(string code, string message)
		{
			this.Code = code;
			this.Message = message;
		}
	}
}
