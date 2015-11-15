using System;
using System.Collections.Generic;

namespace DataStructures
{
    /// <summary>
    /// A Comparer for KeyValuePair that compares keys
    /// </summary>
    /// <typeparam name="TKey">Any comparable type</typeparam>
    /// <typeparam name="TValue">Any type</typeparam>
    public class KeyValuePairComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>> where TKey : IComparable<TKey>
    {
        private readonly IComparer<TKey> _keyComparer;

        /// <summary>
        /// Create a Comparer for KeyValuePair with an optional comparer for <typeparamref name="TKey"/>.
        /// </summary>
        /// <param name="keyComparer">Custom comparer to compare Keys. Uses default if omitted.</param>
        public KeyValuePairComparer(IComparer<TKey> keyComparer = null)
        {
            _keyComparer = keyComparer ?? Comparer<TKey>.Default;
        }

        /// <summary>
        /// Compares two KeyValuePairs and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first KeyValuePair to compare.</param>
        /// <param name="y">The second KeyValuePair to compare.</param>
        /// <returns>Value greater than zero if <paramref name="x"/> is greater than <paramref name="y"/>. Returns zero if parameters are equal, else retuns a value less than zero.</returns>
        public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            return _keyComparer.Compare(x.Key, y.Key);
        }
    }
}
