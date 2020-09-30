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
	public class EnterSecuredSession : AuthHandler
	{
		public EnterSecuredSession(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(HttpRequest request, EnterSsRequest data)
		{
			AuthByKeyResult authResult = this.GetAuthenticationKey(request);
			if (!authResult.Result)
			{
				return new JsonErrorResult(authResult.ErrorResponse);
			}

			ErrorResponse validationError = this.ValidateRequest(data);
			if (validationError != null)
			{
				return new JsonErrorResult(validationError);
			}

			byte[] securedKey = Guid.NewGuid().ToByteArray();

			try
			{
				using (SqlConnection conn = sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = sqlServer.GetSpCommand("dbo.Employee_EnterSecuredSession", conn))
					{
						cmd.AddBinaryParam("@PermanentKey", 16, authResult.Key.ToArray());
						cmd.AddBinaryParam("@SecuredKey", 16, securedKey);
						cmd.AddCharParam("@Phone", 10, data.Phone);
						cmd.AddCharParam("@PinCode", 4, data.PinCode);

						SqlParameter retValParam = cmd.AddReturnValue();

						await cmd.ExecuteNonQueryAsync();

						int retVal = retValParam.GetInt32OrDefault();
						if (retVal < 0)
						{
							ErrorResponse errorResponse = this.GetErrorResponse(retVal);
							return new JsonErrorResult(errorResponse);
						}

						EnterSsResponse response = new EnterSsResponse();
						response.SecuredKey = AuthKey.Create(securedKey).ToString() + authResult.Key.ToString();
						return new JsonResult(response);
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		private ErrorResponse ValidateRequest(EnterSsRequest data)
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
				return this.GetAuthKeyNotFoundError();
			}
			else if (retVal == -3)
			{
				return new ErrorResponse("login_not_found", "Номер телефона или пароль введены неправильно");
			}
			else if (retVal == -4)
			{
				return new ErrorResponse("incorrect_password", "Номер телефона или пароль введены неправильно");
			}
			else
			{
				return new ErrorResponse("auth_not_match", "Пользователь успешно авторизовался под учётной записью, не соответствующей исходной");
			}
		}
	}
}
