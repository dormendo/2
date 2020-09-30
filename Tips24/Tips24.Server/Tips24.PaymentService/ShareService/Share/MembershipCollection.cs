using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.PaymentService.Share
{
	public class MembershipCollection
	{
		private Dictionary<int, List<MembershipData>> _dict = new Dictionary<int, List<MembershipData>>();
		
		public void Add(MembershipData md)
		{
			if (!this._dict.TryGetValue(md.EmployeeId, out List<MembershipData> list))
			{
				list = new List<MembershipData>();
				this._dict.Add(md.EmployeeId, list);
			}

			list.Add(md);
		}

		public IEnumerable<MembershipData> EnumeratePartsOfDay()
		{
			foreach (List<MembershipData> list in this._dict.Values)
			{
				foreach (MembershipData md in list)
				{
					yield return md;
				}
			}
		}

		public IEnumerable<int> EnumerateMembers()
		{
			return this._dict.Keys;
		}

		public int GetCount()
		{
			int count = 0;
			foreach (List<MembershipData> list in this._dict.Values)
			{
				count += list.Count;
			}

			return count;
		}

		public int GetMemberCount()
		{
			return this._dict.Count;
		}
	}
}
