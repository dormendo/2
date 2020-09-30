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
	public class FollowRegistrationLink : AuthHandler
	{
		public FollowRegistrationLink(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(HttpRequest request, FollowReglinkRequest data)
		{
			AuthByKeyResult authResult = this.HasAuthenticationKey(request);
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

					using (SqlCommand cmd = sqlServer.GetSpCommand("Employee_FollowRegistrationLink", conn))
					{
						cmd.AddUniqueIdentifierParam("@LinkParameter", data.LinkParameter);
						cmd.AddBinaryParam("@PermanentKey", 16, authResult.Key.ToArray());

						SqlParameter LinkPlaceIdParam = cmd.AddIntParam("@LinkPlaceId").Output();
						SqlParameter LinkPlaceNameParam = cmd.AddNVarCharParam("@LinkPlaceName", 100).Output();
						SqlParameter LinkPlaceAddressParam = cmd.AddNVarCharParam("@LinkPlaceAddress", 100).Output();
						SqlParameter LinkPlaceCityParam = cmd.AddNVarCharParam("@LinkPlaceCity", 40).Output();
						SqlParameter EmployeeIdParam = cmd.AddIntParam("@EmployeeId").Output();
						SqlParameter EmployeePlaceIdParam = cmd.AddIntParam("@EmployeePlaceId").Output();
						SqlParameter EmployeeIsDisabledParam = cmd.AddBitParam("@EmployeeIsDisabled").Output();
						SqlParameter retValParam = cmd.AddReturnValue();

						await cmd.ExecuteNonQueryAsync();
						int retVal = retValParam.GetInt32OrDefault();
						if (retVal < 0)
						{
							ErrorResponse errorResponse = this.GetErrorResponse(retVal);
							return new JsonErrorResult(errorResponse);
						}

						FollowReglinkResponse response = new FollowReglinkResponse();
						response.LinkPlaceId = LinkPlaceIdParam.GetInt32OrDefault();
						response.LinkPlaceName = LinkPlaceNameParam.Value.ToString();
						response.LinkPlaceAddress = LinkPlaceAddressParam.Value.ToString();
						response.LinkPlaceCity = LinkPlaceCityParam.Value.ToString();
						response.EmployeeId = EmployeeIdParam.GetInt32OrNull();
						response.EmployeePlaceId = EmployeePlaceIdParam.GetInt32OrNull();
						response.EmployeeIsDisabled = EmployeeIsDisabledParam.GetBooleanOrNull();
						return new JsonResult(response);
					}
				}
			}
			catch (Exception ex)
			{
				return new JsonErrorResult(this.GetExceptionResponse(ex));
			}
		}

		private ErrorResponse ValidateRequest(FollowReglinkRequest data)
		{
			ApiValidator.ValidateLinkParameter(data.LinkParameter, out ErrorResponse error);
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
				return new ErrorResponse("link_expired", "Время действия ссылки истекло");
			}
			else
			{
				return this.GetAuthKeyNotFoundError();
			}
		}
	}
}
