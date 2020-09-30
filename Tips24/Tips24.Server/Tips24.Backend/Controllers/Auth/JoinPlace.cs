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
	public class JoinPlace : AuthHandler
	{
		public JoinPlace(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		public async Task<IActionResult> Handle(HttpRequest request, JoinPlaceRequest data)
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

					using (SqlCommand cmd = sqlServer.GetSpCommand("dbo.Employee_JoinPlace", conn))
					{
						cmd.AddBinaryParam("@PermanentKey", 16, authResult.Key.ToArray());
						cmd.AddUniqueIdentifierParam("@LinkParameter", data.LinkParameter);
						cmd.AddIntParam("@PlaceId", data.PlaceToJoinId);
						SqlParameter retValParam = cmd.AddReturnValue();

						await cmd.ExecuteNonQueryAsync();
						int retVal = retValParam.GetInt32OrDefault();

						if (retVal == -1)
						{
							return this.GetAuthKeyNotFoundResponse();
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

		private ErrorResponse ValidateRequest(JoinPlaceRequest data)
		{
			if (!ApiValidator.ValidateLinkParameter(data.LinkParameter, out ErrorResponse error))
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
				return this.GetAuthKeyNotFoundError();
			}
		}
	}
}
