using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace TestTcp.Server.Sockets
{
    /// <summary>
    /// Represents a collection of resusable SocketAsyncEventArgs objects.  
    /// </summary>
    class SocketAsyncEventArgsPool
    {
        ConcurrentStack<SocketAsyncEventArgs> m_pool;
        
        /// <summary>
        /// Initializes the object pool to the specified size
        /// </summary>
        /// <param name="capacity">The maximum number of SocketAsyncEventArgs objects the pool can hold</param>
        public SocketAsyncEventArgsPool(int capacity)
        {
            m_pool = new ConcurrentStack<SocketAsyncEventArgs>();
        }

        /// <summary>
        /// Add a SocketAsyncEventArg instance to the pool
        /// </summary>
        /// <param name="item">The SocketAsyncEventArgs instance to add to the pool</param>
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
            m_pool.Push(item);
        }

        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool
        /// </summary>
        /// <returns>The object removed from the pool</returns>
        public SocketAsyncEventArgs Pop()
        {
            SocketAsyncEventArgs e;
            m_pool.TryPop(out e);
            return e;
        }

        /// <summary>
        /// The number of SocketAsyncEventArgs instances in the pool
        /// </summary>
        public int Count
        {
            get { return m_pool.Count; }
        }

    }
}
