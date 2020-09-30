using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.Share
{
	public class AmountData
	{
		public decimal PayoutAmount { get; set; }

		public Dictionary<int, PersonalAmountData> PersonalAmounts { get; private set; } = new Dictionary<int, PersonalAmountData>();

		public void AddPersonalAmount(int employeeId, decimal amount)
		{
			if (!this.PersonalAmounts.TryGetValue(employeeId, out PersonalAmountData pad))
			{
				pad = new PersonalAmountData { EmployeeId = employeeId };
				this.PersonalAmounts.Add(employeeId, pad);
			}

			pad.Amount += amount;
		}
	}
}
