using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class NullStream : Stream
	{
		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => true;

		public override long Length => 0;

		public override long Position { get => 0; set { } }

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return count;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0;
		}

		public override void SetLength(long value)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
		}
	}
}
