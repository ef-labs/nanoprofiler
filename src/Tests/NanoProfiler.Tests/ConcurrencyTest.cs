using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests
{
    /// <summary>
    ///     Test concurrent profiling sessions using threads and tasks.
    /// </summary>
    [TestFixture]
    public class ConcurrencyTest
    {
        [ThreadStatic]
        private static ProfilingSession _current;

        [Test]
        [Repeat(100)]
        public void NanoProfilerWrapper_WritesReportWhenExitingRootSpan_SaveSessionCallbackAndTasks()
        {
            var reports = new ConcurrentBag<string>();
            ProfilingSession.CircularBuffer = new CircularBuffer<ITimingSession>(200);
            ProfilingSession.ProfilingStorage = new NanoProfilerStorageInMemory(session => { reports.Add(GetReport(session)); });

            const int count = 10;
            List<Task<string>> tasks = Enumerable.Range(1, count)
                .Select(i => Task.Run(DoWork))
                .ToList();

            Task.WhenAll(tasks).Wait();

            // Due to running two tasks inside each thread, the call context seems a bit arbitrary
            // so the number of reports vary depending on whether it consider child1.1 and child1.2
            // spans a child of the DoWork span or not.
            Assert.AreEqual(reports.Count, count);
            foreach (string report in reports)
                Assert.True(report.Contains("root = "), "report.Contains('root = ')");
        }

        [Test]
        [Repeat(100)]
        public void NanoProfilerWrapper_WritesReportWhenExitingRootSpan_WithNoOpStorageAndTasks()
        {
            ProfilingSession.CircularBuffer = new CircularBuffer<ITimingSession>(200);
            ProfilingSession.ProfilingStorage = new NoOperationProfilingStorage();

            const int count = 10;
            List<Task<string>> tasks = Enumerable.Range(1, count)
                .Select(i => Task.Run(DoWork))
                .ToList();

            List<string> reports = Task.WhenAll(tasks).Result.ToList();

            // Due to running two tasks inside each thread, the call context seems a bit arbitrary
            // so the number of reports vary depending on whether it consider child1.1 and child1.2
            // spans a child of the DoWork span or not.
            Assert.AreEqual(reports.Count, count);
            foreach (string report in reports)
                Assert.True(report.Contains("root = "), "report.Contains('root = ')");
        }

        [Test]
        [Repeat(100)]
        public void NanoProfilerWrapper_WritesReportWhenExitingRootSpan_WithSaveSessionCallbackAndThreads()
        {
            var reports = new ConcurrentBag<string>();
            ProfilingSession.CircularBuffer = new CircularBuffer<ITimingSession>(200);
            ProfilingSession.ProfilingStorage = new NanoProfilerStorageInMemory(session => { reports.Add(GetReport(session)); });

            const int threadCount = 10;
            List<Thread> threads = Enumerable.Range(1, threadCount)
                .Select(i => new Thread(() =>
                {
                    DoWork()
                        .Wait();
                }))
                .ToList();

            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            // Due to running two tasks inside each thread, the call context seems a bit arbitrary
            // so the number of reports vary depending on whether it consider child1.1 and child1.2
            // spans a child of the DoWork span or not.
            Assert.GreaterOrEqual(reports.Count, threadCount);
            foreach (string report in reports)
                Assert.True(report.Contains("root = "), "report.Contains('root = ')");
        }

        private static async Task<string> DoWork()
        {
            ProfilingSession.Start("DoWork");

            // Avoid GC of session
            _current = ProfilingSession.Current;

            ITimingSession timingSession = ProfilingSession.Current.Profiler.GetTimingSession();
            using (ProfilingSession.Current.Step("child1"))
            {
                await Task.WhenAll(Task.Run(() =>
                    {
                        using (ProfilingSession.Current.Step("child1.1"))
                        {
                            Thread.Sleep(10);
                        }
                    }),
                    Task.Run(() =>
                    {
                        using (ProfilingSession.Current.Step("child1.2"))
                        {
                            Thread.Sleep(20);
                        }
                    }));
            }
            ProfilingSession.Stop();
            string report = GetReport(timingSession);
            return report;
        }

        private static string GetReport(ITimingSession timingSession)
        {
            long totalMs = timingSession.DurationMilliseconds;

            var sb = new StringBuilder();
            sb.AppendLine($"Profiling session: [{timingSession.Name}], {totalMs} ms total)");

            IEnumerable<ITiming> timingsParentFirst = TraverseTimingsPreOrder(timingSession,
                timingSession.Timings.ToList());

            foreach (ITiming timing in timingsParentFirst)
            {
                int depth = GetDepth(timing);
                var depthString = new string('>', depth);
                sb.AppendLine($"{depthString} {timing.Name} = {timing.DurationMilliseconds} ms");
            }
            sb.AppendLine();
            string report = sb.ToString();
            return report;
        }


        private static IList<ITiming> TraverseTimingsPreOrder(ITiming parent,
            IEnumerable<ITiming> allTimings,
            int depth = 0)
        {
            SetDepth(parent, depth);
            IEnumerable<ITiming> timings = allTimings as IList<ITiming> ?? allTimings.ToList();

            return new[] {parent}.Concat(timings.Where(x => x.ParentId == parent.Id)
                .SelectMany(child => TraverseTimingsPreOrder(child, timings, depth + 1))).ToList();
        }

        private static void SetDepth(ITiming timing, int depth)
        {
            if (timing.Data == null)
                timing.Data = new ConcurrentDictionary<string, string>();
            timing.Data["depth"] = depth.ToString();
        }

        private static int GetDepth(ITiming timing)
        {
            return int.Parse(timing.Data["depth"]);
        }

        private class NanoProfilerStorageInMemory : IProfilingStorage
        {
            private readonly Action<ITimingSession> _onReport;

            public NanoProfilerStorageInMemory(Action<ITimingSession> onReport)
            {
                _onReport = onReport;
            }

            public void SaveSession(ITimingSession session)
            {
                _onReport?.Invoke(session);
            }
        }
    }
}
