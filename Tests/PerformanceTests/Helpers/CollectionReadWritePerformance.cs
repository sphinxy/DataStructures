using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PerformanceTests.Helpers
{
    internal sealed class CollectionReadWritePerformance
    {
        private const int MAX_VALUE = 100;
        private readonly ICollection<int> _target;
        private readonly Thread[] _threads;
        private readonly int _iterations;

        public CollectionReadWritePerformance(ICollection<int> target, int readersCount, int writersCount, int iterations)
        {
            _target = target;
            _iterations = iterations;
            var count = writersCount + readersCount;
            _threads = new Thread[count];

            for (var i = 0; i < writersCount; i++)
            {
                _threads[i] = new Thread(Writer);
            }
            for (var i = writersCount; i < count; i++)
            {
                _threads[i] = new Thread(Reader);
            }
        }

        public TimeSpan Run()
        {
            var watcher = new Stopwatch();
            watcher.Start();
            foreach (Thread t in _threads)
            {
                t.Start();
            }
            foreach (Thread t in _threads)
            {
                t.Join();
            }
            watcher.Stop();
            return watcher.Elapsed;
        }

        private void Reader()
        {
            var rand = new Random();
            try
            {
                for (var i = 0; i < _iterations; i++)
                {
                    _target.Contains(rand.Next(MAX_VALUE));
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex);
            }

        }

        private void Writer()
        {
            var rand = new Random();
            try
            {
                for (var i = 0; i < _iterations; i++)
                {
                    _target.Add(rand.Next(MAX_VALUE));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
