using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tips24.PaymentService
{
	public class HookCompanyData
	{
		public string companyInn { get; set; }
		public string companyKpp { get; set; }
		public HookOperation operation { get; set; }
		public string SHA1Hash { get; set; }

		public bool CheckHash(string token)
		{
			string v = token.Substring(0, 10) + "&" + operation.id;
			byte[] ba = Encoding.UTF8.GetBytes(v);
			byte[] hash;

			using (SHA1 hf = SHA1.Create())
			{
				hash = hf.ComputeHash(ba);
			}

			string hex = ByteArrayToHex.ToHex(hash);
			return (string.Compare(hex, SHA1Hash, true) == 0);
		}
	}

	public class HookOperation
	{
		public string id { get; set; }
		public string companyId { get; set; }
		public string status { get; set; }
		public string category { get; set; }
		public string contragentName { get; set; }
		public string contragentInn { get; set; }
		public string contragentKpp { get; set; }
		public string contragentBankAccountNumber { get; set; }
		public string contragentBankName { get; set; }
		public string contragentBankBic { get; set; }
		public string currency { get; set; }
		public decimal amount { get; set; }
		public decimal amountWithCommission { get; set; }
		public string bankAccountNumber { get; set; }
		public string paymentPurpose { get; set; }
		public string executed { get; set; }
		public string created { get; set; }
		public string docNumber { get; set; }
		public string kbk { get; set; }
		public string oktmo { get; set; }
		public string paymentBasis { get; set; }
		public string taxCode { get; set; }
		public string taxDocNum { get; set; }
		public string taxDocDate { get; set; }
		public string payerStatus { get; set; }
		public string uin { get; set; }
	}
}
