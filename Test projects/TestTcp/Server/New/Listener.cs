//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace TestTcp.Server
//{
//    public class Listener2 : IDisposable
//    {
//        private int _port;
//        private IPEndPoint _localEndPoint;
//        private Socket _listenSocket;
//        private int _connectionCounter;
//        private object _collectionLock = new object();
//        private MainService _service = new MainService();
//        private ConnectionHolder _connectionHolder;
//        private const int ConnectionCapacity = 10000;
//        public static int Accepts = 0;
//        private SemaphoreSlim _maxNumberAcceptedClients;

//        public Listener2(int port)
//        {
//            this._port = port;
//            this._localEndPoint = new IPEndPoint(IPAddress.IPv6Any, this._port);
//            this._connectionHolder = new ConnectionHolder(ConnectionCapacity);
//            this._maxNumberAcceptedClients = new SemaphoreSlim(ConnectionCapacity, ConnectionCapacity);
//        }

//        public MainService Service
//        {
//            get
//            {
//                return this._service;
//            }
//        }

//        public void Start()
//        {
//            this._listenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
//            this._listenSocket.Bind(this._localEndPoint);
//            // start the server with a listen backlog of 100 connections
//            this._listenSocket.Listen(10000);
//            this.StartAccept(null);
//        }

//        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
//        {
//            if (acceptEventArg == null)
//            {
//                acceptEventArg = new SocketAsyncEventArgs();
//                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
//            }
//            else
//            {
//                // socket must be cleared since the context object is being reused
//                acceptEventArg.AcceptSocket = null;
//            }

//            this._maxNumberAcceptedClients.Wait();
//            bool willRaiseEvent = this._listenSocket.AcceptAsync(acceptEventArg);
//            if (!willRaiseEvent)
//            {
//                ProcessAccept(acceptEventArg);
//            }
//        }

//        private async Task AcceptLoop()
//        {
//            if (!ExecutionContext.IsFlowSuppressed())
//            {
//                ExecutionContext.SuppressFlow();
//            }

//            while (true)
//            {
//                //todo Ввести отмену на AcceptTcpClientAsync
//                TcpClient client;
//                try
//                {
//                    client = await this._listener.AcceptTcpClientAsync();
//                    Interlocked.Increment(ref Accepts);
//                }
//                catch (ObjectDisposedException)
//                {
//                    if (ExecutionContext.IsFlowSuppressed())
//                    {
//                        ExecutionContext.RestoreFlow();
//                    }
//                    return;
//                }
//                catch (SocketException)
//                {
//                    continue;
//                }

//                Task t = Task.Factory.StartNew(() => this.AcceptConnection(client));
//            }
//        }

//        private async void AcceptConnection(TcpClient client)
//        {
//            int id = Interlocked.Increment(ref this._connectionCounter);
//            ServerConnection connection = new ServerConnection(client, id, this);
//            await connection.Start();
//        }

//        public void AddConnection(ServerConnection connection)
//        {
//            this._connectionHolder.AddConnection(connection);
//        }

//        public void Dispose()
//        {
//            if (this._listener != null)
//            {
//                this._listener.Stop();
//                this._listener = null;
//            }

//            this._connectionHolder.Clean();
//        }
//    }
//}
