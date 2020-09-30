using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.PaymentService.Share
{
	public class Payment
	{
		public long Id { get; set; }

		public PaymentStatus Status { get; set; }

		public ReasonToReturnType? ReasonToReturn { get; set; }

		public string DocumentName { get; set; }

		public int? DocumentId { get; set; }

		public string DocumentNumber { get; set; }

		public DateTime? DocumentDate { get; set; }

		public string ExternalId { get; set; }

		public string DataSource { get; set; }

		public string Provider { get; set; }

		public decimal OriginalAmount { get; set; }

		public decimal ReceivedAmount { get; set; }

		public decimal BankCommissionAmount => this.OriginalAmount - this.ReceivedAmount;

		public decimal AgentCommissionAmount => this.OriginalAmount - this.PayoutAmount;

		public decimal IncomeAmount => this.ReceivedAmount - this.PayoutAmount;

		public decimal PayoutAmount { get; set; }

		public DateTime PaymentDateTime { get; set; }

		public bool IsTimeSpecified { get; set; }

		public DateTime ArrivalDateTime { get; set; }

		public string Fio { get; set; }

		public string Address { get; set; }

		public string Purpose { get; set; }

		public int PlaceId { get; set; }

		public int? EmployeeId { get; set; }

		public int ShareSchemeHistoryId { get; set; }

		public string RawData { get; set; }

		public List<PersonalAmount> PersonalAmounts { get; private set; } = new List<PersonalAmount>();
	}
}
