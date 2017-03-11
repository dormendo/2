using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestTcp.Server.Sockets
{
    class SocketPool
    {
        private ConcurrentQueue<Socket> _pool;
        private EndPoint _ep;
        
        public int Count
        {
            get
            {
                return this._pool.Count;
            }
        }

        public SocketPool(EndPoint ep)
        {
            this._ep = ep;
            this._pool = new ConcurrentQueue<Socket>();
        }

        public Socket AcquireSocket()
        {
            Socket socket;
            this._pool.TryDequeue(out socket);
            return socket;
        }

        public void ReleaseSocket(Socket socket)
        {
            this._pool.Enqueue(socket);
        }
    }
}
