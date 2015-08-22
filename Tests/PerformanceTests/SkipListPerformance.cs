using System;
using System.Linq;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceTests.Helpers;

namespace PerformanceTests
{
    [TestClass]
    public class SkipListPerformance
    {
        [TestMethod]
        public void SkipList_ConcurrentReadWritePerformance()
        {
            const int count = 10;
            var times = new int[count];
            var c = new ConcurrentSkipList<int>();
            for (var i = 0; i < count; i++)
            {
                var t = new CollectionReadWritePerformance(c, 10, 5, 10000);
                times[i] = t.Run().Milliseconds;
                c.Clear();
            }

            Console.WriteLine("Avg: {0}, Min: {1}, Max: {2}", times.Average(), times.Min(), times.Max());
            Console.WriteLine(string.Join(" ", times));
        }
    }
}
