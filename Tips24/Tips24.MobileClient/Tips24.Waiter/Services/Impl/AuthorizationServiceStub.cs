using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Tips24.Client.Services.Impl
{
    public class AuthorizationServiceStub : IAuthorizationService
    {
		private const string AUTH_KEY = "AUTH_KEY";

		private const string SECURED_AUTH_KEY = "SECURED_AUTH_KEY";

		private string _authKey;

		private string _securedAuthKey;

		public string GetRegularAuthKey()
		{
			return GetAuthKey(AUTH_KEY) ?? this._authKey;
		}

		public void SetRegularAuthKey(string authKey)
		{
			SetAuthKey(AUTH_KEY, authKey);
			this._authKey = authKey;
		}

		public string GetSecuredAuthKey()
		{
			return GetAuthKey(SECURED_AUTH_KEY) ?? this._securedAuthKey;
		}

		public void SetSecuredAuthKey(string authKey)
		{
			SetAuthKey(SECURED_AUTH_KEY, authKey);
			this._securedAuthKey = authKey;
		}

		private string GetAuthKey(string key)
		{
			return Task.Run(async () => await GetAuthKeyAsync(key)).Result;
		}

		private async Task<string> GetAuthKeyAsync(string key)
		{
			try
			{
				return await SecureStorage.GetAsync(key);
			}
			catch
			{
				return null;
			}
		}

		private static void SetAuthKey(string key, string value)
		{
			Task.Run(async () => await SetAuthKeyAsync(key, value));
		}

		private static async Task SetAuthKeyAsync(string key, string value)
		{
			try
			{
				await SecureStorage.SetAsync(key, value);
			}
			catch
			{
			}
		}
	}
}
