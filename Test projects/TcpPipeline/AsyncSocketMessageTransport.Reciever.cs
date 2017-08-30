using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using JetBrains.Annotations;
using Maxima.Common.BufferManagement;
using Maxima.Server.Tcp;

namespace Maxima.Tcp
{
    internal sealed partial class AsyncSocketMessageTransport
    {
        private sealed class Reciever<TOuterToken>
        {
            public delegate void RecieverCallback([NotNull] TOuterToken outerToken, SocketError socketError, [CanBeNull] IList<ArraySegment<byte>> buffer);

            [NotNull]
            private readonly IBufferManager _bufferManager;
            [NotNull]
            private readonly ObjectPool<SocketAsyncEventArgs> _socketArgsPool;

            public Reciever([NotNull] IBufferManager bufferManager, [NotNull] ObjectPool<SocketAsyncEventArgs> socketArgsPool)
            {
                if (bufferManager == null)
                    throw new ArgumentNullException("bufferManager");
                if (socketArgsPool == null)
                    throw new ArgumentNullException("socketArgsPool");
                _bufferManager = bufferManager;
                _socketArgsPool = socketArgsPool;
            }

            public void RecieveAsync([NotNull] Socket socket, int expectedMessageSize, [NotNull] TOuterToken outerToken, [NotNull] RecieverCallback callback)
            {
                if (socket == null)
                    throw new ArgumentNullException("socket");
                if (expectedMessageSize <= 0)
                    throw new ArgumentOutOfRangeException("expectedMessageSize", "must be positive");
                if (outerToken == null)
                    throw new ArgumentNullException("outerToken");
                if (callback == null)
                    throw new ArgumentNullException("callback");

                var buffer = _bufferManager.CheckOut(expectedMessageSize);
                var socketArgs = _socketArgsPool.Get();

                var token = new RecieveToken(socket, outerToken, buffer, expectedMessageSize, callback);
                socketArgs.BufferList = buffer;
                socketArgs.Completed += RecieveCompletedHandler;
                socketArgs.UserToken = token;

                try
                {
                    bool willRaiseEvent = socket.ReceiveAsync(socketArgs);
                    if (!willRaiseEvent)
                        RecieveCompletedHandler(this, socketArgs);
                }
                catch (SocketException socketException)
                {
                    Debug.Assert(socketException.SocketErrorCode != SocketError.Success);
                    ReleaseResourcesAndCallErrorCallback(socketArgs, token, socketException.SocketErrorCode);
                }
            }

            public void ReleaseCallbackBuffer([NotNull] IList<ArraySegment<byte>> buffer)
            {
                if (buffer == null)
                    throw new ArgumentNullException("buffer");

                ReleaseBuffer(_bufferManager, buffer);
            }

            private void RecieveCompletedHandler(object sender, [NotNull] SocketAsyncEventArgs socketEventArgs)
            {
                Debug.Assert(socketEventArgs.LastOperation == SocketAsyncOperation.Receive);
                Debug.Assert(socketEventArgs.SocketFlags != SocketFlags.Truncated);

                var token = (RecieveToken)socketEventArgs.UserToken;
                if (socketEventArgs.SocketError != SocketError.Success)
                {
                    ReleaseResourcesAndCallErrorCallback(socketEventArgs, token, socketEventArgs.SocketError);
                    return;
                }

                Debug.Assert(socketEventArgs.BytesTransferred > 0);
                token.RecievedByteCount += socketEventArgs.BytesTransferred;

                Debug.Assert(token.RecievedByteCount <= token.ExpectedBytesCount);
                if (token.RecievedByteCount == token.ExpectedBytesCount)
                {
                    ReleaseSocketArgs(_socketArgsPool, socketEventArgs, RecieveCompletedHandler);

                    token.Callback(token.OuterToken, socketEventArgs.SocketError, token.Buffer);
                    return;
                }

                socketEventArgs.BufferList = token.Buffer.Slice(token.RecievedByteCount);
                try
                {
                    bool willRaiseEvent = token.Socket.ReceiveAsync(socketEventArgs);
                    if (!willRaiseEvent)
                        RecieveCompletedHandler(null, socketEventArgs);
                }
                catch (SocketException socketException)
                {
                    ReleaseResourcesAndCallErrorCallback(socketEventArgs, token, socketException.SocketErrorCode);
                }
            }

            private void ReleaseResourcesAndCallErrorCallback([NotNull] SocketAsyncEventArgs socketEventArgs, [NotNull] RecieveToken token, SocketError socketError)
            {
                ReleaseBuffer(_bufferManager, token.Buffer);
                ReleaseSocketArgs(_socketArgsPool, socketEventArgs, RecieveCompletedHandler);

                token.Callback(token.OuterToken, socketError, null);
            }

            private sealed class RecieveToken
            {
                [NotNull]
                public Socket Socket { get; private set; }

                [NotNull]
                public TOuterToken OuterToken { get; private set; }

                public int ExpectedBytesCount { get; private set; }

                [NotNull]
                public IList<ArraySegment<byte>> Buffer { get; private set; }

                [NotNull]
                public RecieverCallback Callback { get; private set; }

                public int RecievedByteCount { get; set; }

                public RecieveToken([NotNull] Socket socket, [NotNull] TOuterToken outerToken, 
                    [NotNull] IList<ArraySegment<byte>> buffer, int expectedBytesCount, 
                    [NotNull] RecieverCallback callback)
                {
                    if (socket == null)
                        throw new ArgumentNullException("socket");
                    if (outerToken == null)
                        throw new ArgumentNullException("outerToken");
                    if (buffer == null)
                        throw new ArgumentNullException("buffer");
                    if (expectedBytesCount <= 0)
                        throw new ArgumentOutOfRangeException("expectedBytesCount", "must be positive");
                    if (callback == null)
                        throw new ArgumentNullException("callback");

                    OuterToken = outerToken;
                    ExpectedBytesCount = expectedBytesCount;
                    Buffer = buffer;
                    Socket = socket;
                    Callback = callback;
                }
            }
        }
    }
}
