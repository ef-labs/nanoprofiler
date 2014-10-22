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
using System.Threading;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Asynchronous saving results of <see cref="IProfiler"/>s with a single thread queue worker. 
    /// The worker thread is automatically started when the first item is added.
    /// Override the Save() method for custom saving logic.
    /// 
    /// All methods and properties are thread safe.
    /// </summary>
    /// <remarks></remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public abstract class ProfilingStorageBase : IProfilingStorage
    {
        private readonly slf4net.ILogger _logger = slf4net.LoggerFactory.GetLogger(typeof(ProfilingStorageBase));

        private readonly ConcurrentQueue<IProfiler> _profilerQueue = new ConcurrentQueue<IProfiler>();
        private Thread _workerThread;
        private readonly int _maxQueueLength;
        private readonly int _threadSleepMilliseconds;
        private readonly AutoResetEvent _processWait = new AutoResetEvent(false);
        private readonly ManualResetEvent _entryWait = new ManualResetEvent(true);
        private static readonly string ON_QUEUE_OVERFLOW_EVENT_MESSAGE = "ProfilingStorageBase worker queue overflowed";

        /// <summary>
        /// The infinite queue length.
        /// </summary>
        public const int Infinite = -1;

        /// <summary>
        /// Disables the queue, which means, each call to SaveResult saves the result immediately.
        /// </summary>
        public const int Inline = 0;

        #region  Constructors
        /// <summary>
        /// Constructs a new <see cref="ProfilingStorageBase"/>.
        /// </summary>
        /// <param name="maxQueueLength">
        ///     The max length of the internal queue.
        ///     Max queue length must be -1 (infinite), 0 (process inline) or a positive number.
        /// </param>
        /// <param name="threadSleepMilliseconds">The time the worker thread sleeps. A long sleep period or infinite can cause the process to live longer than necessary.</param>
        /// <remarks></remarks>
        protected ProfilingStorageBase(int maxQueueLength = 1000, int threadSleepMilliseconds = 100)
        {
            if (maxQueueLength < -1)
            {
                throw new ArgumentException("Max queue length must be -1 (infinite), 0 (process inline) or a positive number.");
            }

            if (threadSleepMilliseconds <= 0)
            {
                throw new ArgumentException("Thread sleep interval must be positive.");
            }

            _maxQueueLength = maxQueueLength;
            _threadSleepMilliseconds = threadSleepMilliseconds;
        }

        #endregion

        #region IProfilingStorage Members

        void IProfilingStorage.SaveResult(IProfiler profiler)
        {
            SaveResult(profiler);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves the results of an <see cref="IProfiler"/>.
        /// </summary>
        /// <param name="profiler">The <see cref="IProfiler"/> to be saved.</param>
        public void SaveResult(IProfiler profiler)
        {
            if (_maxQueueLength == Inline)
            {
                SaveProfiler(profiler);
            }
            else if (Count < _maxQueueLength || _maxQueueLength == Infinite)
            {
                Enqueue(profiler);
                InvokeThreadStart();
            }
            else
            {
                OnQueueOverflow(profiler);
            }
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        protected int Count
        {
            get
            {
                return _profilerQueue.Count;
            }
        }

        /// <summary>
        /// Saves an <see cref="IProfiler"/>.
        /// </summary>
        /// <param name="profiler"></param>
        protected abstract void SaveProfiler(IProfiler profiler);

        /// <summary>
        /// Enqueues a profiler to internal queue.
        /// </summary>
        /// <param name="profiler">The <see cref="IProfiler"/> to be enqueued.</param>
        protected void Enqueue(IProfiler profiler)
        {
            _profilerQueue.Enqueue(profiler);
        }

        /// <summary>
        /// Tries to dequeue a profiler from internal queue for processing.
        /// </summary>
        /// <param name="profiler">The <see cref="IProfiler"/> to be dequeued.</param>
        /// <returns>Returns the dequeued <see cref="IProfiler"/>.</returns>
        protected bool TryDequeue(out IProfiler profiler)
        {
            return _profilerQueue.TryDequeue(out profiler);
        }

        /// <summary>
        /// What to do on internal queue overflow.
        /// 
        /// By default, it will delay the enqueue of profiler by at most 5000ms and throws exception.
        /// </summary>
        /// <param name="profiler">The <see cref="IProfiler"/> being enqueued when internal queue overflow.</param>
        protected virtual void OnQueueOverflow(IProfiler profiler)
        {
            // On overflow, never block the main thread running,
            // simply throw away the item at the top of the queue, enqueue the new item and log the event
            // so basically, the queue works like a ring buffer
            IProfiler temp;
            TryDequeue(out temp);
            Enqueue(profiler);

            _logger.Error(ON_QUEUE_OVERFLOW_EVENT_MESSAGE);
        }

        #endregion

        #region Private Methods

        private void InvokeThreadStart()
        {
            lock (_profilerQueue)
            {
                // Kick off thread if not there
                if (_workerThread == null)
                {
                    _workerThread = new Thread(SaveQueuedProfilers) { Name = GetType().Name };
                    _workerThread.Start();
                }

                // Signal process to continue
                _processWait.Set();
            }
        }

        private void SaveQueuedProfilers()
        {
            do
            {
                // Suspend for a while
                _processWait.WaitOne(_threadSleepMilliseconds, exitContext: false);

                // Upgrade to foreground thread
                Thread.CurrentThread.IsBackground = false;

                // set null the current profiling session bound to the running thread to release the memory
                ProfilingSession.ProfilingSessionContainer.CurrentSession = null;
                ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = null;

                // Save all the queued profilers
                IProfiler profiler;
                while (TryDequeue(out profiler))
                {
                    SaveProfiler(profiler);
                    
                    // Signal waiting threads to continue
                    _entryWait.Set();
                }

                // Downgrade to background thread while waiting
                Thread.CurrentThread.IsBackground = true;

            } while (true);
        }

        #endregion
    }
}
