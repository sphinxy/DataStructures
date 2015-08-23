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
