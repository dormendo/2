using System;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;

namespace TestTcp.Client
{
    public class WcfCaller : IService
    {
        #region Статическая инициализация

        private ChannelFactory<IService> _factory;

        public void AcquireChannelFactory(NetworkCredential networkCredential)
        {
            if (_factory == null)
            {
                _factory = new ChannelFactory<IService>("mainEndpoint");

                if (networkCredential != null)
                {
                    _factory.Credentials.Windows.ClientCredential = networkCredential;
                }
            }
        }

        public void ReleaseChannelFactory()
        {
            if (_factory != null)
            {
                _factory.Close();
                _factory = null;
            }
        }

        #endregion

        #region IService

        public async Task<byte[]> ExecuteWcfProcess(byte[] message)
        {
            IService proxy = null;
            IClientChannel ch = null;
            byte[] result;
            try
            {
                proxy = _factory.CreateChannel();
                ch = (IClientChannel)proxy;
                ch.Open();
                result = await proxy.ExecuteWcfProcess(message);
                ch.Close();
            }
            catch (Exception)
            {
                if (ch != null)
                {
                    if (ch.State == CommunicationState.Faulted)
                    {
                        ch.Abort();
                    }
                    else
                    {
                        ch.Close();
                    }
                }

                throw;
            }

            return result;
        }

        #endregion
    }
}
