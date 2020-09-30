using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.PaymentService.Share
{
	public class GroupShareData
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public decimal Weight { get; set; }

		public decimal GroupWeight { get; private set; }

		public decimal GroupTotalDays { get; set; }

		public MembershipCollection Memberships { get; private set; } = new MembershipCollection();

		public void AddMembership(MembershipData md)
		{
			this.Memberships.Add(md);
			md.CalculatePartOfDay();
			this.GroupTotalDays += md.PartOfDay;
		}

		public void CalculateGroupWeight(bool isTimeSpecified)
		{
			this.GroupWeight = (isTimeSpecified ? this.Weight * this.Memberships.GetMemberCount() : this.Weight * this.GroupTotalDays);
		}
	}
}
