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
    /// The profiler provider for <see cref="Profiler"/>.
    /// </summary>
    public class ProfilerProvider : IProfilerProvider
    {
        private readonly slf4net.ILogger _logger = slf4net.LoggerFactory.GetLogger(typeof(ProfilerProvider));

        #region Public Methods

        /// <summary>
        /// Starts a profiling session.
        /// </summary>
        /// <param name="name">The name of the profiler.</param>
        /// <param name="storage">The profiler storage.</param>
        /// <param name="tags">Tags of the profiler to be started.</param>
        /// <returns>Returns the started <see cref="IProfiler"/>.</returns>
        public IProfiler Start(string name, IProfilingStorage storage, IEnumerable<string> tags)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }

            return CreateProfiler(name, storage, tags);
        }

        /// <summary>
        /// Handles exceptions thrown from method calls of profiler provider and the profilers created by the provider.
        /// </summary>
        /// <param name="ex">The exception thrown.</param>
        /// <param name="origin">The origin instance where trgiggered the method call.</param>
        public void HandleException(Exception ex, object origin)
        {
            _logger.Error(ex, "Unexpected exception thrown from {0}: {1}", origin, ex.Message);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates a <see cref="IProfiler"/> instance.
        /// </summary>
        /// <param name="name">The name of the profiler.</param>
        /// <param name="storage">The profiler storage.</param>
        /// <param name="tags">Tags of the profiler to be started.</param>
        /// <returns>Returns the created <see cref="IProfiler"/>.</returns>
        protected virtual Profiler CreateProfiler(
            string name, IProfilingStorage storage, IEnumerable<string> tags)
        {
            return new Profiler(name, storage, tags);
        }

        #endregion

        #region IProfilerProvider Members

        IProfiler IProfilerProvider.Start(string name, IProfilingStorage storage, IEnumerable<string> tags)
        {
            return Start(name, storage, tags);
        }

        void IProfilerProvider.HandleException(Exception ex, object origin)
        {
            HandleException(ex, origin);
        }

        #endregion
    }
}
