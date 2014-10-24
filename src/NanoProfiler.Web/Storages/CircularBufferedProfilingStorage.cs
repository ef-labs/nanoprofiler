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
using System.Collections.Concurrent;
using System.Collections.Generic;

using EF.Diagnostics.Profiling.Web.Handlers;

namespace EF.Diagnostics.Profiling.Web.Storages
{
    /// <summary>
    /// A circular buffered <see cref="IProfilingStorage"/> implementation.
    /// Used with <see cref="NanoProfilerModule"/> to view latest profiling results.
    /// </summary>
    public class CircularBufferedProfilingStorage : IProfilingStorage
    {
        private readonly int _size;
        private readonly Func<IProfiler, bool> _shouldBeExcluded;
        private readonly IProfilingStorage _wrappedStorage;

        private readonly ConcurrentQueue<IProfiler> _circularBuffer
            = new ConcurrentQueue<IProfiler>();

        /// <summary>
        /// Gets or sets the singleton instance of <see cref="CircularBufferedProfilingStorage"/>.
        /// </summary>
        public static CircularBufferedProfilingStorage Instance { get; set; }
        
        /// <summary>
        /// Gets latest results from circular buffer.
        /// </summary>
        public IEnumerable<IProfiler> LatestResults
        {
            get { return _circularBuffer; }
        }

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="CircularBufferedProfilingStorage"/>.
        /// </summary>
        /// <param name="size">The size of the circular buffer.</param>
        /// <param name="shouldBeExcluded">Whether or not, a <see cref="IProfiler"/> should not be saved in circular buffer.</param>
        /// <param name="wrappedStorage">
        ///     An optional <see cref="IProfilingStorage"/> instance to be wrapped.
        ///     If wrappedStorage is specified, <see cref="CircularBufferedProfilingStorage"/> calls
        ///     wrappedStorage.SaveResult() before saving to internal circular buffer.
        /// </param>
        public CircularBufferedProfilingStorage(int size = 100, Func<IProfiler, bool> shouldBeExcluded = null, IProfilingStorage wrappedStorage = null)
        {
            _size = size;
            _shouldBeExcluded = shouldBeExcluded;
            _wrappedStorage = wrappedStorage;

            // set Instance to be accessed by NanoProfilerModule for view-result feature
            Instance = this;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves a the result of a <see cref="IProfiler"/>.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> whose results to be saved.
        /// </param>
        /// <param name="inMemoryOnly">Whether or not only save result to memory.</param>
        public void SaveResult(IProfiler profiler, bool inMemoryOnly)
        {
            if (_wrappedStorage != null && !inMemoryOnly)
            {
                _wrappedStorage.SaveResult(profiler);
            }

            if (_shouldBeExcluded == null || !_shouldBeExcluded(profiler))
            {
                _circularBuffer.Enqueue(profiler);
                if (_circularBuffer.Count > _size)
                {
                    _circularBuffer.TryDequeue(out profiler);
                }
            }
        }

        #endregion

        #region IProfilingStorage Members

        void IProfilingStorage.SaveResult(IProfiler profiler)
        {
            SaveResult(profiler, false);
        }

        #endregion
    }
}
