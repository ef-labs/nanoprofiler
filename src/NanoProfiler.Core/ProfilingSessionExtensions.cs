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

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Extension methods for <see cref="ProfilingSession"/> class.
    /// </summary>
    public static class ProfilingSessionExtensions
    {
        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="profilingSession">The profiling session.</param>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns></returns>
        public static IDisposable Step(this ProfilingSession profilingSession, string name, params string[] tags)
        {
            if (profilingSession == null || string.IsNullOrEmpty(name)) return null;

            return profilingSession.StepImpl(name, tags);
        }

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="profilingSession">The profiling session.</param>
        /// <param name="getName">The delegate to get the name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns></returns>
        public static IDisposable Step(this ProfilingSession profilingSession, Func<string> getName, params string[] tags)
        {
            if (getName == null) return null;

            return profilingSession.Step(getName(), tags);
        }

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <param name="profilingSession">The profiling session.</param>
        /// <returns>Returns the created <see cref="System.IDisposable"/> as the ignored step.</returns>
        public static IDisposable Ignore(this ProfilingSession profilingSession)
        {
            if (profilingSession == null) return null;

            return profilingSession.IgnoreImpl();
        }

        /// <summary>
        /// Add a tag to current profiling session.
        /// </summary>
        /// <param name="profilingSession"></param>
        /// <param name="tag"></param>
        public static void AddTag(this ProfilingSession profilingSession, string tag)
        {
            if (profilingSession == null) return;

            profilingSession.AddTagImpl(tag);
        }

        /// <summary>
        /// Add a custom data field to current profiling session.
        /// </summary>
        /// <param name="profilingSession"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddField(this ProfilingSession profilingSession, string key, string value)
        {
            if (profilingSession == null) return;

            profilingSession.AddFieldImpl(key, value);
        }
    }
}
