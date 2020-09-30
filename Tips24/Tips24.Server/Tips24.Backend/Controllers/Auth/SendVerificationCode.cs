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
	public class SendVerificationCode : Handler
	{
		public SendVerificationCode(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(SendVcodeRequest data)
		{
			ErrorResponse validationError = this.ValidateRequest(data);
			if (validationError != null)
			{
				return new JsonErrorResult(validationError);
			}

			try
			{
				Guid verificationId = Guid.NewGuid();
				string code = "111111";

				using (SqlConnection conn = sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = sqlServer.GetSpCommand("dbo.Employee_RegisterVerificationCode", conn))
					{
						cmd.AddUniqueIdentifierParam("@VerificationId", verificationId);
						cmd.AddCharParam("@Code", 6, code);
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

				// SEND SMS

				SendVcodeResponse response = new SendVcodeResponse();
				response.VerificationId = verificationId;
				return new JsonResult(response);
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		private ErrorResponse ValidateRequest(SendVcodeRequest data)
		{
			ApiValidator.ValidatePhone(data.Phone, out ErrorResponse error);
			return error;
		}

		private ErrorResponse GetErrorResponse(int retVal)
		{
			return this.GetIncorrectDataError();
		}
	}
}
