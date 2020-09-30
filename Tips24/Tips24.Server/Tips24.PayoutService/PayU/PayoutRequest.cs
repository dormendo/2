using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PayoutService.PayU
{
	public class PayoutRequest
	{
		public int Id { get; set; }

		public Employee EmployeeData { get; set; }

		public string Provider { get; set; }

		public int ProviderId { get; set; }

		public decimal PayoutAmount { get; set; }

		public decimal CommissionAmount { get; set; }

		public decimal TotalAmount => this.PayoutAmount + this.CommissionAmount;

		public int Status { get; set; }

		public DateTime CreateDateTime { get; set; }

		public DateTime? PaidDateTime { get; set; }
	}
}
