using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Client.Services.Api
{
	public enum AuthHeaderKind : byte
	{
		None = 0,
		Regular = 1,
		Secured = 2,
		OptionalRegular = 3
	}
}
