using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto.Auth
{
	public class JoinPlaceRequest : Request
	{
		public Guid LinkParameter { get; set; }

		public int PlaceToJoinId { get; set; }
	}
}
