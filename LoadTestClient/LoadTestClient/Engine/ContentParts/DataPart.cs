using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class DataPart : IContentPart
	{
		int _columnOrdinal;

		internal DataPart(int columnOrdinal)
		{
			_columnOrdinal = columnOrdinal;
		}

		async Task IContentPart.Write(Stream stream, WorkItemData workItem)
		{
			string value = workItem.Row.GetValue(_columnOrdinal);
			if (!string.IsNullOrEmpty(value))
			{
				byte[] ba = Encoding.UTF8.GetBytes(value);
				await stream.WriteAsync(ba, 0, ba.Length);
			}
		}
	}
}
