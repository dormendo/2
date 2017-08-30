using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace Maxima.Tcp
{
    public sealed class ClientConnectionInfo : IClientConnectionInfo
    {
        private Socket _socket;
        private Stream _stream;

        public ClientConnectionInfo()
        {
            TimeFromAccept = new Stopwatch();
        }

        public bool IsInited { get; private set; }

        [NotNull]
        public Stream Stream
        {
            get
            {
                EnsureInited();
                return this._stream;
            }
        }

        [NotNull]
        public Socket Socket
        {
            get
            {
                EnsureInited();
                return this._socket;
            }
        }

        [NotNull]
        public Stopwatch TimeFromAccept { get; private set; }

        public void Init([NotNull] Socket socket, [NotNull] Stream stream)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this._socket = socket;
            this._stream = stream;
            TimeFromAccept.Restart();
            this.IsInited = true;
        }

        public void Clean()
        {
            TimeFromAccept.Stop();
            this.IsInited = false;
            this._stream.Dispose();
            this._stream = null;
            this._socket = null;
        }

        private void EnsureInited()
        {
            if (!this.IsInited)
            {
                throw new InvalidOperationException("Can not use not inited object");
            }
        }
    }
}