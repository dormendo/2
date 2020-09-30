using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.ModApi
{
	public class RaiffeisenConverter : PaymentConverterBase
	{
		public override bool CanConvert(OperationHistoryRequest.Operation o)
		{
			return string.Compare(o.contragentBankName, "АО \"РАЙФФАЙЗЕНБАНК\"", StringComparison.OrdinalIgnoreCase) == 0;
		}

		public override Payment Convert(OperationHistoryRequest.Operation o, string paymentJson)
		{
			string purpose = o.paymentPurpose.Trim();
			(int placeId, int? employeeId) = Helper.ParsePurposeCode(purpose);

			string[] contragentParts = o.contragentName.Split("//", StringSplitOptions.RemoveEmptyEntries);

			Payment payment = new Payment();
			payment.Id = 0;
			payment.Status = PaymentStatus.Approved;
			//payment.DocumentName = o.absId;
			//payment.DocumentNumber = o.docNumber;
			//payment.DocumentDate = DateTime.Parse(o.created, CultureInfo.InvariantCulture).Date;


			payment.ExternalId = o.id.ToString();
			payment.DataSource = "MODAPI";
			payment.Provider = "RAIFZN";
			payment.OriginalAmount = o.amount;
			payment.ReceivedAmount = o.amount;
			payment.PaymentDateTime = DateTime.Parse(o.created, CultureInfo.InvariantCulture);
			payment.IsTimeSpecified = (payment.PaymentDateTime.TimeOfDay != TimeSpan.Zero);
			payment.ArrivalDateTime = DateTime.Now;
			payment.Fio = contragentParts[0].Trim();
			payment.Address = contragentParts[1].Trim();
			payment.Purpose = o.paymentPurpose;
			payment.PlaceId = placeId;
			payment.EmployeeId = employeeId;
			payment.RawData = paymentJson;

			return payment;
		}
	}
}
