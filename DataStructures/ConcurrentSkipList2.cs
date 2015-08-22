using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DataStructures
{
    public class ConcurrentSkipList<T> : ICollection<T>, IProducerConsumerCollection<T>, IDisposable
        where T : IComparable<T>
    {
        private const byte MAX_HEIGHT = 32;
        internal const byte HEIGHT_STEP = 4;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Random _random = new Random();

        internal readonly Node _head;
        internal readonly Node _tail;
        private readonly IComparer<T> _comparer;

        private int _count;
        internal byte _height;
        private Node _lastFoundNode;

        public ConcurrentSkipList(IComparer<T> comparer = null)
        {
            _comparer = comparer ?? Comparer<T>.Default;
            _random = new Random();

            _head = new Node(default(T), HEIGHT_STEP);
            _tail = new Node(default(T), HEIGHT_STEP);
            Reset();
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _head.SetHeight(HEIGHT_STEP);
                _tail.SetHeight(HEIGHT_STEP);
                Reset();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            Node node;
            _lock.EnterReadLock();
            try
            {
                node = FindNode(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterWriteLock();
            try
            {
                _lastFoundNode = node;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return CompareNode(node, item) == 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo((Array) array, arrayIndex);
        }

        public int Count
        {
            get { return _count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(T item)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                Node prev = FindNode(item);

                _lock.EnterWriteLock();
                try
                {
                    _lastFoundNode = AddNewNode(item, prev);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                Node node = FindNode(item);
                if (CompareNode(node, item) != 0) return false;

                _lock.EnterWriteLock();
                try
                {
                    DeleteNode(node);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var items = new LinkedList<T>();
            Node node = _head.GetNext(0);
            while (node != _tail)
            {
                items.AddLast(node.Item);
                node = node.GetNext(0);
            }
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            ((IDisposable) _lock).Dispose();
        }

        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public bool TryTake(out T item)
        {
            item = default(T);
            Node node;
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (Count == 0) return false;

                node = _head.GetNext(0);
                _lock.EnterWriteLock();
                try
                {
                    DeleteNode(node);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            item = node.Item;
            return true;
        }

        public T[] ToArray()
        {
            T[] array;
            _lock.EnterReadLock();
            try
            {
                array = new T[Count];
                CopyTo(array, 0);
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return array;
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex");

            _lock.EnterReadLock();
            try
            {
                if (array.Length - arrayIndex < Count)
                    throw new ArgumentException("Insufficient space in destination array.");

                Node node = _head.GetNext(0);
                for (int i = arrayIndex; i < arrayIndex + Count; i++)
                {
                    array.SetValue(node.Item, i);
                    node = node.GetNext(0);
                }
            }
            finally
            {
                _lock.ExitReadLock();
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

        private void Reset()
        {
            for (int i = 0; i < _head.Height; i++)
            {
                _head.SetNext(i, _tail);
                _tail.SetPrev(i, _head);
            }

            _count = 0;
            _height = 1;
            _lastFoundNode = _head;
        }

        private Node FindNode(T key)
        {
            int level = _height - 1;
            Node node = _head;
            Node lastFound = _lastFoundNode;

            int cmp;
            if (lastFound != _head)
            {
                if ((cmp = CompareNode(lastFound, key)) == 0) return lastFound;
                if (cmp < 0)
                {
                    node = lastFound;
                    level = lastFound.Height - 1;
                }
            }

            while (level >= 0)
            {
                Node next = node.GetNext(level);
                while ((cmp = CompareNode(next, key)) < 0)
                {
                    node = next;
                    next = next.GetNext(level);
                }
                if (cmp == 0)
                {
                    node = next;
                    break;
                }

                level--;
            }

            return node;
        }

        private Node AddNewNode(T item, Node prev)
        {
            Node next = prev.GetNext(0);
            byte newNodeHeight = GetNewNodeHeight();

            var newNode = new Node(item, newNodeHeight);
            InsertNode(newNode, newNodeHeight, prev, next);
            _count++;
            return newNode;
        }

        private byte GetNewNodeHeight()
        {
            byte maxNodeHeight = _height;
            if (maxNodeHeight < MAX_HEIGHT && (1 << maxNodeHeight) < _count)
            {
                maxNodeHeight++;
            }
            var nodeHeight = (byte) (1 + _random.Next(maxNodeHeight));
            if (nodeHeight > _height)
            {
                _height = nodeHeight;
                if (_head.Height < _height)
                {
                    maxNodeHeight = (byte) _head.Height;
                    _head.SetHeight(maxNodeHeight + HEIGHT_STEP);
                    _tail.SetHeight(maxNodeHeight + HEIGHT_STEP);
                    while (maxNodeHeight < _head.Height)
                    {
                        _head.SetNext(maxNodeHeight, _tail);
                        _tail.SetPrev(maxNodeHeight, _head);
                        maxNodeHeight++;
                    }
                }
            }
            return nodeHeight;
        }

        private void InsertNode(Node newNode, byte height, Node prev, Node next)
        {
            for (int i = 0; i < height; i++)
            {
                while (prev.Height <= i) prev = prev.GetPrev(i - 1);
                while (next.Height <= i) next = next.GetNext(i - 1);
                newNode.SetPrev(i, prev);
                newNode.SetNext(i, next);

                prev.SetNext(i, newNode);
                next.SetPrev(i, newNode);
            }
        }

        private void DeleteNode(Node node)
        {
            for (byte i = 0; i < node.Height; i++)
            {
                Node prev = node.GetPrev(i);
                Node next = node.GetNext(i);

                while (prev.Height <= i) prev = prev.GetPrev(i - 1);
                while (next.Height <= i) next = next.GetNext(i - 1);

                prev.SetNext(i, next);
                next.SetPrev(i, prev);
            }

            _count--;

            if (_height > 1 && (1 << _height) > _count)
            {
                _height--;
            }
        }

        private int CompareNode(Node node, T key)
        {
            if (node == _head) return -1;
            if (node == _tail) return 1;

            return _comparer.Compare(node.Item, key);
        }

        [DebuggerDisplay("Node [{Item}] ({Height})")]
        internal class Node
        {
            private readonly T _item;
            private Node[] _next;
            private Node[] _prev;

            protected internal Node(T item, byte height)
            {
                _item = item;
                _next = new Node[height];
                _prev = new Node[height];
            }

            protected internal T Item
            {
                get { return _item; }
            }

            protected internal int Height
            {
                get { return _next.Length; }
            }

            protected internal Node GetNext(int level)
            {
                return _next[level];
            }

            protected internal void SetNext(int level, Node node)
            {
                _next[level] = node;
            }

            protected internal void SetPrev(int level, Node node)
            {
                _prev[level] = node;
            }

            protected internal Node GetPrev(int level)
            {
                return _prev[level];
            }

            protected internal void SetHeight(int height)
            {
                var newNext = new Node[height];
                var newPrev = new Node[height];
                var count = Math.Min(_next.Length, height);
                for (var i = 0; i < count; i++)
                {
                    newNext[i] = _next[i];
                    newPrev[i] = _prev[i];
                }
                _next = newNext;
                _prev = newPrev;
            }
        }
    }
}