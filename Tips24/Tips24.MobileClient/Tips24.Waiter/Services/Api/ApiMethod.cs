using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tips24.Client.Services.Api
{
	public class ApiMethod<REQUEST, RESPONSE>
		where REQUEST : Dto.Request
		where RESPONSE : class
	{
		private static JsonSerializerSettings _jsonSettings;

		static ApiMethod()
		{
			_jsonSettings = new JsonSerializerSettings();
			_jsonSettings.FloatParseHandling = FloatParseHandling.Decimal;
			_jsonSettings.NullValueHandling = NullValueHandling.Ignore;
			_jsonSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
            _jsonSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
		}

		protected HttpClient _httpClient;
		protected string _uri;
		protected AuthHeaderKind _authHeaderKind;

		public ApiMethod(HttpClient httpClient, string controllerUriPart, string methodUriPart, AuthHeaderKind authHeaderKind = AuthHeaderKind.None)
		{
			this._httpClient = httpClient;
			this._uri = controllerUriPart + "/" + methodUriPart;
			this._authHeaderKind = authHeaderKind;
		}

		public async Task<ApiResponse<RESPONSE>> Call(REQUEST requestData = null)
		{
			string authKey = null;
			if (this._authHeaderKind == AuthHeaderKind.Regular || this._authHeaderKind == AuthHeaderKind.OptionalRegular)
			{
				authKey = ServiceLocator.AuthorizationService.GetRegularAuthKey();
			}
			else if (this._authHeaderKind == AuthHeaderKind.Secured)
			{
				authKey = ServiceLocator.AuthorizationService.GetSecuredAuthKey();
			}

			if ((this._authHeaderKind == AuthHeaderKind.Regular || this._authHeaderKind == AuthHeaderKind.Secured) && authKey == null)
			{
				return new ApiResponse<RESPONSE> { Kind = ApiResponseKind.NotAuthorized };
			}

			string serializedRequest = (requestData != null ? JsonConvert.SerializeObject(requestData, _jsonSettings) : null);
			byte[] requestByteArray = (!string.IsNullOrEmpty(serializedRequest) ? Encoding.UTF8.GetBytes(serializedRequest) : null);

			using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, this._uri))
			{
				if (requestByteArray != null)
				{
					HttpContent content = new ByteArrayContent(requestByteArray);
					content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
					requestMessage.Content = content;
				}

				if (this._authHeaderKind == AuthHeaderKind.Regular || this._authHeaderKind == AuthHeaderKind.Secured ||
					this._authHeaderKind == AuthHeaderKind.OptionalRegular && authKey != null)
				{
					requestMessage.Headers.Add("Tips24EmployeeKey", authKey);
				}

				using (HttpResponseMessage responseMessage = await this._httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead))
				{
					ApiResponse<RESPONSE> responseData;
					string serializedResponse = await responseMessage.Content.ReadAsStringAsync();
					if (responseMessage.IsSuccessStatusCode)
					{
						RESPONSE response = JsonConvert.DeserializeObject<RESPONSE>(serializedResponse, _jsonSettings);
						responseData = new ApiResponse<RESPONSE> { Kind = ApiResponseKind.Success, Response = response };
					}
					else
					{
						ErrorResponse response = JsonConvert.DeserializeObject<ErrorResponse>(serializedResponse, _jsonSettings);
						responseData = new ApiResponse<RESPONSE> { Kind = ApiResponseKind.Error, ErrorResponse = response };
					}

					return responseData;
				}
			}
		}
	}
}
