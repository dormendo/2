using System;
using System.Collections.Generic;
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

namespace TestTcp.Server
{
	public class ServerConnection : IDisposable
	{
		private TcpClient _client;
		private NegotiateStream _stream;
		private int _id;
		private Listener _listener;
        private volatile bool _isDisposed;

		public int Id
		{
			get
			{
				return this._id;
			}
		}

        public bool IsDisposed
        {
            get
            {
                return this._isDisposed;
            }
        }

		public ServerConnection(TcpClient client, int id, Listener listener)
		{
			this._client = client;
			this._id = id;
			this._listener = listener;
		}

		public async Task Start()
		{
			this._client.ReceiveBufferSize = 65536;
            this._client.SendBufferSize = 65536;
            this._listener.AddConnection(this);
			try
			{
				this._stream = new NegotiateStream(this._client.GetStream());
                ExtendedProtectionPolicy policy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);
                NetworkCredential nc = new NetworkCredential();
    			await this._stream.AuthenticateAsServerAsync(nc, ProtectionLevel.EncryptAndSign, TokenImpersonationLevel.Identification);
                await this.Read();
            }
			catch (AuthenticationException ex)
			{
				Console.WriteLine(ex.ToString());
			}
			catch (ObjectDisposedException ex)
			{
				Console.WriteLine(ex.ToString());
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                this.Dispose();
            }
		}

		public async Task Read()
		{
			if (!ExecutionContext.IsFlowSuppressed())
            {
			ExecutionContext.SuppressFlow();
            }

            while (true)
			{
				TcpMessage requestMessage = await TcpMessage.ReadMessage(this._stream);
                if (requestMessage == null)
                {
                    return;
                }

                await ProcessMessage(requestMessage);
			}
		}

        private async Task ProcessMessage(TcpMessage requestMessage)
		{
            if (!ExecutionContext.IsFlowSuppressed())
            {
            ExecutionContext.SuppressFlow();
            }

            byte[] result = this._listener.Service.ExecuteProcess(requestMessage.Data);

            TcpMessage responseMessage = TcpMessage.CreateMessage(result);
			byte[] responseBuffer = responseMessage.GetBufferedMessage();
            await this._stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
        }

		public void Dispose()
		{
			this.Dispose(true);
		}

		public void Dispose(bool removeFromCollection)
		{
            this._isDisposed = true;
            if (this._client != null)
			{
                //SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                //e.DisconnectReuseSocket = false;
                //e.Completed += this.e_Completed;
                //if (!this._client.Client.DisconnectAsync(e))
                //{
                //    this.e_Completed(this._client.Client, e);
                //}
                //this._client.Client.Disconnect(false);
                //this._client.Client.Close(0);
                //this._client.Close();
                this._client.GetStream().Close();
                this._stream.Close();
                this._client.Close();
                this._listener.RemoveConnection(this._id);
			}
		}

        void e_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= this.e_Completed;
            this._client.Client.Close(0);
        }
	}
}
