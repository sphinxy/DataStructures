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
                var t = new CollectionReadWritePerformance(c, 10, 5, 1000);
                times[i] = t.Run().Milliseconds;
                PrintSkipListForm(c);
                c.Clear();
            }

            Console.WriteLine("Avg: {0}, Min: {1}, Max: {2}", times.Average(), times.Min(), times.Max());
            Console.WriteLine(string.Join(" ", times));
        }

        [TestMethod]
        public void SkipList_Population()
        {
            var target = new SkipList<int>();
            for (int i = 0; i < 10000; i++)
            {
                target.Add(i);
            }
            PrintSkipListForm(target);
        }

        private void PrintSkipListForm<T>(SkipList<T> target) where T : IComparable<T>
        {
            for (int i = target._height; i >= 0; i--)
            {
                Console.Write("{0:00}|", i);
                var node = target._head.GetNext(0);
                while (node != target._tail)
                {
                    Console.Write(node.Height >= i ? "*" : " ");
                    node = node.GetNext(0);
                }
                Console.WriteLine();
            }
            Console.WriteLine("----------------------------");
        }
    }
}
