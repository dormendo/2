using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tips24.PaymentService.YksSms
{
	public class PaymentDoc
	{
		public class AmountData
		{
			[JsonIgnore]
			public decimal Amount { get; set; }

			[JsonProperty("value")]
			public string AmountStr
			{
				get
				{
					return this.Amount.ToString("F2", CultureInfo.InvariantCulture);
				}
				set
				{
					this.Amount = decimal.Parse(value, CultureInfo.InvariantCulture);
				}
			}

			[JsonProperty("currency")]
			public string Currency { get; set; }
		}

		public enum MethodType
		{
			[EnumMember(Value = "sberbank")]
			Sberbank
		}

		public class MethodData
		{
			[JsonProperty("type")]
			[JsonConverter(typeof(StringEnumConverter))]
			public MethodType Type { get; set; }

			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("saved")]
			public bool IsSaved { get; set; }

			[JsonProperty("title")]
			public string Title { get; set; }

			[JsonProperty("phone")]
			public string Phone { get; set; }
		}

		public class ConfirmationData
		{
			[JsonProperty("type")]
			public string Type { get; set; }
		}

		public class MetadataData
		{
			[JsonProperty("ir_id")]
			public int RequestId { get; set; }
		}

		public enum StatusType
		{
			[EnumMember(Value = "pending")]
			Pending = 0,
			[EnumMember(Value = "waiting_for_capture")]
			WaitingForCapture = 1,
			[EnumMember(Value = "succeeded")]
			Succeeded = 2,
			[EnumMember(Value = "canceled")]
			Canceled = 3
		}

		public enum CancellationParty
		{
			[EnumMember(Value = "yandex_checkout")]
			YandexCheckout = 0,
			[EnumMember(Value = "payment_network")]
			PaymentNetwork = 1,
			[EnumMember(Value = "merchant")]
			Merchant = 2
		}

		public class CancellationDetailsData
		{
			[JsonProperty("party")]
			[JsonConverter(typeof(StringEnumConverter))]
			public CancellationParty Party { get; set; }

			[JsonProperty("reason")]
			public string Reason { get; set; }
		}

		public class AuthorizationDetailsData
		{
			[JsonProperty("rrn")]
			public string Rrn { get; set; }

			[JsonProperty("auth_code")]
			public string AuthCode { get; set; }
		}

		public class RecipientData
		{
			[JsonProperty("account_id")]
			public string AccountId { get; set; }

			[JsonProperty("gateway_id")]
			public string GatewayId { get; set; }
		}

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("status")]
		[JsonConverter(typeof(StringEnumConverter))]
		public StatusType Status { get; set; }

		[JsonProperty("amount")]
		public AmountData Amount { get; set; } = new AmountData();

		[JsonProperty("refunded_amount")]
		public AmountData RefundedAmount { get; set; } = new AmountData();

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("recipient")]
		public RecipientData Recipient { get; set; }

		[JsonProperty("payment_method")]
		public MethodData PaymentMethodData { get; set;  } = new MethodData();

		[JsonProperty("captured_at")]
		public string CapturedAt { get; set; }

		[JsonProperty("created_at")]
		public string CreatedAt { get; set; }

		[JsonProperty("expires_at")]
		public string ExpiresAt { get; set; }

		[JsonProperty("confirmation")]
		public ConfirmationData Confirmation { get; set; } = new ConfirmationData();

		[JsonProperty("test")]
		public bool Test { get; set; }

		[JsonProperty("paid")]
		public bool Paid { get; set; }

		[JsonProperty("metadata")]
		public MetadataData Metadata { get; set; } = new MetadataData();

		[JsonProperty("cancellation_details")]
		public CancellationDetailsData CancellationDetails { get; set; }

		[JsonProperty("authorization_details")]
		public AuthorizationDetailsData AuthorizationDetails { get; set; }
	}
}
