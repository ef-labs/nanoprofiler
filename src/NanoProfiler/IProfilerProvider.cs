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
using System.Collections.Generic;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Represents a profiler provider.
    /// </summary>
    public interface IProfilerProvider
    {
        /// <summary>
        /// Starts a profiling session.
        /// </summary>
        /// <param name="name">The name of the profiler.</param>
        /// <param name="storage">The profiler storage.</param>
        /// <param name="tags">Tags of the profiler to be started.</param>
        /// <returns>Returns the started <see cref="IProfiler"/>.</returns>
        IProfiler Start(string name, IProfilingStorage storage, IEnumerable<string> tags);

        /// <summary>
        /// Handles exceptions thrown from method calls of profiler provider and the profilers created by the provider.
        /// </summary>
        /// <param name="ex">The exception thrown.</param>
        /// <param name="origin">The origin instance where trgiggered the method call.</param>
        void HandleException(Exception ex, object origin);
    }
}
