using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestLog
{
    public class CacheQueue<T> where T : ICacheItemCleanUp, new()
    {
        private ConcurrentQueue<T> _cache = new ConcurrentQueue<T>();

        private int _count = 0;

        public CacheQueue(int initialCacheSize)
        {
            if (initialCacheSize > 0)
            {
                for (int i = 0; i < initialCacheSize; i++)
                {
                    this._cache.Enqueue(new T());
                }
            }
        }

        public void ReleaseCacheItem(T item)
        {
            item.CleanUp();
            this._cache.Enqueue(item);
        }

        public T AcquireCacheItem()
        {
            T item;
            if (!this._cache.TryDequeue(out item))
            {
                item = new T();
            }
            else
            {
                
            }

            return item;
        }

        internal int GetSize()
        {
            return this._cache.Count;
        }
    }
}
