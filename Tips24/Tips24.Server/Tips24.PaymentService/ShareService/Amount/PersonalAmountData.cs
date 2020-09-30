using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.Share
{
	public class PersonalAmountData
	{
		public int EmployeeId { get; set; }

		public decimal Amount { get; set; }

		public void NormalizeAmount()
		{
			this.Amount = decimal.Floor(this.Amount * 100M + 0.5M) / 100M;
		}
	}
}
