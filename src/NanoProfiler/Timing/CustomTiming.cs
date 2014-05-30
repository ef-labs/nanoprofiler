/*
    The MIT License (MIT)
    Copyright © 2014 Englishtown <opensource@englishtown.com>

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

namespace EF.Diagnostics.Profiling.Timing
{
    /// <summary>
    /// Represents a child custom timing of a step timing.
    /// </summary>
    public class CustomTiming : TimingBase
    {
        /// <summary>
        /// Gets or sets the size of input.
        /// </summary>
        public int? InputSize { get; set; }

        /// <summary>
        /// Gets or sets the input data.
        /// </summary>
        public string InputData { get; set; }

        /// <summary>
        /// Gets or sets the size of output.
        /// </summary>
        public int OutputSize { get; set; }

        /// <summary>
        /// Gets or sets the milliseconds when output starts since the start of the profiling session
        /// </summary>
        public long? OutputStartMilliseconds { get; set; }

        /// <summary>
        /// Initializes a new <see cref="CustomTiming"/>.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the <see cref="CustomTiming"/> to when stops.
        /// </param>
        /// <param name="type">The type of the <see cref="CustomTiming"/></param>
        /// <param name="name">The name of the <see cref="CustomTiming"/></param>
        public CustomTiming(IProfiler profiler, string type, string name)
            : base(profiler, type, ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId, name)
        {
            StartMilliseconds = profiler.DurationMilliseconds;
        }
    }
}
