using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Tips24.Dto;

namespace Tips24.Client.Services.Api
{
	public abstract class ControllerClient
	{
		protected HttpClient _httpClient;

		protected string _controllerUriPart;

		protected ClientInformation _clientInfo;

		protected ControllerClient(HttpClient httpClient, ClientInformation clientInfo, string controllerUriPart)
		{
			this._httpClient = httpClient;
			this._clientInfo = clientInfo;
			this._controllerUriPart = controllerUriPart;
		}
	}
}
