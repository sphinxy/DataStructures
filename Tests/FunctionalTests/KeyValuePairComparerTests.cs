using System.Collections.Generic;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    [TestClass]
    public class KeyValuePairComparerTests
    {
        [TestMethod]
        public void CompareEqual()
        {
            IComparer<KeyValuePair<int, string>> comparer = new KeyValuePairComparer<int, string>();
            KeyValuePair<int, string> a = new KeyValuePair<int, string>(1, null);
            KeyValuePair<int, string> b = new KeyValuePair<int, string>(1, "asdf");
            Assert.AreEqual(0, comparer.Compare(a, b));
        }

        [TestMethod]
        public void CompareGreater()
        {
            IComparer<KeyValuePair<int, string>> comparer = new KeyValuePairComparer<int, string>();
            KeyValuePair<int, string> a = new KeyValuePair<int, string>(2, null);
            KeyValuePair<int, string> b = new KeyValuePair<int, string>(1, "asdf");
            Assert.AreEqual(1, comparer.Compare(a, b));
        }

        [TestMethod]
        public void CompareNullSecondKey()
        {
            // All instances are greater than null (MSDN)
            IComparer<KeyValuePair<string, string>> comparer = new KeyValuePairComparer<string, string>();
            KeyValuePair<string, string> a = new KeyValuePair<string, string>("a", null);
            KeyValuePair<string, string> b = new KeyValuePair<string, string>(null, "asdf");
            Assert.AreEqual(1, comparer.Compare(a, b));
        }

        [TestMethod]
        public void CompareNullFirstKey()
        {
            // All instances are greater than null (MSDN)
            IComparer<KeyValuePair<string, string>> comparer = new KeyValuePairComparer<string, string>();
            KeyValuePair<string, string> a = new KeyValuePair<string, string>(null, null);
            KeyValuePair<string, string> b = new KeyValuePair<string, string>("a", "asdf");
            Assert.AreEqual(-1, comparer.Compare(a, b));
        }

        [TestMethod]
        public void CompareNullBothKeys()
        {
            IComparer<KeyValuePair<string, string>> comparer = new KeyValuePairComparer<string, string>();
            KeyValuePair<string, string> a = new KeyValuePair<string, string>(null, null);
            KeyValuePair<string, string> b = new KeyValuePair<string, string>(null, "asdf");
            Assert.AreEqual(0, comparer.Compare(a, b));
        }

        [TestMethod]
        public void CompareUsingCustomKeyComparer()
        {
            // Test that the custom comparer for the keys is used if supplied
            IComparer<int> keyComparer = new ReverseIntComparer();
            IComparer<KeyValuePair<int, string>> comparer = new KeyValuePairComparer<int, string>(keyComparer);
            KeyValuePair<int, string> a = new KeyValuePair<int, string>(2, null);
            KeyValuePair<int, string> b = new KeyValuePair<int, string>(1, "asdf");
            Assert.AreEqual(-1, comparer.Compare(a, b));
        }

        #region Custom Comparers

        private class ReverseIntComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return -Comparer<int>.Default.Compare(x, y);
            }
        }

        #endregion
    }
}
