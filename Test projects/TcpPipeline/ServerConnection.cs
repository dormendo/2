using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Maxima.Common.BufferManagement;
using Maxima.Log;
using Maxima.Server;

namespace Maxima.Tcp
{
    public sealed class ServerConnection : IDisposable
    {
        private const string SocketErrorFolder = "Socket.Error";
        private const string CommunicationErrorFolder = "Communication.Error";
        private const string ExecuteFolder = "___Execute";

        private readonly Socket _clientSocket;
        private readonly ClientConnectionInfo _connectionInfo;
        private readonly int _id;
        private readonly Listener _listener;
        // используется для компрессии и декомпрессии сообщений
        private readonly IBufferManager _bufferManager;
        private volatile bool _isDisposed;

        private CommandContext _commandContext;

        public int Id
        {
            get { return this._id; }
        }

        #region connection statistic info

        public DateTime StartTime { get; private set; }

        public int ThreadId { get; private set; }

        public string Command { get; private set; }

        public string CurrPos
        {
            get
            {
                CommandContext commandContext = this._commandContext;
                return commandContext != null ? commandContext.CurrPos : "CommandProcessingIsNotStarted";
            }
        }

        public string CallerIp
        {
            get { return ClientAddress.ToString(); }
        }

        #endregion

        public IPAddress ClientAddress { get; private set; }

        private IMessageTransport MessageTransport
        {
            get { return this._listener.MessageTransport; }
        }

        public bool IsDisposed
        {
            get { return this._isDisposed; }
        }

        public ServerConnection([NotNull] Listener listener, [NotNull] IBufferManager bufferManager, [NotNull] ClientConnectionInfo connectionInfo, int id)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException("connectionInfo");
            }
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            if (bufferManager == null)
            {
                throw new ArgumentNullException("bufferManager");
            }

            this._connectionInfo = connectionInfo;
            this._clientSocket = connectionInfo.Socket;
            this._id = id;
            this._listener = listener;
            this._bufferManager = bufferManager;

            StartTime = DateTime.Now;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            ClientAddress = ((IPEndPoint) this._clientSocket.RemoteEndPoint).Address;
        }

        public async Task Start()
        {
            this._clientSocket.ReceiveBufferSize = this._listener.Configuration.ClientSocketReceiveBufferSize;
            this._clientSocket.SendBufferSize = this._listener.Configuration.ClientSocketSendBufferSize;
            this._listener.AddConnection(this);

            try
            {
                await ProccessMessage();
            }
            catch (IOException ex)
            {
                SocketException socketException = ex.InnerException as SocketException;
                if (socketException != null)
                {
                    string errText = string.Format("SocketException. {0}, {1}", socketException.SocketErrorCode, socketException);
                    if (Glvar.RunAsServise)
                    {
                        ServerLog.AddFBLog(SocketErrorFolder, string.Format("| {0,8} | {1}\r\n{2}", this.Command, this.ClientAddress, errText));
                    }
                    else
                    {
                        Console.WriteLine(errText);
                        Logger.ErrorException("ProccessMessage io error", ex);
                    }
                }
                else
                {
                    this.LogProcessError(ex);
                }
            }
            catch (Exception ex)
            {
                this.LogProcessError(ex);
            }
            finally
            {
                this._listener.RemoveConnection(this);
                Dispose();
            }
        }

        private void LogProcessError(Exception ex)
        {
            if (Glvar.RunAsServise)
            {
                ServerLog.AddFBLog(CommunicationErrorFolder, string.Format("| {0,8} | {1}\r\n{2}", this.Command, this.ClientAddress, ex));
            }
            else
            {
                Console.WriteLine(ex.ToString());
                Logger.ErrorException("ProccessMessage error", ex);
            }
        }

        private async Task ProccessMessage()
        {
            TcpMessage requestMessage = await MessageTransport.ReadAsync(this._connectionInfo);
            if (requestMessage == null)
            {
                throw new Exception("Сообщение имеет некорректный формат");
            }

            Command = requestMessage.Header.Command;

            bool isNeedExeLog = ServerCommands.IsNeedExeLog(Command);
            Stopwatch stopwatch = null;
            if (isNeedExeLog)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            using (CommandContext commandContext = new CommandContext(this, requestMessage, _bufferManager))
            {
                this._commandContext = commandContext;
                await CommandDispatcher.Instance.ExecuteAsync(commandContext);
                this._commandContext = null;
            }

            if (isNeedExeLog)
            {
                stopwatch.Stop();
                ServerLog.AddFBLog(ExecuteFolder,
                    string.Format("| {0,8} | {1,8} | {2}", Command, stopwatch.ElapsedMilliseconds, ClientAddress));
            }
        }

        public async Task WriteResponse(TcpMessage responseMessage)
        {
            await MessageTransport.WriteResponseMessageAsync(this._connectionInfo, responseMessage);
        }

        /// <summary>
        /// Посылка заголовка и потока.
        /// </summary>
        public async Task WriteStreamDataAsync(TcpMessageHeader header, Stream input)
        {
            await MessageTransport.WriteResponseStreamAsync(this._connectionInfo, header, input);
        }

        public void Dispose()
        {
            this._isDisposed = true;
            this._listener.CloseClientSocket(this._connectionInfo);
        }
    }
}