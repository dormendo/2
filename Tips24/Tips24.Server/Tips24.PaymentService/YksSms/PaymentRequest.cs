using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.YksSms
{
	public class PaymentRequest
	{
		public class PlaceData
		{
			public int Id { get; set; }

			public string Name { get; set; }
		}

		public int Id { get; set; }
		public PaymentRequestStatus Status { get; set; }
		public PlaceData Place { get; set; } = new PlaceData();
		public int? EmployeeId { get; set; }
		public DateTime CreateDateTime { get; set; }
		public string UserLogin { get; set; }
		public decimal Amount { get; set; }
		public byte ProviderId { get; set; }
		public string KassaPaymentId { get; set; }

		/// <summary>
		/// Заполняется только при успешном создании платежа
		/// </summary>
		public long? PaymentId { get; set; }

		public void FillFromReader(SqlDataReader dr)
		{
			this.Id = dr.GetInt32("RequestId");
			this.Status = (PaymentRequestStatus)dr.GetByte("Status");
			this.Place.Id = dr.GetInt32("PlaceId");
			this.EmployeeId = dr.GetInt32OrNull("EmployeeId");
			this.CreateDateTime = dr.GetDateTime("CreateDateTime");
			this.UserLogin = dr.GetString("UserLogin");
			this.Amount = dr.GetDecimal("Amount");
			this.ProviderId = dr.GetByte("ProviderId");
			this.Place.Name = dr.GetString("PlaceDisplayName");
			this.KassaPaymentId = dr.GetStringOrNull("KassaPaymentId");
		}
	}
}
