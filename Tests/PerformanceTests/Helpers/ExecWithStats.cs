using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerformanceTests.Helpers
{
    internal sealed class ExecWithStats
    {
        private readonly Action _action;
        private readonly CountdownEvent _doneEvent;
        private readonly int _count;
        private int _exceptionsCount;
        private readonly string _name;
        private readonly TimeSpan[] _times;
        private readonly Stopwatch _stopwatch;

        public ExecWithStats(string name, int count, Action action, CountdownEvent doneEvent = null)
        {
            _action = action;
            _doneEvent = doneEvent;
            _name = name;
            _count = count;
            _exceptionsCount = 0;
            _times = new TimeSpan[_count];
            _stopwatch = new Stopwatch();
        }

        public void Exec()
        {
            for (var i = 0; i < _count; i++)
            {
                _stopwatch.Start();
                try
                {
                    _action();
                }
                catch (Exception)
                {
                    _exceptionsCount++;
                }
                finally
                {
                    _stopwatch.Stop();
                    _times[i] = _stopwatch.Elapsed;
                    _stopwatch.Reset();
                }
            }
            if (_doneEvent != null) _doneEvent.Signal();
        }

        public Stats GetStats()
        {
            return new Stats(_name, _count, _times, _exceptionsCount);
        }

        public sealed class Stats
        {
            public readonly TimeSpan Min;   // Minimum
            public readonly TimeSpan Med;   // Median
            public readonly TimeSpan Avg;   // Average
            public readonly TimeSpan Max;   // Maximum
            public readonly int ExceptionsCount;
            public readonly string Name;

            public Stats(string name, int count, TimeSpan[] times, int exceptionsCount)
            {
                Array.Sort(times, 0, count);

                Min = times[0];
                Med = times[count / 2];
                Max = times[count - 1];
                Avg = new TimeSpan((int)times.Average(_ => _.Ticks));
                ExceptionsCount = exceptionsCount;
                Name = name;
            }
        }

        internal static void OutputStatsSummary(IEnumerable<ExecWithStats> execStats)
        {
            foreach (var stats in execStats.Select(_ => _.GetStats())
                .GroupBy(_ => _.Name)
                .Select(
                    _ =>
                        new
                        {
                            Name = _.Key,
                            ThreadsCount = _.Count(),
                            Min = _.Min(x => x.Min),
                            Med = _.OrderBy(x => x.Med).ElementAt(_.Count() / 2).Med,
                            Avg = new TimeSpan((int)_.Average(x => x.Avg.Ticks)),
                            Max = _.Max(x => x.Max),
                            Exceptions = _.Sum(x => x.ExceptionsCount)
                        }))
            {
                Console.WriteLine("Name:{0}", stats.Name);
                Console.WriteLine("ThreadsCount:{0}", stats.ThreadsCount);
                Console.WriteLine("Time: Min: {0}, Median:{1}, Average: {2}, Max {3}", stats.Min, stats.Med, stats.Avg,
                    stats.Max);
                Console.WriteLine("Exceptions: {0}\n", stats.Exceptions);
            }
        }
    }
}
