using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.PaymentService.Share
{
	public class ShareData
	{
		public decimal PaymentLimit { get; set; }

		public decimal SystemCommission { get; set; }

		public int ShareSchemeHistoryId { get; set; }

		public byte PersonalShare { get; set; }

		public PlaceShareData Place { get; private set; } = new PlaceShareData();

		public ReceiverData Receiver { get; set; }

		public List<GroupShareData> Groups { get; private set; } = new List<GroupShareData>();

		public MembershipCollection Memberships { get; private set; } = new MembershipCollection();
	}
}
