using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Tips24.Backend
{
	public class AuthEmployeeByKeyResult
	{
		public bool Result { get; set; }

		public int EmployeeId { get; set; }

		public ErrorResponse ErrorResponse { get; set; }
	}

	public class EmployeeAuthenticator
	{
		public const string KEY_HEADER = "Tips24EmployeeKey";

		private SqlServer _sql;

		public EmployeeAuthenticator(SqlServer sql)
		{
			this._sql = sql;
		}

		public async Task<AuthEmployeeByKeyResult> AuthenticateByKey(IHeaderDictionary headers)
		{
			if (!headers.TryGetValue(KEY_HEADER, out StringValues values) || values.Count == 0)
			{
				return new AuthEmployeeByKeyResult
				{
					Result = false,
					ErrorResponse = new ErrorResponse("auth_header_not_set")
				};
			}

			string value = values[0];
			if (string.IsNullOrEmpty(value))
			{
				return new AuthEmployeeByKeyResult
				{
					ErrorResponse = new ErrorResponse("auth_header_not_set")
				};
			}

			//byte[] key = ByteArrayToHex.FromHex(value);
			if (value.Length != 10)
			{
				return new AuthEmployeeByKeyResult
				{
					ErrorResponse = new ErrorResponse("auth_header_is_incorrect")
				};
			}

			using (SqlConnection conn = this._sql.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = this._sql.GetCommand("SELECT EmployeeId FROM dbo.EmployeeAuth WHERE PermanentKey = @PermanentKey", conn))
				{
					cmd.AddCharParam("@PermanentKey", 10, value);
					object result = await cmd.ExecuteScalarAsync();
					if (result == null)
					{
						return new AuthEmployeeByKeyResult
						{
							ErrorResponse = new ErrorResponse("auth_key_not_found")
						};
					}

					return new AuthEmployeeByKeyResult
					{
						Result = true,
						EmployeeId = Convert.ToInt32(result)
					};
				}
			}
		}
	}
}
