using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto.Auth
{
	public class EnterSsRequest : Request
	{
		public string Phone { get; set; }

		public string PinCode { get; set; }
	}
}
