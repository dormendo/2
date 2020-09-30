using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.ModApi
{
	public class SbAcqConverter : PaymentConverterBase
	{
		public override bool NeedCreatePayment => false;

		public override bool CanConvert(OperationHistoryRequest.Operation o)
		{
			return o.contragentBankName.EndsWith("ПАО СБЕРБАНК", StringComparison.OrdinalIgnoreCase) &&
				o.paymentPurpose.StartsWith("По договору эквайринга", StringComparison.OrdinalIgnoreCase);
		}

		public override Payment Convert(OperationHistoryRequest.Operation o, string paymentJson)
		{
			throw new NotImplementedException();
		}
	}
}
