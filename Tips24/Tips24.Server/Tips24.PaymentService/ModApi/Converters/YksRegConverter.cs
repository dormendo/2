using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.ModApi
{
	public class YksRegConverter : PaymentConverterBase
	{
		public override bool NeedCreatePayment => false;

		public override bool CanConvert(OperationHistoryRequest.Operation o)
		{
			return string.Compare(o.contragentBankName, "ООО НКО \"ЯНДЕКС.ДЕНЬГИ\"", StringComparison.OrdinalIgnoreCase) == 0 &&
				string.Compare(o.contragentName, "ООО НКО \"Яндекс.Деньги\"", StringComparison.OrdinalIgnoreCase) == 0 &&
				o.paymentPurpose.StartsWith("//Реестр//", StringComparison.OrdinalIgnoreCase);
		}

		public override Payment Convert(OperationHistoryRequest.Operation o, string paymentJson)
		{
			throw new NotImplementedException();
		}
	}
}
