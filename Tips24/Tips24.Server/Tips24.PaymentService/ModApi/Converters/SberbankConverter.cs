using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.ModApi
{
	public class SberbankConverter : PaymentConverterBase
	{
		public override bool CanConvert(OperationHistoryRequest.Operation o)
		{
			return o.contragentBankName.EndsWith("ПАО СБЕРБАНК", StringComparison.OrdinalIgnoreCase) &&
				o.contragentName.StartsWith("ПАО СБЕРБАНК//", StringComparison.OrdinalIgnoreCase);
		}

		public override Payment Convert(OperationHistoryRequest.Operation o, string paymentJson)
		{
			string[] purposeParts = o.paymentPurpose.Split(';', StringSplitOptions.RemoveEmptyEntries);
			string purpose = purposeParts.Where(p => p.StartsWith("НАЗНАЧЕНИЕ:", StringComparison.OrdinalIgnoreCase)).First().Substring("НАЗНАЧЕНИЕ:".Length);
			string dateStr = purposeParts.Where(p => p.StartsWith("ЗА ", StringComparison.OrdinalIgnoreCase)).First().Substring("ЗА ".Length);

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
			payment.Provider = "SBERBK";
			payment.OriginalAmount = o.amount;
			payment.ReceivedAmount = o.amount;
			payment.PaymentDateTime = DateTime.ParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
			payment.IsTimeSpecified = false;
			payment.ArrivalDateTime = DateTime.Now;
			payment.Fio = contragentParts[1].Trim();
			payment.Address = contragentParts[3].Trim();
			payment.Purpose = o.paymentPurpose;
			payment.PlaceId = placeId;
			payment.EmployeeId = employeeId;
			payment.RawData = paymentJson;

			return payment;
		}
	}
}
