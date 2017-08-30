using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Maxima.Tcp;

namespace Maxima.Server.Tcp
{
    public sealed class ConnectionHolder
    {
        private readonly ConcurrentDictionary<int, ServerConnection> _connections;

        public ConnectionHolder(int capacity)
        {
            this._connections = new ConcurrentDictionary<int, ServerConnection>(Environment.ProcessorCount, capacity);
        }

        public ICollection<ServerConnection> Connections
        {
            get { return this._connections.Values; }
        }

        public void AddConnection(ServerConnection connection)
        {
            this._connections.TryAdd(connection.Id, connection);
        }

        public void RemoveConnection(ServerConnection connection)
        {
            ServerConnection output;
            this._connections.TryRemove(connection.Id, out output);
        }

        public void Clean()
        {
            foreach (KeyValuePair<int, ServerConnection> serverConnection in _connections)
            {
                ServerConnection connection = serverConnection.Value;
                if (connection != null && !connection.IsDisposed)
                {
                    try
                    {
                        connection.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
            }

            this._connections.Clear();
        }
    }
}