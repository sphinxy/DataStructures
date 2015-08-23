using System;
using DataStructures;
using FunctionalTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionalTests
{
    [TestClass]
    public class SkipListTests : CollectionTests<SkipList<int>>
    {
        protected internal override SkipList<int> GetCollection()
        {
            return new SkipList<int>();
        }

        [TestMethod]
        public void Peek()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);
            AssertEx.Throws<InvalidOperationException>(() => target.Peek());

            target.Add(1);
            Assert.AreEqual(1, target.Peek());
            Assert.AreEqual(1, target.Count);

            target.Add(2);
            Assert.AreEqual(1, target.Peek());
            Assert.AreEqual(2, target.Count);


            target.Add(0);
            Assert.AreEqual(0, target.Peek());
            Assert.AreEqual(3, target.Count);
        }

        [TestMethod]
        public void GetFirst()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);
            AssertEx.Throws<InvalidOperationException>(() => target.GetFirst());

            target.Add(1);
            Assert.AreEqual(1, target.GetFirst());
            Assert.AreEqual(1, target.Count);

            target.Add(2);
            Assert.AreEqual(1, target.GetFirst());
            Assert.AreEqual(2, target.Count);


            target.Add(0);
            Assert.AreEqual(0, target.GetFirst());
            Assert.AreEqual(3, target.Count);
        }

        [TestMethod]
        public void GetLast()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            AssertEx.Throws<InvalidOperationException>(() => target.GetLast());

            target.Add(1);
            Assert.AreEqual(1, target.GetLast());
            Assert.AreEqual(1, target.Count);

            target.Add(2);
            Assert.AreEqual(2, target.GetLast());
            Assert.AreEqual(2, target.Count);

            target.Add(0);
            Assert.AreEqual(2, target.GetLast());
            Assert.AreEqual(3, target.Count);

            target.Add(3);
            Assert.AreEqual(3, target.GetLast());
            Assert.AreEqual(4, target.Count);
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
            Assert.AreEqual(0, target.Take());
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(1, target.Take());
            Assert.AreEqual(1, target.Count);
            target.Add(3);
            Assert.AreEqual(2, target.Take());
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(3, target.Take());
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void TakeLast()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            AssertEx.Throws<InvalidOperationException>(() => target.TakeLast());

            target.Add(1);
            Assert.AreEqual(1, target.TakeLast());
            Assert.AreEqual(0, target.Count);

            target.Add(2);
            Assert.AreEqual(2, target.TakeLast());
            Assert.AreEqual(0, target.Count);

            target.Add(0);
            target.Add(1);
            target.Add(2);
            Assert.AreEqual(2, target.TakeLast());
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(1, target.TakeLast());
            Assert.AreEqual(1, target.Count);
            target.Add(3);
            Assert.AreEqual(3, target.TakeLast());
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(0, target.TakeLast());
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void Floor()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            Assert.AreEqual(0, target.Floor(1));
            Assert.AreEqual(0, target.Floor(2));

            target.Add(1);
            Assert.AreEqual(1, target.Floor(1));
            Assert.AreEqual(1, target.Floor(2));

            target.Add(3);
            Assert.AreEqual(1, target.Floor(1));
            Assert.AreEqual(1, target.Floor(2));
            Assert.AreEqual(3, target.Floor(3));
            Assert.AreEqual(3, target.Floor(5));

            target.Add(5);
            Assert.AreEqual(1, target.Floor(1));
            Assert.AreEqual(1, target.Floor(2));
            Assert.AreEqual(3, target.Floor(3));
            Assert.AreEqual(5, target.Floor(5));
            Assert.AreEqual(5, target.Floor(7));

            target.Add(2);
            Assert.AreEqual(1, target.Floor(1));
            Assert.AreEqual(2, target.Floor(2));
            Assert.AreEqual(3, target.Floor(3));
            Assert.AreEqual(5, target.Floor(5));
            Assert.AreEqual(5, target.Floor(7));

            target.Remove(1);
            Assert.AreEqual(0, target.Floor(1));
            Assert.AreEqual(2, target.Floor(2));
            Assert.AreEqual(3, target.Floor(3));
            Assert.AreEqual(5, target.Floor(5));
            Assert.AreEqual(5, target.Floor(7));

            target.Remove(3);
            Assert.AreEqual(0, target.Floor(1));
            Assert.AreEqual(2, target.Floor(2));
            Assert.AreEqual(2, target.Floor(3));
            Assert.AreEqual(5, target.Floor(5));
            Assert.AreEqual(5, target.Floor(7));
        }

        [TestMethod]
        public void Ceiling()
        {
            var target = GetCollection();
            Assert.AreEqual(0, target.Count);

            Assert.AreEqual(0, target.Ceiling(1));
            Assert.AreEqual(0, target.Ceiling(2));

            target.Add(1);
            Assert.AreEqual(1, target.Ceiling(0));
            Assert.AreEqual(1, target.Ceiling(1));
            Assert.AreEqual(0, target.Ceiling(2));

            target.Add(3);
            Assert.AreEqual(1, target.Ceiling(0));
            Assert.AreEqual(1, target.Ceiling(1));
            Assert.AreEqual(3, target.Ceiling(2));
            Assert.AreEqual(3, target.Ceiling(3));
            Assert.AreEqual(0, target.Ceiling(5));

            target.Add(5);
            Assert.AreEqual(1, target.Ceiling(1));
            Assert.AreEqual(3, target.Ceiling(2));
            Assert.AreEqual(3, target.Ceiling(3));
            Assert.AreEqual(5, target.Ceiling(4));
            Assert.AreEqual(5, target.Ceiling(5));
            Assert.AreEqual(0, target.Ceiling(7));

            target.Add(7);
            Assert.AreEqual(1, target.Ceiling(1));
            Assert.AreEqual(3, target.Ceiling(2));
            Assert.AreEqual(3, target.Ceiling(3));
            Assert.AreEqual(5, target.Ceiling(5));
            Assert.AreEqual(7, target.Ceiling(6));
            Assert.AreEqual(7, target.Ceiling(7));
            Assert.AreEqual(0, target.Ceiling(8));

            target.Remove(1);
            Assert.AreEqual(3, target.Ceiling(1));
            Assert.AreEqual(3, target.Ceiling(2));
            Assert.AreEqual(3, target.Ceiling(3));
            Assert.AreEqual(5, target.Ceiling(5));
            Assert.AreEqual(7, target.Ceiling(7));
            Assert.AreEqual(0, target.Ceiling(8));

            target.Remove(3);
            Assert.AreEqual(5, target.Ceiling(1));
            Assert.AreEqual(5, target.Ceiling(2));
            Assert.AreEqual(5, target.Ceiling(3));
            Assert.AreEqual(5, target.Ceiling(5));
            Assert.AreEqual(7, target.Ceiling(7));
            Assert.AreEqual(0, target.Ceiling(8));

            target.Remove(5);
            for (var i = 0; i < 8; i++)
            {
                Assert.AreEqual(7, target.Ceiling(i));
            }
            Assert.AreEqual(0, target.Ceiling(8));
        }
    }
}
