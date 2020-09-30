using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto.Auth
{
	public class CheckVcodeRequest : Request
	{
		public Guid VerificationId { get; set; }

		public string Code { get; set; }
	}
}
