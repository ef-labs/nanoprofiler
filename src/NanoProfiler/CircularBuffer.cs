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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// A ConcurrentQueue based simple circular buffer implementation.
    /// </summary>
    public sealed class CircularBuffer<T> : ICircularBuffer<T>
    {
        private readonly int _size;
        private readonly Func<T, bool> _shouldBeExcluded;
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="CircularBuffer{T}"/>.
        /// </summary>
        /// <param name="size">The size of the circular buffer.</param>
        /// <param name="shouldBeExcluded">Whether or not, an item should not be saved in circular buffer.</param>
        public CircularBuffer(int size = 100, Func<T, bool> shouldBeExcluded = null)
        {
            _size = size;
            _shouldBeExcluded = shouldBeExcluded;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an item to buffer.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (_size <= 0) return;

            if (_shouldBeExcluded == null || !_shouldBeExcluded(item))
            {
                _queue.Enqueue(item);
                if (_queue.Count > _size)
                {
                    _queue.TryDequeue(out item);
                }
            }
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        #endregion
    }
}
