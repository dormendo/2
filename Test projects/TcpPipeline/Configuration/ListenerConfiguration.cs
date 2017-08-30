using System.Configuration;
using System.Net;
using Maxima.Server;
using Maxima.Server.Configuration;

namespace Maxima.Tcp
{
    /// <summary>
    /// Конфигурация Socket сервера.
    /// </summary>
    public sealed class ListenerConfiguration
    {
        /// <summary>
        /// Размер буфера на прием данных слушающего сокета.
        /// </summary>
        public int ListenSocketReceiveBufferSize { get; private set; }

        /// <summary>
        /// Размер буфера на отправку данных слушающего сокета.
        /// </summary>
        public int ListenSocketSendBufferSize { get; private set; }

        /// <summary>
        /// Размер буфер на прием данных клиентского сокета.
        /// </summary>
        public int ClientSocketReceiveBufferSize { get; private set; }

        /// <summary>
        /// Размер буфер на отправку данных клиентского сокета.
        /// </summary>
        public int ClientSocketSendBufferSize { get; private set; }

        /// <summary>
        /// Максимальное количество одновременных подключений.
        /// </summary>
        /// <remarks>В данный момент нет *явного* отключения или неприема клиентских подключений при привышении данного лимита.</remarks>
        public int ConnectionCapacity { get; private set; }

        /// <summary>
        /// Backlog одного слушающего сокета.
        /// </summary>
        public int ListenBacklog { get; private set; }

        /// <summary>
        /// Исходное число подключений паралельно принимаемых сервером.
        /// </summary>
        public int InitialAcceptsCount { get; private set; }

        /// <summary>
        /// Выбранное для работы сервера socket api.
        /// </summary>
        public MessageTransportKind Transport { get; private set; }

        /// <summary>
        /// Точка подключения сервера (адрес + порт).
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        private ListenerConfiguration()
        {
        }

        public static ListenerConfiguration LoadConfigurationFromAppConfig()
        {
            ServerConfigurationSection serverConfigurationSection = (ServerConfigurationSection)ConfigurationManager.GetSection("server");
            var listenerConfig = serverConfigurationSection.Listener;

            MessageTransportKind messageTransport = listenerConfig.MessageTransport == "AsyncApi"
                ? MessageTransportKind.AsyncApi
                : MessageTransportKind.NetworkStream;

            ListenerConfiguration listenerConfiguration = new ListenerConfiguration
            {
                Transport = messageTransport,
                EndPoint = new IPEndPoint(IPAddress.Any, Glvar.PORT_MAIN),
                ConnectionCapacity = listenerConfig.ConnectionCapacity,
                InitialAcceptsCount = listenerConfig.InitialAcceptsCount,
                ListenBacklog = listenerConfig.ListenBacklog,
                ListenSocketReceiveBufferSize = listenerConfig.ListenSocketReceiveBufferSize,
                ListenSocketSendBufferSize = listenerConfig.ListenSocketSendBufferSize,
                ClientSocketReceiveBufferSize = listenerConfig.ClientSocketReceiveBufferSize,
                ClientSocketSendBufferSize = listenerConfig.ClientSocketSendBufferSize
            };
            return listenerConfiguration;
        }
    }

    public enum MessageTransportKind
    {
        NetworkStream,
        AsyncApi
    }
}
