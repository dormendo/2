using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Tips24.Backend
{
	public class Handler
	{
		protected SqlServer sqlServer;

		protected IHostingEnvironment he;

		protected Handler(SqlServer sqlServer, IHostingEnvironment he)
		{
			this.sqlServer = sqlServer;
			this.he = he;
		}

		protected ErrorResponse GetExceptionResponse(Exception ex, string result = "common_error")
		{
			ErrorResponse response = new ErrorResponse(result, ex.Message);

			if (this.he.IsDevelopment())
			{
				response.ExtendedInfo = ex.ToString();
			}
			return response;
		}

		protected ErrorResponse GetIncorrectDataError()
		{
			return new ErrorResponse("incorrect_data", "Некорректные данные");
		}
	}
}
