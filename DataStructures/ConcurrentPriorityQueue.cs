using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DataStructures
{
    /// <summary>
    /// Thread-safe heap-based resizable max-priority queue.
    /// Elements with high priority are served before elements with low priority.
    /// Priority is defined by comparing elements, so to separate priority from value use
    /// KeyValuePair or a custom class and provide corresponding Comparer.
    /// </summary>
    /// <typeparam name="T">Any comparable type, either through a specified Comparer or implementing IComparable&lt;<typeparamref name="T"/>&gt;</typeparam>
    public class ConcurrentPriorityQueue<T> : PriorityQueue<T>, IProducerConsumerCollection<T>, IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Create concurrent max-priority queue with default capacity of 10.
        /// </summary>
        /// <param name="comparer">Custom comparer to compare elements. If omitted - default will be used.</param>
        public ConcurrentPriorityQueue(IComparer<T> comparer = null) : base(comparer)
        {
            
        }

        /// <summary>
        /// Create concurrent max-priority queue with provided capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        /// <param name="comparer">Custom comparer to compare elements. If omitted - default will be used.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws <see cref="ArgumentOutOfRangeException"/> when capacity is less than or equal to zero.</exception>
        /// <exception cref="ArgumentException">Throws <see cref="ArgumentException"/> when comparer is null and <typeparamref name="T"/> is not comparable</exception>
        public ConcurrentPriorityQueue(int capacity, IComparer<T> comparer = null) : base(capacity, comparer)
        {
            
        }

        public override void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                base.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public override void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                base.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return base.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void CopyTo(T[] array, int arrayIndex)
        {
            var hasLock = _lock.IsReadLockHeld;
            if(!hasLock) _lock.EnterReadLock();
            try
            {
                base.CopyTo(array, arrayIndex);
            }
            finally
            {
               if (!hasLock) _lock.ExitReadLock();
            }
        }

        public void CopyTo(Array array, int index)
        {
            _lock.EnterReadLock();
            try
            {
                base.CopyTo((T[])array, index);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public T[] ToArray()
        {
            _lock.EnterReadLock();
            try
            {
                var array = new T[Count];
                CopyTo(array, 0);
                return array;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return base.GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override T Peek()
        {
            _lock.EnterReadLock();
            try
            {
                return base.Peek();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override bool Remove(T item)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var index = GetItemIndex(item);
                switch (index)
                {
                    case -1:
                        return false;
                    case 0:
                        // Take does locking on it's own
                        Take();
                        break;
                    default:
                        _lock.EnterWriteLock();
                        try
                        {
                            // provide a 1-based index of the item
                            RemoveAt(index + 1, shift: -1);
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }
                        break;
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            return true;
        }

        public override T Take()
        {
            _lock.EnterWriteLock();
            try
            {
                return base.Take();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryTake(out T item)
        {
            item = default(T);
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (Count == 0) return false;
                item = Take();
                return true;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException(""); }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public void Dispose()
        {
            ((IDisposable) _lock).Dispose();
        }
    }
}
