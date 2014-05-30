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

using EF.Diagnostics.Profiling.Timing;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Represents of a generic profiler.
    /// </summary>
    public interface IProfiler : ITiming
    {
        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        string Client { get; set; }

        /// <summary>
        /// Gets or sets the local address.
        /// </summary>
        string LocalAddress { get; set; }

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <param name="executeType">The executeType of this step.</param>
        /// <returns>Returns the created <see cref="IProfilingStep"/>.</returns>
        IProfilingStep Step(string name, IEnumerable<string> tags, string executeType);

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <returns>Returns the created <see cref="System.IDisposable"/> as the ignored step.</returns>
        IDisposable Ignore();

        /// <summary>
        /// Stops the current profiler.
        /// </summary>
        /// <param name="discardResults">
        /// When true, ignore the profiling results of the profiler.
        /// </param>
        void Stop(bool discardResults = false);

        /// <summary>
        /// Gets the collection of the profiled raw step timings of the current profilier.
        /// </summary>
        IEnumerable<StepTiming> StepTimings { get; }

        /// <summary>
        /// Adds a step timing to the raw step timing collection.
        /// </summary>
        /// <param name="stepTiming">The step timing to be added.</param>
        void AddStepTiming(StepTiming stepTiming);

        /// <summary>
        /// Gets the collection of the profiled raw custom timings of the current profilier.
        /// </summary>
        IEnumerable<CustomTiming> CustomTimings { get; }

        /// <summary>
        /// Adds a custom timing to the raw custom timing collection.
        /// </summary>
        /// <param name="customTiming"></param>
        void AddCustomTiming(CustomTiming customTiming);
    }
}
