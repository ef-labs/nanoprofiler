/*
    The MIT License (MIT)
    Copyright © 2015 Englishtown <opensource@englishtown.com>

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// The default <see cref="IProfiler"/> implementation.
    /// </summary>
    internal sealed class Profiler : IProfiler
    {
        private readonly IProfilingStorage _storage;
        private readonly Stopwatch _stopwatch;
        private readonly ITimingSession _timingSession;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="Profiler"/> class instance.
        /// </summary>
        /// <param name="name">The profiler name.</param>
        /// <param name="storage">The profiler storage.</param>
        /// <param name="tags">Tags of the profiler.</param>
        public Profiler(string name, IProfilingStorage storage, TagCollection tags)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }

            _storage = storage;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _timingSession = new TimingSession(this, name, tags) {Started = DateTime.UtcNow};
            var rootTiming = new ProfilingStep(this, "root", null);
            _timingSession.AddTiming(rootTiming);
        }

        #endregion

        #region IProfiler Members

        public Guid Id
        {
            get { return _timingSession.Id; }
        }

        public TimeSpan Elapsed
        {
            get { return _stopwatch.Elapsed; }
        }

        public bool IsStopped
        {
            get { return !_stopwatch.IsRunning; }
        }

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns>Returns the created <see cref="IProfilingStep"/>.</returns>
        public IProfilingStep Step(string name, TagCollection tags)
        {
            return new ProfilingStep(this, name, tags);
        }

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <returns>Returns the created <see cref="System.IDisposable"/> as the ignored step.</returns>
        public IDisposable Ignore()
        {
            IProfilingStep ignoredStep = new ProfilingStep(this, "ignored step", null);
            ignoredStep.Discard();
            return ignoredStep;
        }

        /// <summary>
        /// Stops the current profiler.
        /// </summary>
        /// <param name="discardResults">
        /// When true, ignore the profiling results of the profiler.
        /// </param>
        public void Stop(bool discardResults)
        {
            if (IsStopped) return;

            _stopwatch.Stop();

            // stop the root step timing
            var rootStep = GetTimingSession().Timings.FirstOrDefault() as IProfilingStep;
            if (rootStep != null)
            {
                // the root step is added to profiling session on created,
                // so don't need to add it again on stopping
                rootStep.Stop(false);
            }

            // save result
            if (!discardResults)
            {
                var session = GetTimingSession();
                AddAggregationFields(session);
                _storage.SaveSession(session);
            }
        }

        public ITimingSession GetTimingSession()
        {
            return _timingSession;
        }

        #endregion

        #region Private Methods

        private static void AddAggregationFields(ITimingSession session)
        {
            if (session.Timings == null || !session.Timings.Any()) return;

            var groups = session.Timings.GroupBy(timing => timing.Type);
            foreach (var group in groups)
            {
                if (string.Equals("step", group.Key)) continue;

                session.Data[group.Key + "Count"] = group.Count().ToString(CultureInfo.InvariantCulture);
                session.Data[group.Key + "Duration"] = ((long)group.Average(timing => timing.DurationMilliseconds)).ToString(CultureInfo.InvariantCulture);
            }
        }

        #endregion
    }
}
