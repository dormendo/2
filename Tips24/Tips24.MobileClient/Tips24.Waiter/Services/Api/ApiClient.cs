using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Tips24.Dto;

namespace Tips24.Client.Services.Api
{
	public class ApiClient
	{
		private string _baseUri;

		private HttpClient _httpClient;

		private ClientInformation _clientInfo;

		public AuthControllerClient Auth { get; private set; }

		public ApiClient(string baseUri, string osVersion, string appVersion)
		{
			this._baseUri = baseUri;
			this._clientInfo = new ClientInformation { Os = osVersion, Version = appVersion };
		}

		public void Start()
		{
			this._httpClient = new HttpClient()
			{
				BaseAddress = new Uri(this._baseUri),
				MaxResponseContentBufferSize = 64 * 1024,
				Timeout = TimeSpan.FromSeconds(10)
			};

			this.Auth = new AuthControllerClient(this._httpClient, this._clientInfo);
		}

		public void Stop()
		{
			this.Auth = null;

			if (this._httpClient != null)
			{
				this._httpClient.Dispose();
				this._httpClient = null;
			}
		}
	}
}
