using System;
using JetBrains.Annotations;
using Maxima.Common.BufferManagement;

namespace Maxima.Tcp
{
    public sealed class MessageTransportFactory
    {
        private readonly ListenerConfiguration _listenerConfiguration;
        private readonly ProcessingConfiguration _processingConfiguration;
        private readonly BufferManagerFactory _bufferManagerFactory;

        public MessageTransportFactory(ListenerConfiguration listenerConfiguration, ProcessingConfiguration processingConfiguration, BufferManagerFactory bufferManagerFactory)
        {
            if (listenerConfiguration == null)
            {
                throw new ArgumentNullException("listenerConfiguration");
            }
            if (processingConfiguration == null)
            {
                throw new ArgumentNullException("processingConfiguration");
            }
            if (bufferManagerFactory == null)
            {
                throw new ArgumentNullException("bufferManagerFactory");
            }
            _listenerConfiguration = listenerConfiguration;
            _processingConfiguration = processingConfiguration;
            _bufferManagerFactory = bufferManagerFactory;
        }

        public IMessageTransport GetMessageTransport([NotNull] Listener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            return _listenerConfiguration.Transport == MessageTransportKind.AsyncApi
                ? (IMessageTransport)new AsyncSocketMessageTransport(listener, _processingConfiguration, _bufferManagerFactory)
                : new NetworkStreamMessageTransport(_processingConfiguration, _bufferManagerFactory);
        }
    }
}