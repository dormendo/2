using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Tips24.Backend
{
	public class AuthHandler : Handler
	{
		public class AuthByKeyResult
		{
			public bool Result { get; set; }

			public AuthKey Key { get; set; }

			public ErrorResponse ErrorResponse { get; set; }
		}

		public class AuthByKeyPairResult
		{
			public bool Result { get; set; }

			public AuthKey RegularKey { get; set; }

			public AuthKey SecuredKey { get; set; }

			public ErrorResponse ErrorResponse { get; set; }
		}


		public const string KEY_HEADER = "Tips24EmployeeKey";

		protected AuthHandler(SqlServer sqlServer, IHostingEnvironment he)
			: base(sqlServer, he)
		{
		}

		#region Один ключ

		protected AuthByKeyResult GetAuthenticationKey(HttpRequest request)
		{
			if (!request.Headers.TryGetValue(KEY_HEADER, out StringValues values) || values.Count == 0)
			{
				return new AuthByKeyResult
				{
					Result = false,
					ErrorResponse = new ErrorResponse("auth_header_not_set")
				};
			}

			string value = values[0];
			if (string.IsNullOrEmpty(value))
			{
				return new AuthByKeyResult
				{
					ErrorResponse = new ErrorResponse("auth_header_not_set")
				};
			}

			if (value.Length != 32)
			{
				return new AuthByKeyResult
				{
					ErrorResponse = new ErrorResponse("auth_header_is_incorrect")
				};
			}

			return new AuthByKeyResult
			{
				Result = true,
				Key = AuthKey.Create(value)
			};
		}

		protected AuthByKeyResult HasAuthenticationKey(HttpRequest request)
		{
			if (!request.Headers.TryGetValue(KEY_HEADER, out StringValues values) || values.Count == 0)
			{
				return new AuthByKeyResult
				{
					Result = true
				};
			}

			string value = values[0];
			if (string.IsNullOrEmpty(value))
			{
				return new AuthByKeyResult
				{
					Result = true
				};
			}

			if (value.Length != 32)
			{
				return new AuthByKeyResult
				{
					ErrorResponse = new ErrorResponse("auth_header_is_incorrect")
				};
			}

			return new AuthByKeyResult
			{
				Result = true,
				Key = AuthKey.Create(value)
			};
		}

		#endregion

		#region Два ключа

		protected AuthByKeyPairResult GetAuthenticationKeyPair(HttpRequest request)
		{
			if (!request.Headers.TryGetValue(KEY_HEADER, out StringValues values) || values.Count == 0)
			{
				return new AuthByKeyPairResult
				{
					Result = false,
					ErrorResponse = new ErrorResponse("auth_header_not_set")
				};
			}

			string value = values[0];
			if (string.IsNullOrEmpty(value))
			{
				return new AuthByKeyPairResult
				{
					ErrorResponse = new ErrorResponse("auth_header_not_set")
				};
			}

			if (value.Length != 20)
			{
				return new AuthByKeyPairResult
				{
					ErrorResponse = new ErrorResponse("auth_header_is_incorrect")
				};
			}

			return new AuthByKeyPairResult
			{
				Result = true,
				RegularKey = AuthKey.Create(value.Substring(10, 10)),
				SecuredKey = AuthKey.Create(value.Substring(0, 10))
			};
		}

		#endregion

		protected ErrorResponse GetAuthKeyNotFoundError()
		{
			return new ErrorResponse("auth_key_not_found", "Постоянный ключ авторизации не найден");
		}

		protected ErrorResponse GetAuthKeyPairNotFoundError()
		{
			return new ErrorResponse("auth_key_pair_not_found", "Пара ключей авторизации не найдена");
		}

		protected ErrorResponse GetSecuredSessionExpiredError()
		{
			return new ErrorResponse("secured_session_expired", "Срок действия защищённой сессии истёк");
		}

		protected JsonErrorResult GetAuthKeyNotFoundResponse()
		{
			return new JsonErrorResult(this.GetAuthKeyNotFoundError());
		}
	}
}
