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
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Represents a profiling step of a <see cref="IProfiler"/>.
    /// </summary>
    public interface IProfilingStep : IDisposable
    {
        /// <summary>
        /// Discards the current profiling step.
        /// </summary>
        void Discard();

        /// <summary>
        /// Gets the timing of the profiling current step.
        /// </summary>
        /// <returns></returns>
        ITiming GetStepTiming();

        /// <summary>
        /// Stops the current <see cref="ProfilingStep"/> and adds the <see cref="ProfilingStep"/> to profiler.
        /// </summary>
        /// <param name="addToProfiler">
        ///     Whether or not add the current <see cref="ProfilingStep"/> to profiler when stops.
        /// </param>
        void Stop(bool addToProfiler);

        /// <summary>
        /// Add a tag to current profiling step.
        /// </summary>
        /// <param name="tag"></param>
        void AddTag(string tag);

        /// <summary>
        /// Add a custom data field to current profiling step.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void AddField(string key, string value);
    }
}
