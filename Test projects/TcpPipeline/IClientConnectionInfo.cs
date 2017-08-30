using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace Maxima.Tcp
{
    public interface IClientConnectionInfo
    {
        [NotNull]
        Stream Stream { get; }

        [NotNull]
        Socket Socket { get; }

        [NotNull]
        Stopwatch TimeFromAccept { get; }
    }
}