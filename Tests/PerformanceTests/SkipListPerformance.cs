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
            var count = 10;
            var times = new int[count];
            for (var i = 0; i < count; i++)
            {
                var c = new ConcurrentSkipList2<int>();
                var t = new CollectionReadWritePerformance(c, 20, 10, 10000);
                times[i] = t.Run().Milliseconds;
            }

            Array.Sort(times);
            var min = times[0];
            var max = times[count - 1];
            var avg = times.Sum()/count;
            Console.WriteLine("Avg: {0}, Min: {1}, Max: {2}", avg, min, max);
        }
    }
}
