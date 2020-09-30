using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.Share
{
	public enum ReasonToReturnType : byte
	{
		ByRequest = 0,
		AmountLimitExceeded = 1,
		PlaceInactive = 2,
		NoWeightsToShare = 3,
	}
}
