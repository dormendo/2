using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTcp
{
    public class TcpMessage
    {
        private Header _messageHeader;

        private byte[] _data;

        public byte[] Data
        {
            get
            {
                return this._data;
            }
        }

        private class Header
        {
            public const int HeaderSize = 4;
            public int Size;

            public unsafe void Write(byte[] buffer)
            {
                fixed (byte* b = buffer)
                {
                    int* p = (int*)b;
                    *p = this.Size;
                }
            }

            public static unsafe Header Read(byte[] buffer)
            {
                Header header = new Header();
                fixed (byte* b = buffer)
                {
                    int* p = (int*)b;
                    header.Size = *p;
                }
                return header;
            }
        }
        
        public static TcpMessage CreateMessage(byte[] data)
        {
            return new TcpMessage(data);
        }

        private TcpMessage(byte[] data)
        {
            this._messageHeader = new Header() { Size = data.Length };
            this._data = data;
        }

        private TcpMessage(byte[] data, Header header)
        {
            this._messageHeader = header;
            this._data = data;
        }

        public byte[] GetBufferedMessage()
        {
            byte[] buffer = new byte[this._data.Length + Header.HeaderSize];
            this._messageHeader.Write(buffer);
            Buffer.BlockCopy(this._data, 0, buffer, 4, this._data.Length);
            return buffer;
        }

        public static async Task<TcpMessage> ReadMessage(Stream stream)
        {
            byte[] headerBuffer = await ReadBytes(stream, Header.HeaderSize);
            if (headerBuffer == null)
            {
                return null;
            }

            Header header = Header.Read(headerBuffer);
            byte[] data = await ReadBytes(stream, header.Size);
            if (headerBuffer == null)
            {
                return null;
            }

            return new TcpMessage(data, header);
        }

        private static async Task<byte[]> ReadBytes(Stream stream, int length)
        {
            byte[] bytes = new byte[length];
			int bytesReadTotally = 0;
			do
			{
                int bytesRead = await stream.ReadAsync(bytes, bytesReadTotally, length - bytesReadTotally);
                if (bytesRead == 0)
                {
                    return null;
                }
                bytesReadTotally += bytesRead;
			}
            while (bytesReadTotally < length);
			return bytes;

        }

        public static async Task<bool> ReadBytes(Stream stream, byte[] buffer)
        {
            int bytesReadTotally = 0;
            do
            {
                int bytesRead = await stream.ReadAsync(buffer, bytesReadTotally, buffer.Length - bytesReadTotally);
                if (bytesRead == 0)
                {
                    return false;
                }
                bytesReadTotally += bytesRead;
            }
            while (bytesReadTotally < buffer.Length);
            return true;
        }
    }
}
