using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Maxima.Common.BufferManagement;
using Maxima.Log;
using Maxima.Server.Tcp;

namespace Maxima.Tcp
{
    public sealed class Listener : IStartable
    {
        private static int _totalConnectionsCount; // glCnt_Total
        private static long _lastConnectionTime; // GlLastExe
        private static ConnectionHolder _connectionHolder;

        private readonly ListenerConfiguration _configuration;
        private readonly BufferManagerFactory _bufferManagerFactory;
        private readonly IMessageTransport _messageTransport;
        private readonly IBufferManager _compressionBufferManager;

        private static int _accepts;
        private int _connectionCounter;

        private readonly ObjectPool<ClientConnectionInfo> _clientConnectionInfoPool;
        private readonly ObjectPool<SocketAsyncEventArgs> _disconnectSocketArgsPool;
        private readonly ObjectPool<Socket> _socketPool;
        private Socket _listenSocket;
        private readonly ManualResetEvent _stoppedWaitHandle;
        private CancellationToken _cancelationToken;

        [NotNull]
        internal IMessageTransport MessageTransport
        {
            get { return this._messageTransport; }
        }

        [NotNull]
        internal ListenerConfiguration Configuration
        {
            get { return this._configuration; }
        }

        public static ConnectionsInfo ConnectionsInfo
        {
            get
            {
                int totalConnectionsCount = Thread.VolatileRead(ref _totalConnectionsCount);
                long lastConnectionTime = 0;
                if (totalConnectionsCount > 0)
                {
                    lastConnectionTime = Thread.VolatileRead(ref _lastConnectionTime);
                }

                return new ConnectionsInfo(totalConnectionsCount, lastConnectionTime);
            }
        }

        public static ICollection<ServerConnection> Connections
        {
            get { return _connectionHolder.Connections; }
        }

        public WaitHandle StoppedWaitHandle
        {
            get { return _stoppedWaitHandle; }
        }

        public Listener([NotNull] ListenerConfiguration configuration,
            [NotNull] ProcessingConfiguration processingConfiguration,
            [NotNull] BufferManagerFactory bufferManagerFactory,
            [NotNull] MessageTransportFactory messageTransportFactory)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (processingConfiguration == null)
            {
                throw new ArgumentNullException("processingConfiguration");
            }
            if (bufferManagerFactory == null)
            {
                throw new ArgumentNullException("bufferManagerFactory");
            }
            if (messageTransportFactory == null)
            {
                throw new ArgumentNullException("messageTransportFactory");
            }

            this._configuration = configuration;
            this._bufferManagerFactory = bufferManagerFactory;

            this._clientConnectionInfoPool = new ObjectPool<ClientConnectionInfo>();
            this._disconnectSocketArgsPool = new ObjectPool<SocketAsyncEventArgs>();
            this._socketPool = new ObjectPool<Socket>();

            _connectionHolder = new ConnectionHolder(configuration.ConnectionCapacity);
            _stoppedWaitHandle = new ManualResetEvent(false);
            _messageTransport = messageTransportFactory.GetMessageTransport(this);
            _compressionBufferManager = bufferManagerFactory.GetBufferManager(1024, 1024 * 8);
        }

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and context objects.  These objects do not 
        /// need to be preallocated or reused, by is done this way to illustrate how the API can easily be used
        /// to create reusable objects to increase server performance.
        /// </summary>
        private void Init()
        {
            string message = string.Format("Listener on {0}, MessageTransport = {1}, BufferManager = {2}",
                Configuration.EndPoint, MessageTransport.GetType().Name, _compressionBufferManager.GetType().Name);

            Logger.Info(message, LogType.System);
            Console.WriteLine(message);

            // preallocate pool
            for (int i = 0; i < this._configuration.ConnectionCapacity; i++)
            {
                SocketAsyncEventArgs disconnectSocketEventArg = new SocketAsyncEventArgs { DisconnectReuseSocket = true };
                disconnectSocketEventArg.Completed += DisconnectSocketCompletedHandler;
                this._disconnectSocketArgsPool.Put(disconnectSocketEventArg);

                this._clientConnectionInfoPool.Put(new ClientConnectionInfo());
            }
        }

        public void Start(CancellationToken cancellationToken)
        {
            _cancelationToken = cancellationToken;
            Init();

            this._listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveBufferSize = Configuration.ListenSocketReceiveBufferSize,
                SendBufferSize = Configuration.ListenSocketSendBufferSize
            };
            this._listenSocket.Bind(Configuration.EndPoint);

            this._listenSocket.Listen(Configuration.ListenBacklog);

            Task.Delay(-1, cancellationToken)
                .ContinueWith(task => { Stop(); }, TaskContinuationOptions.OnlyOnCanceled);

            for (int i = 0; i < Configuration.InitialAcceptsCount; i++)
            {
                var acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += AcceptCompletedHandler;

                AcceptAsync(acceptEventArg);
            }
        }

        public void Terminate()
        {
            Stop();
        }

        internal void AcceptAsync([NotNull] SocketAsyncEventArgs acceptEventArg)
        {
            // socket must be cleared since the context object is being reused
            acceptEventArg.AcceptSocket = this._socketPool.TryGet();

            Socket listenSocket = this._listenSocket;
            if (listenSocket == null)
            {
                return;
            }

            try
            {

                bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
                if (!willRaiseEvent)
                {
                    Task.Factory.StartNew(() => ProcessAccept(acceptEventArg), _cancelationToken);
                }
            }
            catch (Exception exception)
            {
                Logger.ErrorException("Error on accept", exception, LogType.System);
            }
        }

        /// <summary>
        /// This method is the callback method associated with Socket.AcceptAsync operations and is invoked
        /// when an accept operation is complete
        /// </summary>
        private void AcceptCompletedHandler(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private async void ProcessAccept(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Interlocked.Increment(ref _accepts);
            // Get the socket for the accepted client connection and put it into the object user token
            ClientConnectionInfo connectionInfo = this._clientConnectionInfoPool.Get();
            try
            {
                Socket acceptSocket = socketAsyncEventArgs.AcceptSocket;

                // Accept the next connection request
                AcceptAsync(socketAsyncEventArgs);

                connectionInfo.Init(acceptSocket, new NetworkStream(acceptSocket));

                await ProcessConnection(connectionInfo);
            }
            catch (SocketException)
            {
                CloseClientSocket(connectionInfo);
            }
            catch (IOException)
            {
                CloseClientSocket(connectionInfo);
            }
        }

        private async Task ProcessConnection(ClientConnectionInfo connectionInfo)
        {
            int id = Interlocked.Increment(ref this._connectionCounter);
            ServerConnection connection = new ServerConnection(this, _compressionBufferManager, connectionInfo, id);

            await connection.Start();
        }

        public void AddConnection(ServerConnection connection)
        {
            Interlocked.Increment(ref _totalConnectionsCount);
            Interlocked.Exchange(ref _lastConnectionTime, DateTime.Now.Ticks);

            _connectionHolder.AddConnection(connection);
        }

        public void RemoveConnection(ServerConnection connection)
        {
            _connectionHolder.RemoveConnection(connection);
        }

        internal void CloseClientSocket(ClientConnectionInfo connectionInfo)
        {
            // close the socket associated with the client
            try
            {
                if (!connectionInfo.IsInited)
                {
                    this._clientConnectionInfoPool.Put(connectionInfo);
                    return;
                }

                SocketAsyncEventArgs args = this._disconnectSocketArgsPool.Get();
                args.UserToken = connectionInfo;

                connectionInfo.Socket.Shutdown(SocketShutdown.Send);
                bool willRaiseEvent = connectionInfo.Socket.DisconnectAsync(args);
                if (!willRaiseEvent)
                {
                    DisconnectSocketCompletedHandler(this, args);
                }
            }
            // throws if client process has already closed
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            this.Stop();
        }

        private void Stop()
        {
            if (this._listenSocket != null)
            {
                if (this._listenSocket.Connected)
                {
                    this._listenSocket.Shutdown(SocketShutdown.Both);
                }
                this._listenSocket.Close();
                this._listenSocket = null;
            }
            _connectionHolder.Clean();
            _stoppedWaitHandle.Set();
        }

        private void DisconnectSocketCompletedHandler(object sender, SocketAsyncEventArgs e)
        {
            Debug.Assert(e.SocketError == SocketError.Success);

            ClientConnectionInfo connectionInfo = (ClientConnectionInfo)e.UserToken;

            this._socketPool.Put(connectionInfo.Socket);
            connectionInfo.Stream.Dispose();
            connectionInfo.Clean();

            e.UserToken = null;
            this._disconnectSocketArgsPool.Put(e);

            this._clientConnectionInfoPool.Put(connectionInfo);
        }
    }
}