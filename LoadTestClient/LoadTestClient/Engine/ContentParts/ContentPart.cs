using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class ContentPart : IContentPart
	{
		byte[] _content;

		internal ContentPart(string part)
		{
			_content = Encoding.UTF8.GetBytes(part);
		}

		async Task IContentPart.Write(Stream stream, WorkItemData workItem)
		{
			await stream.WriteAsync(_content, 0, _content.Length);
		}
	}
}
