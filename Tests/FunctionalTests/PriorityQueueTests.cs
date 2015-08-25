using System;
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
        public void Take()
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
    }
}
