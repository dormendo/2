using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Client.Services.Api
{
	public class ApiResponse
	{
		public ErrorResponse ErrorResponse { get; set; }

		public ApiResponseKind Kind { get; set; }
	}

	public class ApiResponse<RESPONSE> : ApiResponse where RESPONSE : class
	{
		public RESPONSE Response { get; set; }
	}
}
