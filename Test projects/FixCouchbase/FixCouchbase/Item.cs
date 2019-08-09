using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixCouchbase
{
	[Serializable]
	public class Item
	{
		public int Id;

		public string Name;

		public string Description;

		public Dictionary<string, Item2> Items1;

		public Dictionary<string, Item2> Items2;
	}

	[Serializable]
	public class Item2
	{
		public int Id;

		public string Name;

		public string Description;
	}
}
