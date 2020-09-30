using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tips24.PaymentService.YksSms
{
	public abstract class YksSmsRequestBase
	{
		protected HttpClient _client;
		protected ILogger _logger;

		protected YksSmsRequestBase(HttpClient client, ILogger logger)
		{
			this._client = client;
			this._logger = logger;
		}

		public async Task<HttpResponseMessage> GetResponse(string uri, HttpMethod method, string data)
		{
			using (HttpRequestMessage request = new HttpRequestMessage(method, uri))
			{
				request.Version = new Version("1.1");

				this.AddRequestHeaders(request);

				if (!string.IsNullOrEmpty(data))
				{
					HttpContent content = new ByteArrayContent(Encoding.UTF8.GetBytes(data));
					content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
					this.AddContentHeaders(request);
					request.Content = content;
				}

				HttpResponseMessage response = await this._client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
				return response;
			}
		}

		protected virtual void AddRequestHeaders(HttpRequestMessage request)
		{
		}

		protected virtual void AddContentHeaders(HttpRequestMessage request)
		{
		}
	}
}
