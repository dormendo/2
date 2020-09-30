using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PaymentService.ModApi
{
	public class ModApiConfiguration
	{
		public bool Enabled { get; set; }

		public string ApiUrl { get; set; }

		public string Token { get; set; }

		public string AccountId { get; set; }

		public int NDaysOnStartup { get; set; }

		public int NDaysNightly { get; set; }

		public int FetchSize { get; set; }
	}
}
