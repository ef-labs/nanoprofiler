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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EF.Diagnostics.Profiling.Timings
{
    /// <summary>
    /// The default timing session class.
    /// </summary>
    public sealed class TimingSession : Timing, ITimingSession
    {
        private readonly IProfiler _profiler;

        private const string Session = "session";
        private ConcurrentQueue<ITiming> _timings;

        #region ITimingSession Members

        /// <summary>
        /// Gets the machine name.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the timings.
        /// </summary>
        public IEnumerable<ITiming> Timings
        {
            get { return _timings ?? (_timings = new ConcurrentQueue<ITiming>()); }
            set { _timings = (value == null ? new ConcurrentQueue<ITiming>() : new ConcurrentQueue<ITiming>(value)); }
        }

        /// <summary>
        /// Adds a timing to the session.
        /// </summary>
        /// <param name="timing"></param>
        public void AddTiming(ITiming timing)
        {
            if (timing == null) throw new ArgumentNullException("timing");

            _timings.Enqueue(timing);
        }

        #endregion

        #region ITiming Members

        /// <summary>
        /// Gets or sets the duration milliseconds of the timing.
        /// </summary>
        public override long DurationMilliseconds
        {
            get
            {
                if (_profiler != null) return (long)_profiler.Elapsed.TotalMilliseconds;

                return base.DurationMilliseconds;
            }
            set { base.DurationMilliseconds = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="TimingSession"/>.
        /// </summary>
        /// <param name="profiler"></param>
        /// <param name="name"></param>
        /// <param name="tags"></param>
        public TimingSession(IProfiler profiler, string name, TagCollection tags)
            : base(profiler, Session, null, name, tags)
        {
            _profiler = profiler;
            _timings = new ConcurrentQueue<ITiming>();
            Data = new Dictionary<string, string>();
            MachineName = Environment.MachineName;
        }

        /// <summary>
        /// Initializes a <see cref="TimingSession"/>.
        /// </summary>
        public TimingSession()
        {
        }

        #endregion
    }
}
