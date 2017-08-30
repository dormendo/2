using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Maxima.Tcp
{
    public interface IMessageTransport
    {
        [NotNull]
        Task<TcpMessage> ReadAsync([NotNull] IClientConnectionInfo connectionInfo);

        [NotNull]
        Task WriteResponseStreamAsync([NotNull] IClientConnectionInfo connectionInfo, [NotNull] TcpMessageHeader header, [NotNull] Stream input);

        [NotNull]
        Task WriteResponseMessageAsync([NotNull] IClientConnectionInfo connectionInfo, [NotNull] TcpMessage responseMessage);
    }
}
