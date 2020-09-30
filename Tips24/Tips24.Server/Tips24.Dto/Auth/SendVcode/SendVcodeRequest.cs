using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto.Auth
{
	public class SendVcodeRequest : Request
	{
		public string Phone { get; set; }
	}
}
