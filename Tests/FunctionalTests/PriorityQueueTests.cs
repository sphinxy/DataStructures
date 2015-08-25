using System;
using System.Collections.Generic;
using DataStructures;
using FunctionalTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    [TestClass]
    public class PriorityQueueTests : CollectionTests<PriorityQueue<int>>
    {
        protected internal override PriorityQueue<int> GetCollection()
        {
            return new PriorityQueue<int>();
        }

        [TestMethod]
        public override void CopyTo()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            const int count = 10;
            for (var i = 0; i < count; i++)
            {
                target.Add(i);
            }

            AssertEx.Throws<ArgumentNullException>(() => target.CopyTo(null, 0));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => target.CopyTo(new int[count], -1));
            AssertEx.Throws<ArgumentException>(() => target.CopyTo(new int[1], 0));

            var result = new int[count];
            target.CopyTo(result, 0);

            // Priority queue is max-based so greater items comes first
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(count - i - 1, result[i]);
            }

            result = new int[count + 1];
            result[0] = -1;
            target.CopyTo(result, 1);
            Assert.AreEqual(-1, result[0]);
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(count - i - 1, result[i + 1]);
            }
        }

        [TestMethod]
        public override void GetEnumerator()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            const int count = 10;
            for (var i = 0; i < count; i++)
            {
                target.Add(i);
            }

            var enumerator = target.GetEnumerator();
            int x = 9;
            while (enumerator.MoveNext())
            {
                Assert.AreEqual(x, enumerator.Current);
                x--;
            }
            Assert.AreEqual(-1, x);
        }

        [TestMethod]
        public void TakeSimple()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            AssertEx.Throws<InvalidOperationException>(() => target.Take());

            target.Add(1);
            Assert.AreEqual(1, target.Take());
            Assert.AreEqual(0, target.Count);

            target.Add(2);
            Assert.AreEqual(2, target.Take());
            Assert.AreEqual(0, target.Count);

            target.Add(0);
            target.Add(1);
            target.Add(2);
            Assert.AreEqual(2, target.Take());
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(1, target.Take());
            Assert.AreEqual(1, target.Count);
            target.Add(3);
            Assert.AreEqual(3, target.Take());
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(0, target.Take());
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void TakeRandomized()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            var store = new SortedSet<int>();
            var random = new Random();
            const int count = 1000;
            for (var i = 0; i < count; i++)
            {
                var item = random.Next(2 * count);
                while (store.Contains(item))
                {
                    item = random.Next(2 * count);
                }

                if (target.Count < 10)
                {
                    target.Add(item);
                    store.Add(item);
                }
                else
                {
                    Assert.AreEqual(store.Max, target.Take());
                    store.Remove(store.Max);
                }
            }
        }

        // TODO write a test which will add, remove and take randomly and after each step check that heap principles stand

        [TestMethod]
        public void PeekSimple()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            AssertEx.Throws<InvalidOperationException>(() => target.Peek());

            target.Add(1);
            Assert.AreEqual(1, target.Peek());
            Assert.AreEqual(1, target.Count);

            target.Add(2);
            Assert.AreEqual(2, target.Peek());
            Assert.AreEqual(2, target.Count);

            target.Add(0);
            Assert.AreEqual(2, target.Peek());
            Assert.AreEqual(3, target.Count);

            target.Take();
            Assert.AreEqual(1, target.Peek());

            target.Add(3);
            Assert.AreEqual(3, target.Peek());
            Assert.AreEqual(3, target.Count);
        }
    }
}
