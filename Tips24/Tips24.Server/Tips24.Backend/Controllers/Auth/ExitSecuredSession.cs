using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tips24.Dto.Auth;

namespace Tips24.Backend.Auth
{
	public class ExitSecuredSession : AuthHandler
	{
		public ExitSecuredSession(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(HttpRequest request, ExitSsRequest data)
		{
			AuthByKeyPairResult authResult = this.GetAuthenticationKeyPair(request);
			if (!authResult.Result)
			{
				return new JsonErrorResult(authResult.ErrorResponse);
			}

			try
			{
				using (SqlConnection conn = sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = sqlServer.GetSpCommand("dbo.Employee_ExitSecuredSession", conn))
					{
						cmd.AddBinaryParam("@PermanentKey", 16, authResult.RegularKey.ToArray());
						cmd.AddBinaryParam("@SecuredKey", 16, authResult.SecuredKey.ToArray());
						SqlParameter retValParam = cmd.AddReturnValue();

						await cmd.ExecuteNonQueryAsync();

						int retVal = retValParam.GetInt32OrDefault();
						if (retVal < 0)
						{
							ErrorResponse errorResponse = this.GetErrorResponse(retVal);
							return new JsonErrorResult(errorResponse);
						}

						return new EmptyResult();
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		private ErrorResponse GetErrorResponse(int retVal)
		{
			if (retVal == -1)
			{
				return this.GetIncorrectDataError();
			}
			else if (retVal == -2)
			{
				return this.GetAuthKeyNotFoundError();
			}
			else
			{
				return this.GetAuthKeyPairNotFoundError();
			}
		}
	}
}
