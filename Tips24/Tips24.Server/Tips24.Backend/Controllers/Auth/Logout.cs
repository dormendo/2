using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tips24.Dto;

namespace Tips24.Backend.Auth
{
	public class Logout : AuthHandler
	{
		public Logout(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(HttpRequest request, Request data)
		{
			AuthByKeyResult authResult = this.GetAuthenticationKey(request);
			if (!authResult.Result)
			{
				return new JsonErrorResult(authResult.ErrorResponse);
			}

			try
			{
				using (SqlConnection conn = sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = sqlServer.GetSpCommand("dbo.Employee_Logout", conn))
					{
						cmd.AddBinaryParam("@PermanentKey", 16, authResult.Key.ToArray());
						SqlParameter retValParam = cmd.AddReturnValue();
						await cmd.ExecuteNonQueryAsync();

						int retVal = retValParam.GetInt32OrDefault();
						if (retVal < 0)
						{
							ErrorResponse errorResponse = this.GetErrorResponse(retVal);
							return new JsonErrorResult(errorResponse);
						}
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}

			return new EmptyResult();
		}

		private ErrorResponse GetErrorResponse(int retVal)
		{
			if (retVal == -1)
			{
				return this.GetIncorrectDataError();
			}
			else
			{
				return this.GetAuthKeyNotFoundError();
			}
		}
	}
}
