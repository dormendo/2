using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.PaymentService.Share
{
	public class MembershipData
	{
		private static TimeSpan Delta = TimeSpan.FromTicks(1);
		private static TimeSpan Day = TimeSpan.FromDays(1);
		private static decimal DayTicks = TimeSpan.TicksPerDay;

		public int EmployeeId { get; set; }

		public int GroupId { get; set; }

		public DateTime BeginDateTime { get; set; }

		public DateTime EndDateTime { get; set; }

		public decimal PartOfDay { get; private set; }

		public bool IsManager { get; set; }

		public bool IsOwner { get; set; }

		public bool IsFired { get; set; }

		public void CalculatePartOfDay()
		{
			TimeSpan interval = (EndDateTime - BeginDateTime) + Delta;
			this.PartOfDay = (interval >= Day ? 1M : ((decimal)interval.Ticks) / DayTicks);
		}
	}
}
