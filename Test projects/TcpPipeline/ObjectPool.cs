using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Maxima.Tcp
{
    internal sealed class ObjectPool<TItem>
    {
        private readonly ConcurrentStack<TItem> _stack = new ConcurrentStack<TItem>();

        public int ElapsedItems
        {
            get { return this._stack.Count; }
        }

        [NotNull]
        public TItem Get()
        {
            TItem item;
            if (!this._stack.TryPop(out item))
            {
                throw new InvalidOperationException(string.Format("No more objects of type {0} in pool", typeof (TItem).Name));
            }
            return item;
        }

        [CanBeNull]
        public TItem TryGet()
        {
            TItem item;
            this._stack.TryPop(out item);
            return item;
        }

        public void Put([NotNull] TItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            //TODO check for double put in DEBUG
            this._stack.Push(item);
        }
    }
}