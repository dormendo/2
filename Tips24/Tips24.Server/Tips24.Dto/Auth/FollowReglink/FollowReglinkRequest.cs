using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto.Auth
{
	public class FollowReglinkRequest : Request
	{
		public Guid LinkParameter { get; set; }
	}
}
