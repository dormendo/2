using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.ModApi
{
	public interface IPaymentConverter
	{
		string Code { get; }

		bool NeedCreatePayment { get; }

		bool CanConvert(OperationHistoryRequest.Operation o);

		Payment Convert(OperationHistoryRequest.Operation o, string paymentJson);
	}

	public abstract class PaymentConverterBase : IPaymentConverter
	{
		public virtual string Code => this.GetType().Name;
		public virtual bool NeedCreatePayment => true;

		public abstract bool CanConvert(OperationHistoryRequest.Operation o);
		public abstract Payment Convert(OperationHistoryRequest.Operation o, string paymentJson);
	}
}
