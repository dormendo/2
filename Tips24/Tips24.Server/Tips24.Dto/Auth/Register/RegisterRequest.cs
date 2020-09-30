using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto.Auth
{
	public class RegisterRequest : Request
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Phone { get; set; }
		public string PinCode { get; set; }
		public Guid LinkParameter { get; set; }
		public int PlaceToJoinId { get; set; }
	}
}
