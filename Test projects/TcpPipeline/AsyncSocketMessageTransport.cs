using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Maxima.Common.BufferManagement;

namespace Maxima.Tcp
{
    /// <summary>
    /// Реализация получения/отправки через асинхронное Socket api (RecieveAsync/SendAsync).
    /// </summary>
    /// //TODO в данный момент эта реализация не работает как ожидается, но потенциально она более производительна
    internal sealed partial class AsyncSocketMessageTransport : IMessageTransport
    {
        private readonly IBufferManager _bodyBufferManager;
        private readonly Reciever<RecieveUserToken> _bodyReciever;
        private readonly IBufferManager _headerBufferManager;
        private readonly Reciever<RecieveUserToken> _headerReciever;
        private readonly Listener _listener;
        private readonly Sender<SendUserToken> _sender;

        public AsyncSocketMessageTransport([NotNull] Listener listener,
            [NotNull] ProcessingConfiguration processingConfiguration,
            [NotNull] BufferManagerFactory bufferManagerFactory)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            if (processingConfiguration == null)
            {
                throw new ArgumentNullException("processingConfiguration");
            }
            if (bufferManagerFactory == null)
            {
                throw new ArgumentNullException("bufferManagerFactory");
            }

            this._listener = listener;
            // TODO добавить поддержку ограничений из processingConfiguration
            this._bodyBufferManager = bufferManagerFactory.GetBufferManager(16, 16 * 1024, 1000);
            this._headerBufferManager = bufferManagerFactory.GetBufferManager(100, TcpMessageHeader.HeaderSize, 1000);
            ObjectPool<SocketAsyncEventArgs> socketArgsPool = new ObjectPool<SocketAsyncEventArgs>();

            //preallocate pool
            for (int i = 0; i < listener.Configuration.ConnectionCapacity; i++)
            {
                socketArgsPool.Put(new SocketAsyncEventArgs());
            }

            this._headerReciever = new Reciever<RecieveUserToken>(this._headerBufferManager, socketArgsPool);
            this._bodyReciever = new Reciever<RecieveUserToken>(this._bodyBufferManager, socketArgsPool);
            this._sender = new Sender<SendUserToken>(socketArgsPool);
        }

        public async Task WriteResponseStreamAsync([NotNull] IClientConnectionInfo connectionInfo, [NotNull] TcpMessageHeader header, [NotNull] Stream input)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException("connectionInfo");
            }
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            await WriteHeaderAsync(connectionInfo, header);
            await input.CopyToAsync(connectionInfo.Stream);
        }

        public Task WriteResponseMessageAsync([NotNull] IClientConnectionInfo connectionInfo, TcpMessage responseMessage)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException("connectionInfo");
            }
            if (responseMessage == null)
            {
                throw new ArgumentNullException("responseMessage");
            }

            TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();
            Task headerTask = WriteHeaderAsync(connectionInfo, responseMessage.Header);

            headerTask.ContinueWith(task =>
            {
                SendUserToken userToken = new SendUserToken(SendState.MessageSending, completionSource, this, false);
                this._sender.SendAsync(connectionInfo.Socket, responseMessage.BodyBuffers, responseMessage.Header.Size,
                    userToken, SendCallbackHandler);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            headerTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    completionSource.SetException(task.Exception);
                }
                else
                {
                    completionSource.SetCanceled();
                }
            }, TaskContinuationOptions.NotOnRanToCompletion);

            return completionSource.Task;
        }

        public Task<TcpMessage> ReadAsync([NotNull] IClientConnectionInfo connectionInfo)
        {
            TaskCompletionSource<TcpMessage> completionSource = new TaskCompletionSource<TcpMessage>();
            RecieveUserToken userToken = new RecieveUserToken(connectionInfo.Socket, RecieveState.HeaderRecieving, completionSource, this);

            this._headerReciever.RecieveAsync(connectionInfo.Socket, TcpMessageHeader.HeaderSize, userToken, RecieveCallbackHandler);

            return completionSource.Task;
        }

        private void SendCallbackHandler(SendUserToken outerToken, SocketError socketError,
            IList<ArraySegment<byte>> buffer)
        {
            // releasing send buffers
            if (outerToken.NeedToReleaseBuffer)
            {
                switch (outerToken.State)
                {
                    case SendState.HeaderSending:
                        ReleaseBuffer(this._headerBufferManager, buffer);
                        break;
                    case SendState.MessageSending:
                        ReleaseBuffer(this._bodyBufferManager, buffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (socketError != SocketError.Success)
            {
                outerToken.SendTaskCompletionSource.SetException(new SocketException((int)socketError));
                outerToken.Parent._listener.AcceptAsync(null);
                return;
            }

            switch (outerToken.State)
            {
                case SendState.HeaderSending:
                    outerToken.State = SendState.HeaderSent;
                    outerToken.SendTaskCompletionSource.SetResult(0);
                    break;
                case SendState.MessageSending:
                    outerToken.State = SendState.MessageSent;
                    outerToken.SendTaskCompletionSource.SetResult(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            outerToken.Parent._listener.AcceptAsync(null);
        }

        private void RecieveCallbackHandler(RecieveUserToken outerToken, SocketError socketError,
            IList<ArraySegment<byte>> buffer)
        {
            if (socketError != SocketError.Success)
            {
                outerToken.RecieveTaskCompletionSource.SetException(new SocketException((int)socketError));
                outerToken.Parent._listener.AcceptAsync(null);
                return;
            }

            switch (outerToken.State)
            {
                case RecieveState.HeaderRecieving:
                    //TODO make override for Read method to get IList<ArraySegment<byte>>
                    TcpMessageHeader header = TcpMessageHeader.Read(buffer[0].Array, buffer[0].Offset);
                    this._headerReciever.ReleaseCallbackBuffer(buffer);

                    outerToken.RecievedHeader = header;
                    outerToken.State = RecieveState.BodyRecieving;
                    this._bodyReciever.RecieveAsync(outerToken.Socket, header.Size, outerToken, RecieveCallbackHandler);
                    break;

                case RecieveState.BodyRecieving:
                    TcpMessage message = TcpMessage.CreateMessage(outerToken.RecievedHeader, buffer, this._bodyBufferManager);

                    outerToken.State = RecieveState.BodyRecieved;
                    outerToken.RecieveTaskCompletionSource.SetResult(message);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            outerToken.Parent._listener.AcceptAsync(null);
        }

        private Task WriteHeaderAsync([NotNull] IClientConnectionInfo connectionInfo, [NotNull] TcpMessageHeader header)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException("connectionInfo");
            }
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();
            IList<ArraySegment<byte>> headerBuffer = this._headerBufferManager.CheckOut(TcpMessageHeader.HeaderSize);
            Debug.Assert(headerBuffer.Count == 1);
            // TODO Write header to  IList<ArraySegment<byte>>
            header.Write(headerBuffer[0].Array, headerBuffer[0].Offset);
            SendUserToken userToken = new SendUserToken(SendState.HeaderSending, completionSource, this, true);

            this._sender.SendAsync(connectionInfo.Socket, headerBuffer, TcpMessageHeader.HeaderSize, userToken, SendCallbackHandler);

            return userToken.SendTaskCompletionSource.Task;
        }

        #region helpers

        private static void ReleaseBuffer(IBufferManager bufferManager, IEnumerable<ArraySegment<byte>> buffers)
        {
            bufferManager.CheckIn(buffers);
        }

        private static void ReleaseSocketArgs(ObjectPool<SocketAsyncEventArgs> socketArgsPool,
            SocketAsyncEventArgs socketAsyncEventArgs, EventHandler<SocketAsyncEventArgs> completedHandler)
        {
            socketAsyncEventArgs.SetBuffer(null, 0, 0);
            socketAsyncEventArgs.BufferList = null;
            socketAsyncEventArgs.UserToken = null;
            socketAsyncEventArgs.Completed -= completedHandler;
            socketArgsPool.Put(socketAsyncEventArgs);
        }

        #endregion

        #region UserTokens and states for send/recieve

        private sealed class SendUserToken
        {
            public TaskCompletionSource<int> SendTaskCompletionSource { get; private set; }
            public AsyncSocketMessageTransport Parent { get; private set; }
            public bool NeedToReleaseBuffer { get; private set; }
            public SendState State { get; set; }
            
            public SendUserToken(SendState initalState, TaskCompletionSource<int> sendTaskCompletionSource, 
                AsyncSocketMessageTransport parent, bool needToReleaseBuffer)
            {
                State = initalState;
                SendTaskCompletionSource = sendTaskCompletionSource;
                Parent = parent;
                NeedToReleaseBuffer = needToReleaseBuffer;
            }
        }

        private sealed class RecieveUserToken
        {
            [NotNull]
            public Socket Socket { get; private set; }

            [NotNull]
            public TaskCompletionSource<TcpMessage> RecieveTaskCompletionSource { get; private set; }

            [NotNull]
            public AsyncSocketMessageTransport Parent { get; private set; }

            public RecieveState State { get; set; }

            [CanBeNull]
            public TcpMessageHeader RecievedHeader { get; set; }
            
            public RecieveUserToken([NotNull] Socket socket, RecieveState initalState,
                [NotNull] TaskCompletionSource<TcpMessage> recieveTaskCompletionSource,
                [NotNull] AsyncSocketMessageTransport parent)
            {
                if (socket == null)
                {
                    throw new ArgumentNullException("socket");
                }
                if (recieveTaskCompletionSource == null)
                {
                    throw new ArgumentNullException("recieveTaskCompletionSource");
                }
                if (parent == null)
                {
                    throw new ArgumentNullException("parent");
                }

                Socket = socket;
                State = initalState;
                RecieveTaskCompletionSource = recieveTaskCompletionSource;
                Parent = parent;
            }
        }

        private enum SendState
        {
            HeaderSending,
            HeaderSent,
            MessageSending,
            MessageSent
        }

        private enum RecieveState
        {
            HeaderRecieving,
            BodyRecieving,
            BodyRecieved
        }

        #endregion
    }
}