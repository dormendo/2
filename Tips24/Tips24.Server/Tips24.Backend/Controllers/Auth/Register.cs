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
	public class Register : Handler
	{
		public Register(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(RegisterRequest data)
		{
			ErrorResponse validationError = this.ValidateRequest(data);
			if (validationError != null)
			{
				return new JsonErrorResult(validationError);
			}

			try
			{
				byte[] permanentKey = Guid.NewGuid().ToByteArray();

				using (SqlConnection conn = sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = sqlServer.GetSpCommand("Employee_Register", conn))
					{
						cmd.AddNVarCharParam("@FirstName", 50, data.FirstName);
						cmd.AddNVarCharParam("@LastName", 50, data.LastName);
						cmd.AddCharParam("@Phone", 10, data.Phone);
						cmd.AddCharParam("@PinCode", 4, data.PinCode);
						cmd.AddUniqueIdentifierParam("@LinkParameter", data.LinkParameter);
						cmd.AddIntParam("@PlaceId", data.PlaceToJoinId);
						cmd.AddBinaryParam("@PermanentKey", 16, permanentKey);
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

				RegisterResponse response = new RegisterResponse();
				response.PermanentKey = AuthKey.Create(permanentKey).ToString();
				return new JsonResult(response);
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		private ErrorResponse ValidateRequest(RegisterRequest data)
		{
			if (!ApiValidator.ValidateFirstName(data.FirstName, out ErrorResponse error))
			{
				return error;
			}

			if (!ApiValidator.ValidateLastName(data.LastName, out error))
			{
				return error;
			}

			if (!ApiValidator.ValidatePhone(data.Phone, out error))
			{
				return error;
			}

			if (!ApiValidator.ValidatePinCode(data.PinCode, out error))
			{
				return error;
			}

			if (!ApiValidator.ValidateLinkParameter(data.LinkParameter, out error))
			{
				return error;
			}

			ApiValidator.ValidatePlaceId(data.PlaceToJoinId, out error);
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
				return new ErrorResponse("link_not_found", "Ссылка регистрации не найдена");
			}
			else if (retVal == -3)
			{
				return new ErrorResponse("link_place_not_matched", "Ссылка регистрации не найдена");
			}
			else if (retVal == -4)
			{
				return new ErrorResponse("place_not_found", "Заведение не найдено");
			}
			else
			{
				return new ErrorResponse("phone_already_registered", "Номер телефона уже зарегистрирован");
			}
		}
	}
}
