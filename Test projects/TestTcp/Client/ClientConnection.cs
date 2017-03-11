using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTcp.Client
{
	public class ClientConnection : IDisposable
	{
		IPEndPoint _endpoint;
        private TcpClient _client;
		private Stream _stream;
        public static int Writes = 0;

		public ClientConnection(IPEndPoint endpoint)
		{
            this._endpoint = endpoint;
		}

		public async Task Start()
		{
			try
			{
                this._client = new TcpClient(this._endpoint.AddressFamily);
                this._client.SendBufferSize = 200;
                this._client.ReceiveBufferSize = 200;
                await this._client.ConnectAsync(this._endpoint.Address, this._endpoint.Port);
                this._stream = this._client.GetStream();// new NegotiateStream(this._client.GetStream());
                //ExtendedProtectionPolicy policy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
                //NetworkCredential nc = new NetworkCredential();
                //await this._stream.AuthenticateAsClientAsync(nc, "", ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
			}
			catch (AuthenticationException ex)
			{
				Console.WriteLine(ex.ToString());
				this.Dispose();
                throw;
			}
			catch (ObjectDisposedException ex)
			{
				Console.WriteLine(ex.ToString());
				this.Dispose();
                throw;
            }
            catch (SocketException ex)
            {
                Console.WriteLine("{0}, {1}, {2}", ex.NativeErrorCode, ex.SocketErrorCode, ex.Message);
                this.Dispose();
                throw;
            }
		}

		public async Task<byte[]> Write(byte[] message)
		{
			TcpMessage requestMessage = TcpMessage.CreateMessage(message);
            byte[] requestBuffer = requestMessage.GetBufferedMessage();
            await this._stream.WriteAsync(requestBuffer, 0, requestBuffer.Length);

            TcpMessage responseMessage = await TcpMessage.ReadMessage(this._stream);
            Interlocked.Increment(ref Writes);
            return responseMessage.Data;
		}

        public async Task<bool> WriteEx(byte[] message, byte[] response)
        {
            await this._stream.WriteAsync(message, 0, message.Length);

            bool result = await TcpMessage.ReadBytes(this._stream, response);
            Interlocked.Increment(ref Writes);
            return result;
        }

        public void Dispose()
		{
			this.Dispose(true);
		}

		public void Dispose(bool removeFromCollection)
		{
			if (this._stream != null)
			{
				this._client.Close();
			}
		}
	}
}
