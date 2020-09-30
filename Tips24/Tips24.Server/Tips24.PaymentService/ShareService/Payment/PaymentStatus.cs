using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.PaymentService.Share
{
	public enum PaymentStatus : byte
	{
		Accounted = 0,
		Approved = 1,
		Returned = 2,
		ToReturn = 3
	}
}
