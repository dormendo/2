using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Tips24.Dto.Auth;

namespace Tips24.Backend.Auth
{
	public class CheckVerificationCode : Handler
	{
		public CheckVerificationCode(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(CheckVcodeRequest data)
		{
			ErrorResponse validationError = this.ValidateRequest(data);
			if (validationError != null)
			{
				return new JsonErrorResult(validationError);
			}

			try
			{
				using (SqlConnection conn = sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = sqlServer.GetSpCommand("dbo.Employee_CheckVerificationCode", conn))
					{
						cmd.AddUniqueIdentifierParam("@VerificationId", data.VerificationId);
						cmd.AddCharParam("@Code", 6, data.Code);
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

				return new EmptyResult();
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		private ErrorResponse ValidateRequest(CheckVcodeRequest data)
		{
			if (!ApiValidator.ValidateVerificationId(data.VerificationId, out ErrorResponse error))
			{
				return error;
			}

			ApiValidator.ValidateVerificationCode(data.Code, out error);
			return error;
		}

		private ErrorResponse GetErrorResponse(int retVal)
		{
			if (retVal == -1)
			{
				return this.GetIncorrectDataError();
			}
			else if (retVal == -2)
			{
				return new ErrorResponse("verification_not_found", "Идентификатор верификации не найден");
			}
			else if (retVal == -3)
			{
				return new ErrorResponse("verification_code_expired", "Время действия кода верификации истекло");
			}
			else
			{
				return new ErrorResponse("incorrect_verification_code", "Некорректный код верификации");
			}
		}
	}
}
