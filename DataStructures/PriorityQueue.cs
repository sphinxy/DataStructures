using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructures
{
    public class PriorityQueue<T> : ICollection<T> where T : IComparable<T>
    {
        private readonly IComparer<T> _comparer;
        private T[] _items;
        private const int DEFAULT_CAPACITY = 10;
        private const int SHRINK_RATIO = 4;
        private const int RESIZE_FACTOR = 2;

        private int _shrinkBound;

        // ReSharper disable once StaticFieldInGenericType
        private static readonly InvalidOperationException EmptyCollectionException = new InvalidOperationException("Collection is empty.");

        public PriorityQueue(IComparer<T> comparer = null) : this(DEFAULT_CAPACITY, comparer) { }

        public PriorityQueue(int capacity, IComparer<T> comparer = null)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException("capacity", "Expected capacity greater than zero.");

            _comparer = comparer ?? Comparer<T>.Default;
            _shrinkBound =capacity / SHRINK_RATIO;

            // for simplicity of calculations first element is at position 1, so one spot is always empty
            _items = new T[capacity + 1];
        }

        public int Capacity { get { return _items.Length - 1; } }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (Count == Capacity) GrowCapacity();

            _items[++Count] = item;

            Sift(Count);                // move item "up" until heap principles are not met
        }

        public virtual T Take()
        {
            if (Count == 0) throw EmptyCollectionException;

            var item = _items[1];       // first element at 1
            Swap(1, Count);             // last element at Count
            _items[Count] = default(T); // release hold on the object
            Count--;                    // update count after the element is really gone but before Sink
            Sink(1);                    // move item "down" while heap principles are not met

            if (Count <= _shrinkBound && Count > DEFAULT_CAPACITY)
            {
                ShrinkCapacity();
            }

            return item;
        }

        public void Clear()
        {
            _items = new T[1 + DEFAULT_CAPACITY];
            Count = 0;
        }

        public bool Contains(T item)
        {
            return GetItemIndex(item) > 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            int index = GetItemIndex(item);
            switch (index)
            {
                case 0:
                    return false;
                case 1:
                    Take();
                    break;
                default:
                    Swap(index, Count);         // last element at Count
                    _items[Count] = default(T); // release hold on the object
                    Count--;                    // update count after the element is really gone but before Sink
                    int parent = index / 2;     // get parent
                    // if new item at index is greater than it's parent then sift it up, else sink it down
                    if (GreaterOrEqual(_items[index], _items[parent]))
                    {
                        Sift(index);
                    }
                    else
                    {
                        Sink(index);
                    }
                    break;
            }

            return true;

        }

        public int Count { get; private set; }
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Returns index of the first occurrence of the given item or 0.
        /// </summary>
        private int GetItemIndex(T item)
        {
            for (int i = 1; i <= Count; i++)
            {
                if (_comparer.Compare(_items[i], item) == 0) return i;
            }
            return 0;
        }

        private bool GreaterOrEqual(T i, T j)
        {
            return _comparer.Compare(i, j) >= 0;
        }

        /// <summary>
        /// Moves the item with given index "down" the heap while heap principles are not met.
        /// </summary>
        private void Sink(int i)
        {
            while (true)
            {
                int leftChildIndex = 2 * i;
                int rightChildIndex = 2 * i + 1;
                if (leftChildIndex > Count) return; // reached last item

                var item = _items[i];
                var left = _items[leftChildIndex];
                var hasRight = rightChildIndex <= Count; // _items are 1 based
                var right = default(T);
                if (hasRight) right = _items[rightChildIndex];

                // if item is greater than children - exit
                if (GreaterOrEqual(item, left) && (!hasRight || GreaterOrEqual(item, right))) return;

                // else exchange with greater of children
                int greaterChildIndex = !hasRight || GreaterOrEqual(left, right) ? leftChildIndex : rightChildIndex;
                Swap(i, greaterChildIndex);

                // continue at new position
                i = greaterChildIndex;
            }
        }

        /// <summary>
        /// Moves the item with given index "up" the heap while heap principles are not met.
        /// </summary>
        private void Sift(int i)
        {
            while (true)
            {
                if (i <= 1) return;         // reached root
                int parent = i / 2;         // get parent

                // if root is greater or equal - exit
                if (GreaterOrEqual(_items[parent], _items[i])) return;

                Swap(parent, i);
                i = parent;
            }
        }

        private void Swap(int i, int j)
        {
            var tmp = _items[i];
            _items[i] = _items[j];
            _items[j] = tmp;
        }

        private void GrowCapacity()
        {
            int newCapacity = Capacity * RESIZE_FACTOR;
            Array.Resize(ref _items, newCapacity + 1);  // first element is at position 1
            _shrinkBound = newCapacity / SHRINK_RATIO;
        }

        private void ShrinkCapacity()
        {
            int newCapacity = Capacity / RESIZE_FACTOR;
            Array.Resize(ref _items, newCapacity + 1);  // first element is at position 1
            _shrinkBound = newCapacity / SHRINK_RATIO;
        }
    }
}
