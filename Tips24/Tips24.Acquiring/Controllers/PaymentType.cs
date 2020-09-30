using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring.Controllers
{
	public enum PaymentType : byte
	{
		Card = 0,
		GooglePay = 1,
		ApplePay = 2
	}
}
