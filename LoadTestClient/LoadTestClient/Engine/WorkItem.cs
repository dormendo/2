using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class WorkItem : IDisposable
	{
		internal WorkItemData Data { get; set; }

		internal TcpClient Client { get; set; }

		internal WorkItem()
		{
		}

		~WorkItem()
		{

		}

		internal void FailConnection()
		{
			CloseClient();
			Data.ConnectionFailed = true;
		}

		internal void FailRequest()
		{
			CloseClient();
			Data.ConnectionFailed = true;
		}

		internal void FailConvert()
		{
			Data.ConvertFailed = true;
		}

		internal void CloseClient()
		{
			if (Client != null)
			{
				Client.Dispose();
				Client = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (!disposing)
			{
				GC.SuppressFinalize(this);
			}

			CloseClient();
		}
	}
}
