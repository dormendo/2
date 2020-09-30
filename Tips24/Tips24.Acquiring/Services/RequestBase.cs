using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tips24.Acquiring
{
	public abstract class RequestBase
	{
		protected HttpClient _client;

		protected RequestBase(HttpClient client)
		{
			this._client = client;
		}

		public async Task<HttpResponseMessage> GetResponse(string uri, string data)
		{
			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri))
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
