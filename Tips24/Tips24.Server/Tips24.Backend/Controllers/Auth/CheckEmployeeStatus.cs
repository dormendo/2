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
	public class CheckEmployeeStatus : AuthHandler
	{
		public CheckEmployeeStatus(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(HttpRequest request, CheckStatusRequest data)
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

			try
			{
				using (SqlConnection conn = sqlServer.GetConnection())
				{
					await conn.OpenAsync();

					using (SqlCommand cmd = sqlServer.GetSpCommand("dbo.Employee_CheckEmployeeStatus", conn))
					{
						cmd.AddBinaryParam("@PermanentKey", 16, authResult.Key.ToArray());
						cmd.AddIntParam("@PlaceId", data.PlaceId);

						SqlParameter EmployeeIdParam = cmd.AddIntParam("@EmployeeId").Output();
						SqlParameter EmployeeFirstNameParam = cmd.AddNVarCharParam("@EmployeeFirstName", 50).Output();
						SqlParameter EmployeeLastNameParam = cmd.AddNVarCharParam("@EmployeeLastName", 50).Output();
						SqlParameter EmployeeIsDisabledParam = cmd.AddBitParam("@EmployeeIsDisabled").Output();
						SqlParameter PlaceGroupIdParam = cmd.AddIntParam("@PlaceGroupId").Output();
						SqlParameter PlaceGroupNameParam = cmd.AddNVarCharParam("@PlaceGroupName", 50).Output();
						SqlParameter retValParam = cmd.AddReturnValue();

						await cmd.ExecuteNonQueryAsync();

						int retVal = retValParam.GetInt32OrDefault();
						if (retVal == -1)
						{
							return this.GetAuthKeyNotFoundResponse();
						}

						CheckStatusResponse response = new CheckStatusResponse();
						response.EmployeeId = EmployeeIdParam.GetInt32OrDefault();
						response.EmployeeFirstName = EmployeeFirstNameParam.Value.ToString();
						response.EmployeeLastName = EmployeeLastNameParam.Value.ToString();
						response.EmployeeIsDisabled = EmployeeIsDisabledParam.GetBooleanOrDefault();
						response.PlaceGroupId = PlaceGroupIdParam.GetInt32OrNull();
						response.PlaceGroupName = PlaceGroupNameParam.GetStringOrNull();

						return new JsonResult(response);
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		private ErrorResponse ValidateRequest(CheckStatusRequest data)
		{
			ApiValidator.ValidatePlaceId(data.PlaceId, out ErrorResponse error);
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
				return new ErrorResponse("place_not_matched", "Пользователь привязан к другому заведению");
			}
			else
			{
				return new ErrorResponse("user_not_found", "Пользователь не найден");
			}
		}
	}
}
