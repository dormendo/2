using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.ModApi
{
	public class TinkoffConverter : PaymentConverterBase
	{
		private static char[] _charsToTrimPurpose = ". ".ToCharArray();

		public override bool CanConvert(OperationHistoryRequest.Operation o)
		{
			return string.Compare(o.contragentBankName, "АО \"ТИНЬКОФФ БАНК\"", StringComparison.OrdinalIgnoreCase) == 0 &&
				o.contragentName.StartsWith("АО \"ТИНЬКОФФ БАНК\"", StringComparison.OrdinalIgnoreCase) &&
				o.paymentPurpose.EndsWith("НДС не облагается.", StringComparison.OrdinalIgnoreCase);
		}

		public override Payment Convert(OperationHistoryRequest.Operation o, string paymentJson)
		{
			string purpose = o.paymentPurpose.Substring(0, o.paymentPurpose.Length - "НДС не облагается.".Length).TrimEnd(_charsToTrimPurpose);
			(int placeId, int? employeeId) = Helper.ParsePurposeCode(purpose);

			Payment payment = new Payment();
			payment.Id = 0;
			payment.Status = PaymentStatus.Approved;
			//payment.DocumentName = o.absId;
			//payment.DocumentNumber = o.docNumber;
			//payment.DocumentDate = DateTime.Parse(o.created, CultureInfo.InvariantCulture).Date;

			payment.ExternalId = o.id.ToString();
			payment.DataSource = "MODAPI";
			payment.Provider = "TINKFF";
			payment.OriginalAmount = o.amount;
			payment.ReceivedAmount = o.amount;
			payment.PaymentDateTime = DateTime.Parse(o.created, CultureInfo.InvariantCulture);
			payment.IsTimeSpecified = (payment.PaymentDateTime.TimeOfDay != TimeSpan.Zero);
			payment.ArrivalDateTime = DateTime.Now;
			payment.Fio = o.contragentName.Split("/")[1].Trim();
			payment.Address = "";
			payment.Purpose = o.paymentPurpose;
			payment.PlaceId = placeId;
			payment.EmployeeId = employeeId;
			payment.RawData = paymentJson;

			return payment;
		}
	}
}
