using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Maxima.Common.BufferManagement;
using Maxima.Common.Extensions;

namespace Maxima.Tcp
{
    /// <summary>
    /// Реализация получения/отправки через NetworkStream.
    /// </summary>
    internal sealed class NetworkStreamMessageTransport : IMessageTransport
    {
        private readonly ProcessingConfiguration _сonfiguration;
        private readonly IBufferManager _bodyBufferManager;
        private readonly IBufferManager _headerBufferManager;

        public NetworkStreamMessageTransport([NotNull] ProcessingConfiguration сonfiguration, [NotNull] BufferManagerFactory bufferManagerFactory)
        {
            if (сonfiguration == null)
            {
                throw new ArgumentNullException("сonfiguration");
            }
            if (bufferManagerFactory == null)
            {
                throw new ArgumentNullException("bufferManagerFactory");
            }

            this._сonfiguration = сonfiguration;
            this._bodyBufferManager = bufferManagerFactory.GetBufferManager(16, 16 * 1024, 1000);
            this._headerBufferManager = bufferManagerFactory.GetBufferManager(100, TcpMessageHeader.HeaderSize, 1000);
        }

        public async Task WriteResponseStreamAsync([NotNull] IClientConnectionInfo connectionInfo,
            [NotNull] TcpMessageHeader header, [NotNull] Stream input)
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

            WriteHeaderAsync(connectionInfo, header);
            await input.CopyToAsync(connectionInfo.Stream);
        }

        public async Task WriteResponseMessageAsync([NotNull] IClientConnectionInfo connectionInfo,
            [NotNull] TcpMessage responseMessage)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException("connectionInfo");
            }
            if (responseMessage == null)
            {
                throw new ArgumentNullException("responseMessage");
            }

            ArraySegment<byte>? headerBuffer = null;
            try
            {
                headerBuffer = this._headerBufferManager.CheckOutSegment();
                responseMessage.Header.Write(headerBuffer.Value.Array, headerBuffer.Value.Offset);
                // объединение буферов заголовка и тела сообщения в один список, чтобы буфер заголовка не отправлялся в отдельном сетевом пакете
                PrefixedReadonlyList<ArraySegment<byte>> mergedBuffers = new PrefixedReadonlyList<ArraySegment<byte>>(headerBuffer.Value, responseMessage.BodyBuffers);
                int totalMessageSize = responseMessage.Header.Size + TcpMessageHeader.HeaderSize;
                TimeSpan responseSendingTimeout = this._сonfiguration.GetResponseSendingTimeout(responseMessage.Header.Command);

                using (BufferReadStream responseStream = new BufferReadStream(mergedBuffers, totalMessageSize))
                {
                    responseStream.BufferedCopyTo(connectionInfo.Stream, totalMessageSize, responseSendingTimeout);
                }
            }
            finally
            {
                if (headerBuffer != null)
                {
                    this._headerBufferManager.CheckIn(headerBuffer.Value);
                }
            }
        }

        public async Task<TcpMessage> ReadAsync([NotNull] IClientConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException("connectionInfo");
            }

            byte[] headerBuffer = ReadBytes(connectionInfo.Stream, TcpMessageHeader.HeaderSize, this._сonfiguration.HeaderReadingTimeout);
            if (headerBuffer == null)
            {
                // no buffer to read
                return null;
            }

            TcpMessageHeader tcpMessageHeader = TcpMessageHeader.Read(headerBuffer);
            int maxMessageBodySize = this._сonfiguration.GetMaxMessageBodySize(tcpMessageHeader.Command);
            if (tcpMessageHeader.Size > maxMessageBodySize)
            {
                Debug.WriteLine("Message body size limit exceed. Size: {0}, Limit: {1}", tcpMessageHeader.Size, maxMessageBodySize);
                return null;
            }

            using (BufferWriteStream stream = new BufferWriteStream(this._bodyBufferManager))
            {
                try
                {
                    TimeSpan bodyReceivingTimeout = this._сonfiguration.GetMessageRecieveTimeout(tcpMessageHeader.Command);
                    int readBytesCount = connectionInfo.Stream.BufferedCopyTo(stream, tcpMessageHeader.Size, bodyReceivingTimeout);
                    if (readBytesCount != tcpMessageHeader.Size)
                    {
                        Debug.WriteLine("Message body not recieved completely. Recieved: {0}, expected: {1} bytes", readBytesCount, tcpMessageHeader.Size);
                        this._bodyBufferManager.CheckIn(stream.Buffers);
                        return null;
                    }
                    return TcpMessage.CreateMessage(tcpMessageHeader, stream.Buffers, this._bodyBufferManager);
                }
                catch (Exception)
                {
                    this._bodyBufferManager.CheckIn(stream.Buffers);
                    throw;
                }
            }
        }

        private static byte[] ReadBytes(Stream stream, int length, TimeSpan headerReadingTimeout)
        {
            byte[] bytes = new byte[length];
            int bytesReadTotally = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            int previousReadTimeout = stream.ReadTimeout;
            try
            {
                do
                {
                    TimeSpan timeoutLeft = headerReadingTimeout - stopwatch.Elapsed;
                    stream.ReadTimeout = (int)(timeoutLeft.TotalMilliseconds > 0 ? timeoutLeft.TotalMilliseconds : 1);

                    int bytesRead = stream.Read(bytes, bytesReadTotally, length - bytesReadTotally);
                    if (bytesRead == 0)
                    {
                        return null;
                    }

                    bytesReadTotally += bytesRead;
                } while (bytesReadTotally < length);
            }
            finally
            {
                stream.ReadTimeout = previousReadTimeout;
            }

            return bytes;
        }

        private void WriteHeaderAsync([NotNull] IClientConnectionInfo connectionInfo, [NotNull] TcpMessageHeader header)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentNullException("connectionInfo");
            }
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            byte[] responseBuffer = new byte[TcpMessageHeader.HeaderSize];
            header.Write(responseBuffer);
            connectionInfo.Stream.Write(responseBuffer, 0, responseBuffer.Length);
        }

        /// <summary>
        /// Proxy readonly list, "добавляющий" один элемент перед первым элементом, переданной в качестве родительской, коллекции.
        /// </summary>
        /// <typeparam name="T">Тип элементов списка.</typeparam>
        private class PrefixedReadonlyList<T> : IList<T>
        {
            private readonly T _prefixItem;
            private readonly IList<T> _list;

            public PrefixedReadonlyList(T prefixItem, [NotNull] IList<T> list)
            {
                if (list == null)
                {
                    throw new ArgumentNullException("list");
                }
                this._prefixItem = prefixItem;
                this._list = list;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new PrefixedReadonlyListEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(T item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(T item)
            {
                throw new NotSupportedException();
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public bool Remove(T item)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get { return this._list.Count + 1; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public int IndexOf(T item)
            {
                throw new NotSupportedException();
            }

            public void Insert(int index, T item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public T this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return this._prefixItem;
                    }
                    return this._list[index - 1];
                }
                set { throw new NotSupportedException(); }
            }

            private struct PrefixedReadonlyListEnumerator : IEnumerator<T>
            {
                private readonly PrefixedReadonlyList<T> _list;
                private int _index;

                public PrefixedReadonlyListEnumerator([NotNull] PrefixedReadonlyList<T> list)
                {
                    if (list == null)
                    {
                        throw new ArgumentNullException("list");
                    }
                    this._index = -1;
                    this._list = list;
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (IsEndReached)
                    {
                        return false;
                    }

                    this._index++;
                    return !IsEndReached;
                }

                public void Reset()
                {
                    this._index = -1;
                }

                public T Current
                {
                    get
                    {
                        if (this._index < 0 || IsEndReached)
                        {
                            return default(T);
                        }
                        return this._list[this._index];
                    }
                }

                object IEnumerator.Current
                {
                    get { return Current; }
                }

                private bool IsEndReached
                {
                    get { return this._index == (this._list.Count - 1); }
                }
            }
        }
    }
}