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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Extension methods for profiling step.
    /// </summary>
    public static class ProfilingStepExtensions
    {
        /// <summary>
        /// Discards the current profiling step.
        /// </summary>
        /// <param name="step"></param>
        public static void Discard(this IDisposable step)
        {
            var profilingStep = step as IProfilingStep;
            if (profilingStep == null) return;

            profilingStep.Discard();
        }

        /// <summary>
        /// Add a tag to current profiling step.
        /// </summary>
        /// <param name="step"></param>
        /// <param name="tag"></param>
        public static void AddTag(this IDisposable step, string tag)
        {
            var profilingStep = step as IProfilingStep;
            if (profilingStep == null) return;

            profilingStep.AddTag(tag);
        }

        /// <summary>
        /// Add a custom data field to current profiling step.
        /// </summary>
        /// <param name="step"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddField(this IDisposable step, string key, string value)
        {
            var profilingStep = step as IProfilingStep;
            if (profilingStep == null) return;

            profilingStep.AddField(key, value);
        }
    }
}
