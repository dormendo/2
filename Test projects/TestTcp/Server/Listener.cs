using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTcp.Server
{
	public class Listener : IDisposable
	{
		private int _port;
		private TcpListener _listener;
		private int _connectionCounter;
		private object _collectionLock = new object();
		private MainService _service = new MainService();
        private ConnectionHolder _connectionHolder;
        private const int ConnectionCapacity = 10000;
        public static int Accepts = 0;

		public Listener(int port)
		{
			this._port = port;
		    this._connectionHolder = new ConnectionHolder(ConnectionCapacity);
        }

        public MainService Service
        {
            get
            {
                return this._service;
            }
        }

		public async void Start()
		{
			this._listener = new TcpListener(IPAddress.IPv6Any, this._port);
			this._listener.Start(10000);
            //Thread th = new Thread(AcceptLoop, 102400) { IsBackground = true };
            //th.Start();
            await this.AcceptLoop();
		}

		private async Task AcceptLoop()
		{
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
            }

            while (true)
			{
				//todo Ввести отмену на AcceptTcpClientAsync
                TcpClient client;
                try
                {
                    client = await this._listener.AcceptTcpClientAsync();
                    Interlocked.Increment(ref Accepts);
                }
                catch (ObjectDisposedException)
                {
                    if (ExecutionContext.IsFlowSuppressed())
                    {
                        ExecutionContext.RestoreFlow();
                    }
                    return;
                }
                catch (SocketException)
                {
                    continue;
                }

                Task t = Task.Factory.StartNew(() => this.AcceptConnection(client));
            }
		}

        private async void AcceptConnection(TcpClient client)
        {
            int id = Interlocked.Increment(ref this._connectionCounter);
            ServerConnection connection = new ServerConnection(client, id, this);
            await connection.Start();
        }

		public void AddConnection(ServerConnection connection)
		{
		    this._connectionHolder.AddConnection(connection);
        }

        public void RemoveConnection(int id)
        {
            this._connectionHolder.RemoveConnection(id);
        }

		public void Dispose()
		{
			if (this._listener != null)
			{
				this._listener.Stop();
				this._listener = null;
			}

			this._connectionHolder.Clean();
		}
	}
}
