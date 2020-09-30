using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto
{
	public class Request
	{
		[JsonProperty(PropertyName = "__sv", Required = Required.Always)]
		public ClientInformation ClientInfo { get; set; }
	}

	public class ClientInformation
	{
		[JsonProperty(PropertyName = "os", Required = Required.Always)]
		public string Os { get; set; }

		[JsonProperty(PropertyName = "v", Required = Required.Always)]
		public string Version { get; set; }
	}
}
