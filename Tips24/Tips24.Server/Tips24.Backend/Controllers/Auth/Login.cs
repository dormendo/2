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
	public class Login : Handler
	{
		public Login(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(LoginRequest data)
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

					using (SqlCommand cmd = sqlServer.GetSpCommand("Employee_Login", conn))
					{
						cmd.AddCharParam("@Phone", 10, data.Phone);
						cmd.AddCharParam("@PinCode", 4, data.PinCode);

						SqlParameter PermanentKeyParam = cmd.AddBinaryParam("@PermanentKey", 16).Output();
						SqlParameter retValParam = cmd.AddReturnValue();

						await cmd.ExecuteNonQueryAsync();

						int retVal = retValParam.GetInt32OrDefault();
						if (retVal < 0)
						{
							ErrorResponse errorResponse = this.GetErrorResponse(retVal);
							return new JsonErrorResult(errorResponse);
						}

						LoginResponse response = new LoginResponse();
						response.PermanentKey = PermanentKeyParam.Value.ToString();
						return new JsonResult(response);
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		private ErrorResponse ValidateRequest(LoginRequest data)
		{
			if (!ApiValidator.ValidatePhone(data.Phone, out ErrorResponse error))
			{
				return error;
			}

			ApiValidator.ValidatePinCode(data.PinCode, out error);
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
				return new ErrorResponse("login_not_found", "Номер телефона или пароль введены неправильно");
			}
			else if (retVal == -3)
			{
				return new ErrorResponse("incorrect_password", "Номер телефона или пароль введены неправильно");
			}
			else
			{
				return new ErrorResponse("user_not_found", "Номер телефона или пароль введены неправильно");
			}
		}
	}
}
