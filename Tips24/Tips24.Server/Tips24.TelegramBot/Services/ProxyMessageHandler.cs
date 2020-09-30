using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public class ProxyMessageHandler : HttpMessageHandler
	{
		private string _host;
		private int _oldHostLength = "https://api.telegram.org".Length;
		private HttpClient _client;

		public ProxyMessageHandler(string host)
		{
			if (host.EndsWith("/"))
			{
				host = host.Substring(0, host.Length - 1);
			}

			this._host = host;
			this._client = new HttpClient();
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string newUri = this._host + request.RequestUri.ToString().Substring(_oldHostLength);

			HttpRequestMessage newRequest = new HttpRequestMessage(request.Method, newUri);
			newRequest.Content = request.Content;

			return await this._client.SendAsync(newRequest);
		}
	}
}
