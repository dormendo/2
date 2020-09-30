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
	public class CreatePaymentInDoc
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
			public string Currency { get; set; } = "RUB";
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

			[JsonProperty("phone")]
			public string Phone { get; set; }
		}

		public class ConfirmationData
		{
			[JsonProperty("type")]
			public string Type { get; set; } = "external";

			[JsonProperty("locale")]
			public string Locale { get; set; } = "ru_RU";
		}

		public class MetadataData
		{
			[JsonProperty("ir_id")]
			public int RequestId { get; set; }
		}

		[JsonProperty("amount")]
		public AmountData Amount { get; set; } = new AmountData();

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("payment_method_data")]
		public MethodData PaymentMethodData { get; set;  } = new MethodData();

		[JsonProperty("confirmation")]
		public ConfirmationData Confirmation { get; set; } = new ConfirmationData();

		[JsonProperty("capture")]
		public bool Capture { get; set; }

		[JsonProperty("metadata")]
		public MetadataData Metadata { get; set; } = new MetadataData();
	}
}
