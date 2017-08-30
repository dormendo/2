using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using JetBrains.Annotations;
using Maxima.Server.Tcp;

namespace Maxima.Tcp
{
    internal sealed partial class AsyncSocketMessageTransport
    {
        private sealed class Sender<TOuterToken>
        {
            public delegate void SenderCallback([NotNull] TOuterToken outerToken, SocketError socketError, [NotNull] IList<ArraySegment<byte>> buffer);

            private readonly ObjectPool<SocketAsyncEventArgs> _socketArgsPool;

            public Sender([NotNull] ObjectPool<SocketAsyncEventArgs> socketArgsPool)
            {
                if (socketArgsPool == null)
                    throw new ArgumentNullException("socketArgsPool");

                _socketArgsPool = socketArgsPool;
            }

            public void SendAsync([NotNull] Socket socket, [NotNull] IList<ArraySegment<byte>> buffers, int messageSize,
                [NotNull] TOuterToken outerToken, [NotNull] SenderCallback callback)
            {
                if (socket == null)
                    throw new ArgumentNullException("socket");
                if (buffers == null)
                    throw new ArgumentNullException("buffers");
                if (messageSize <= 0)
                    throw new ArgumentOutOfRangeException("messageSize", "must be positive");
                if (outerToken == null)
                    throw new ArgumentNullException("outerToken");
                if (callback == null)
                    throw new ArgumentNullException("callback");

                var socketArgs = _socketArgsPool.Get();
                var token = new SendToken(socket, outerToken, buffers, messageSize, callback);
                socketArgs.BufferList = buffers.Trim(messageSize);
                socketArgs.Completed += SendCompletedHandler;
                socketArgs.UserToken = token;

                try
                {
                    bool willRaiseEvent = socket.SendAsync(socketArgs);
                    if (!willRaiseEvent)
                        SendCompletedHandler(this, socketArgs);
                }
                catch (SocketException socketException)
                {
                    Debug.Assert(socketException.SocketErrorCode != SocketError.Success);
                    ReleaseResourcesAndCallErrorCallback(socketArgs, token, socketException.SocketErrorCode);
                }
            }

            private void SendCompletedHandler(object sender, [NotNull] SocketAsyncEventArgs socketEventArgs)
            {
                Debug.Assert(socketEventArgs.LastOperation == SocketAsyncOperation.Send);
                Debug.Assert(socketEventArgs.SocketFlags != SocketFlags.Truncated);

                var token = (SendToken)socketEventArgs.UserToken;
                if (socketEventArgs.SocketError != SocketError.Success)
                {
                    ReleaseResourcesAndCallErrorCallback(socketEventArgs, token, socketEventArgs.SocketError);
                    return;
                }

                Debug.Assert(socketEventArgs.BytesTransferred > 0);
                token.SentByteCount += socketEventArgs.BytesTransferred;

                Debug.Assert(token.SentByteCount <= token.ExpectedBytesCount);
                if (token.SentByteCount == token.ExpectedBytesCount)
                {
                    ReleaseSocketArgs(_socketArgsPool, socketEventArgs, SendCompletedHandler);
                    token.Callback(token.OuterToken, socketEventArgs.SocketError, token.Buffer);
                    return;
                }

                // TODO может быть имеет смысл сохранить в token оригинальный BufferList и сразу создать клон, который тут модифицировать?
                socketEventArgs.BufferList = token.Buffer.Slice(token.SentByteCount);
                try
                {
                    bool willRaiseEvent = token.Socket.SendAsync(socketEventArgs);
                    if (!willRaiseEvent)
                        SendCompletedHandler(null, socketEventArgs);
                }
                catch (SocketException socketException)
                {
                    ReleaseResourcesAndCallErrorCallback(socketEventArgs, token, socketException.SocketErrorCode);
                }
            }

            private void ReleaseResourcesAndCallErrorCallback([NotNull] SocketAsyncEventArgs socketEventArgs, [NotNull] SendToken token, SocketError socketError)
            {
                ReleaseSocketArgs(_socketArgsPool, socketEventArgs, SendCompletedHandler);
                token.Callback(token.OuterToken, socketError, token.Buffer);
            }

            private sealed class SendToken
            {
                [NotNull]
                public Socket Socket { get; private set; }

                [NotNull]
                public TOuterToken OuterToken { get; private set; }

                public int ExpectedBytesCount { get; private set; }

                [NotNull]
                public IList<ArraySegment<byte>> Buffer { get; private set; }

                [NotNull]
                public SenderCallback Callback { get; private set; }

                public int SentByteCount { get; set; }

                public SendToken([NotNull] Socket socket, [NotNull] TOuterToken outerToken,
                    [NotNull] IList<ArraySegment<byte>> buffer, int expectedBytesCount,
                    [NotNull] SenderCallback callback)
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

                    Socket = socket;
                    OuterToken = outerToken;
                    ExpectedBytesCount = expectedBytesCount;
                    Buffer = buffer;
                    Callback = callback;
                }
            }
        }
    }
}
