using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DataStructures
{
    public class ConcurrentSkipList2<T> : ICollection<T>, IProducerConsumerCollection<T> where T : IComparable<T>
    {
        private const byte MAX_HEIGHT = 32;
        internal const byte HEIGHT_STEP = 4;

        internal readonly Node _head;
        internal readonly Node _tail;
        internal byte _height;
        private int _count;
        private readonly IComparer<T> _comparer;
        private readonly Random _random;
        private Node _lastFoundNode;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public ConcurrentSkipList2(IComparer<T> comparer = null)
        {
            _comparer = comparer ?? Comparer<T>.Default;
            _random = new Random();
            _count = 0;
            _height = 1;
            var t = new ConcurrentStack<int>();

            _head = new Node(default(T), HEIGHT_STEP);
            _tail = new Node(default(T), HEIGHT_STEP);
            for (var i = 0; i < HEIGHT_STEP; i++)
            {
                _head.SetNext(i, _tail);
                _tail.SetPrev(i, _head);
            }
            _lastFoundNode = _head;
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                for (var i = 0; i < _head.Height; i++)
                {
                    _head.SetNext(i, _tail);
                    _tail.SetPrev(i, _head);
                }
                _count = 0;
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

            return CompareNode(node, item) == 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo((Array)array, arrayIndex);
        }

        public bool TryAdd(T item)
        {
            // TODO handle capacity
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

                var node = _head.GetNext(0);
                for (var i = arrayIndex; i < arrayIndex + Count; i++)
                {
                    array.SetValue(node.Item, arrayIndex);
                    node = node.GetNext(0);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int Count { get { return _count; } }
        public object SyncRoot { get { throw new NotSupportedException(""); } }
        public bool IsSynchronized { get { return false; } }
        public bool IsReadOnly { get { return false; } }

        public void Add(T item)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var prev = FindNode(item);

                _lock.EnterWriteLock();
                try
                {
                    AddNewNode(item, prev);
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
                var node = FindNode(item);
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

        private Node FindNode(T key)
        {
            var level = _height - 1;
            var node = _head;
            var lastFound = _lastFoundNode;
            try
            {
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
                    var next = node.GetNext(level);
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

                Interlocked.Exchange(ref _lastFoundNode, node);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR finding key {0} at level {1} in list level {2}.\n{3}", key, level, _height, ex);
            }

            return node;
        }

        private void AddNewNode(T item, Node prev)
        {
            var next = prev.GetNext(0);
            var newNodeHeight = GetNewNodeHeight();

            var newNode = new Node(item, newNodeHeight);
            InsertNode(newNode, newNodeHeight, prev, next);
            _count++;
        }

        private byte GetNewNodeHeight()
        {
            var maxNodeHeight = _height;
            if (maxNodeHeight < MAX_HEIGHT && (1 << maxNodeHeight) < _count)
            {
                maxNodeHeight++;
            }
            var nodeHeight = (byte)(1 + _random.Next(maxNodeHeight));
            if (nodeHeight > _height)
            {
                _height = nodeHeight;
                if (_head.Height < _height)
                {
                    maxNodeHeight = (byte)_head.Height;
                    _head.Grow(maxNodeHeight + HEIGHT_STEP);
                    _tail.Grow(maxNodeHeight + HEIGHT_STEP);
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
            for (var i = 0; i < height; i++)
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
                var prev = node.GetPrev(i);
                var next = node.GetNext(i);

                while (prev.Height <= i) prev = prev.GetPrev(i - 1);
                while (next.Height <= i) next = next.GetNext(i - 1);

                prev.SetNext(i, next);
                next.SetPrev(i, prev);
            }

            _lastFoundNode = _head;
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
            private Node[] _next;
            private Node[] _prev;
            private readonly T _item;

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

            internal int Height { get { return _next.Length; } }

            public Node GetNext(int level)
            {
                return _next[level];
            }

            public void SetNext(int level, Node node)
            {
                _next[level] = node;
            }

            public void SetPrev(int level, Node node)
            {
                _prev[level] = node;
            }

            public Node GetPrev(int level)
            {
                return _prev[level];
            }

            internal void Grow(int height)
            {
                var newNext = new Node[height];
                var newPrev = new Node[height];
                Array.Copy(_next, newNext, _next.Length);
                Array.Copy(_prev, newPrev, _prev.Length);
                _next = newNext;
                _prev = newPrev;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var items = new LinkedList<T>();
            var node = _head.GetNext(0);
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
    }
}
