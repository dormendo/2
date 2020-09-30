using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public class PlaceData
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Address { get; set; }

		public string City { get; set; }

		public bool IsSchemeIndividual { get; set; }

		public bool IsActive { get; set; }
	}
}
