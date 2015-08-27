using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceTests.Helpers;

namespace PerformanceTests
{
    [TestClass]
    public class ConcurrentPriorityQueueTests
    {
        [TestMethod]
        public void SingleThreadTiming()
        {
            const int count = 1000000;
            var target = new ConcurrentPriorityQueue<int>(2);
            var watcher = new Stopwatch();

            watcher.Start();
            for (int i = 0; i < count; i++)
            {
                target.Add(i);
            }
            watcher.Stop();
            Assert.AreEqual(count, target.Count);
            Console.WriteLine("Add {0} elements: {1}", count, watcher.Elapsed);

            watcher.Restart();
            // ReSharper disable once UnusedVariable
            var enumerator = target.GetEnumerator();
            watcher.Stop();
            Console.WriteLine("Get enumerator for {0} elements: {1}", count, watcher.Elapsed);

            watcher.Restart();
            for (int i = 0; i < count; i++)
            {
                target.Take();
            }
            watcher.Stop();
            Assert.AreEqual(0, target.Count);
            Console.WriteLine("Take {0} elements: {1}", count, watcher.Elapsed);

            watcher.Start();
            for (int i = 0; i < 2 * count; i++)
            {
                target.Add(i);
            }
            watcher.Stop();
            Assert.AreEqual(2 * count, target.Count);
            Console.WriteLine("Add twice the capacity of {0} elements: {1}", count, watcher.Elapsed);
        }

        [TestMethod]
        public void MultiThreadEnqueue()
        {
            const int capacity = 1000000;
            const int threadsCount = 100;
            const int count = capacity / threadsCount;
            var target = new ConcurrentPriorityQueue<DateTime>();

            var execStats = new ExecWithStats[threadsCount];

            var watcher = new Stopwatch();

            // several threads enqueue elements
            watcher.Start();
            Parallel.For(0, threadsCount, index =>
            {
                execStats[index] = new ExecWithStats(string.Format("Add {0}", count), count, () => target.Add(new DateTime()));
                execStats[index].Exec();
            });

            watcher.Stop();
            Assert.AreEqual(capacity, target.Count);
            Console.WriteLine("{0} threads each enqueue {1} elements. total time: {2}\n", threadsCount, count, watcher.Elapsed);
            ExecWithStats.OutputStatsSummary(execStats);

            // several threads dequeue elements
            watcher.Start();
            Parallel.For(0, threadsCount, index =>
            {
                execStats[index] = new ExecWithStats(string.Format("Take {0}", count), count, () => target.Take());
                execStats[index].Exec();
            });
            watcher.Stop();
            Assert.AreEqual(0, target.Count);
            Console.WriteLine("\n{0} threads each dequeue {1} elements. total time: {2}\n", threadsCount, count, watcher.Elapsed);
            ExecWithStats.OutputStatsSummary(execStats);
        }

        [TestMethod]
        public void RaceWithStats()
        {
            const int capacity = 1000000;
            const int threadsCount = 100;
            const int count = capacity / threadsCount;
            var target = new ConcurrentPriorityQueue<DateTime>();
            var execStats = new List<ExecWithStats>();
            var threadWait = new CountdownEvent(threadsCount);

            // odd threads will enqueue elements, while even threads will dequeue
            // obviously there will be a race condition and especially in the beginning dequeue will throw, because queue will often be empty
            // the total number of exceptions on dequeue threads will correspond the the number of items left in the queue

            for (var i = 0; i < threadsCount; i++)
            {
                ExecWithStats exec;
                if (i % 2 != 0)
                {
                    exec = new ExecWithStats(string.Format("Add {0} elements", count), count, () => target.Add( new DateTime()), threadWait);
                }
                else
                {
                    exec = new ExecWithStats(string.Format("Take {0} elements", count), count, () => target.Take(), threadWait);
                }

                execStats.Add(exec);

                var thread = new Thread(() => exec.Exec());
                thread.Start();
            }

            // Wait for all threads in pool to calculate.
            threadWait.Wait();

            // Output stats summary
            ExecWithStats.OutputStatsSummary(execStats);

            // Output queue state
            Console.WriteLine("Queue count:{0}, capacity:{1}", target.Count, target.Capacity);

            // Un-comment for a detailed list of stats
            //Console.WriteLine("---------------------");
            //foreach (var execStat in execStats)
            //{
            //    var stats = execStat.GetStats();
            //    Console.WriteLine("Name:{0}, Min: {1}, Median: {2}, Max {3}, Exceptions: {4}", stats.Name, stats.Min, stats.Med, stats.Max, stats.ExceptionsCount);
            //}
        }

        [TestMethod]
        public void ConcurrentReadWritePerformance()
        {
            const int count = 10;
            var times = new int[count];
            var c = new ConcurrentPriorityQueue<int>();
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
