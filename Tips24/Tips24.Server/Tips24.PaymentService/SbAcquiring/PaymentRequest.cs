using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.SbAcquiring
{
	public enum RequestStatus : byte
	{
		Created = 0,
		OrderIdSaved = 1,
		Failed = 2,
		Succeeded = 3,
		Complete = 4,
		FailedToComplete = 5
	}

	public enum RequestType : byte
	{
		Card = 0,
		GooglePay = 1,
		ApplePay = 2
	}

	public class PaymentRequest
	{
		public int RequestId { get; set; }
		public RequestType Type { get; set; }
		public RequestStatus Status { get; set; }
		public DateTime CreateDateTime { get; set; }
		public int PlaceId { get; set; }
		public int? EmployeeId { get; set; }
		public decimal Amount { get; set; }
		public string OrderId { get; set; }
		public string IpAddress { get; set; }
		public long? PaymentId { get; set; }
		public string PaymentMethod { get; set; }

		public string PaymentProvider
		{
			get
			{
				switch (this.Type)
				{
					case RequestType.GooglePay:
						return "SBBAGP";
					case RequestType.ApplePay:
						return "SBBAAP";
					default:
						return "SBBACD";
				}
			}
		}

		public void FillFromReader(SqlDataReader dr)
		{
			this.RequestId = dr.GetInt32("RequestId");
			this.Type = (RequestType)dr.GetByte("Type");
			this.Status = (RequestStatus)dr.GetByte("Status");
			this.CreateDateTime = dr.GetDateTime("CreateDateTime");
			this.PlaceId = dr.GetInt32("PlaceId");
			this.EmployeeId = dr.GetInt32OrNull("EmployeeId");
			this.Amount = dr.GetDecimal("Amount");
			this.OrderId = dr.GetStringOrNull("OrderId");
			this.IpAddress = dr.GetStringOrNull("IpAddress");
			this.PaymentId = dr.GetInt64OrNull("PaymentId");
			this.PaymentMethod = dr.GetStringOrNull("PaymentMethod");
		}
	}
}
