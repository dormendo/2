using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTcp.Server
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerCall)]
	public class MainService : IService
	{
        public static int Calls = 0;

		public byte[] ExecuteProcess(byte[] message)
		{
            Interlocked.Increment(ref Calls);
			//this.Invert(message);
            return message;
		}

        public Task<byte[]> ExecuteWcfProcess(byte[] message)
        {
            //this.Invert(message);
            return Task.FromResult(message);
        }

        public unsafe void Invert(byte[] message)
        {
            int count = message.Length;
            int mask = 255;
            fixed (byte* m1 = message)
            {
                byte* p1 = m1;
                for (int i = 0; i < count; i++, p1++)
                {
                    *p1 = (byte)(((int)*p1) ^ mask);
                }
            }
        }
	}
}
