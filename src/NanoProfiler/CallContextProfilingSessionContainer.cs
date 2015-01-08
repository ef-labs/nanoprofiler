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
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Linq;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// The default CallContext based <see cref="IProfilingSessionContainer"/> implementation.
    /// </summary>
    public class CallContextProfilingSessionContainer : IProfilingSessionContainer
    {
        private static readonly ConcurrentDictionary<Guid, WeakReference> ProfilingSessionStore
            = new ConcurrentDictionary<Guid, WeakReference>();
        private const string CurrentProfilingSessionIdCacheKey = "nano_profiler::current_profiling_session_id";
        private const string CurrentProfilingStepIdCacheKey = "nano_profiler::current_profiling_step_id";
        private static readonly Timer CleanUpProfilingSessionStoreTimer
            = new Timer(CleanUpProfilingSessionStoreTimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CallContextProfilingSessionContainer"/>.
        /// </summary>
        public CallContextProfilingSessionContainer() { }

        /// <summary>
        /// for backward compatible only, don't use.
        /// </summary>
        /// <param name="useWeakReference"></param>
        [Obsolete("for backward compatible only", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CallContextProfilingSessionContainer(bool useWeakReference) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the periodically clean-up profiling session store period in milliseconds.
        /// </summary>
        public static int CleanUpProfilingSessionStorePeriod
        {
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("value should > 0");
                }

                CleanUpProfilingSessionStoreTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(value));
            }
        }

        /// <summary>
        /// Gets or sets the current ProfilingSession.
        /// </summary>
        public ProfilingSession CurrentSession
        {
            get
            {
                var obj = CallContext.GetData(CurrentProfilingSessionIdCacheKey);
                if (obj == null)
                {
                    return null;
                }

                var sessionId = (Guid?)obj;
                WeakReference wrapper;
                if (!ProfilingSessionStore.TryGetValue(sessionId.Value, out wrapper) || wrapper == null || !wrapper.IsAlive)
                {
                    return null;
                }

                return wrapper.Target as ProfilingSession;
            }
            set
            {
                if (value == null)
                {
                    CallContext.LogicalSetData(CurrentProfilingSessionIdCacheKey, null);
                    return;
                }

                ProfilingSessionStore.TryAdd(value.Profiler.Id, new WeakReference(value));
                CallContext.LogicalSetData(CurrentProfilingSessionIdCacheKey, (Guid?)value.Profiler.Id);
            }
        }

        /// <summary>
        /// Gets or sets the current profiling step id.
        /// </summary>
        public Guid? CurrentSessionStepId
        {
            get { return CallContext.LogicalGetData(CurrentProfilingStepIdCacheKey) as Guid?; }
            set { CallContext.LogicalSetData(CurrentProfilingStepIdCacheKey, value); }
        }

        /// <summary>
        /// Clears the current profiling session &amp; step id.
        /// </summary>
        public void Clear()
        {
            // clear current session
            var obj = CallContext.GetData(CurrentProfilingSessionIdCacheKey);
            if (obj != null)
            {
                var sessionId = (Guid?)obj;
                WeakReference temp;
                ProfilingSessionStore.TryRemove(sessionId.Value, out temp);
            }
            CurrentSession = null;

            // clear step id
            CurrentSessionStepId = null;
        }

        #endregion

        #region ICurrentProfilingSessionContainer Members

        ProfilingSession IProfilingSessionContainer.CurrentSession
        {
            get { return CurrentSession; }
            set { CurrentSession = value; }
        }

        Guid? IProfilingSessionContainer.CurrentSessionStepId
        {
            get { return CurrentSessionStepId; }
            set { CurrentSessionStepId = value; }
        }

        void IProfilingSessionContainer.Clear()
        {
            Clear();
        }

        #endregion

        #region Private Methods

        private static void CleanUpProfilingSessionStoreTimerCallback(object state)
        {
            WeakReference wrapper;

            // search for keys to remove
            var keysToRemove = new List<Guid>();
            foreach (var key in ProfilingSessionStore.Select(item => item.Key).ToList())
            {
                if (ProfilingSessionStore.TryGetValue(key, out wrapper) && !wrapper.IsAlive)
                {
                    keysToRemove.Add(key);
                }
            }

            // remove
            foreach (var key in keysToRemove)
            {
                ProfilingSessionStore.TryRemove(key, out wrapper);
            }
        }

        #endregion
    }
}
