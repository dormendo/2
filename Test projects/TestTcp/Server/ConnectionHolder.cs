using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTcp.Server
{
    public class ConnectionHolder
    {
        private int _capacity;
        private ServerConnection[] _connections;
        private int _nextIndex = -1;

        public ConnectionHolder(int capacity)
        {
            this._capacity = capacity;
            this._connections = new ServerConnection[this._capacity];
        }

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        static extern void ZeroMemory(IntPtr dest, IntPtr size);

        public void AddConnection(ServerConnection connection)
        {
            int index = Interlocked.Increment(ref this._nextIndex) % this._capacity;
            this._connections[index] = connection;
        }

        public void RemoveConnection(int index)
        {
            this._connections[index] = null;
        }

        public void Clean()
        {
            for (int i = 0; i < this._connections.Length; i++)
            {
                ServerConnection connection = this._connections[i];
                if (connection != null && !connection.IsDisposed)
                {
                    try
                    {
                        connection.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
            }
        }
    }
}
