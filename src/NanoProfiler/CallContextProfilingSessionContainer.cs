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
using System.Runtime.Remoting.Messaging;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// The default CallContext based <see cref="IProfilingSessionContainer"/> implementation.
    /// </summary>
    public class CallContextProfilingSessionContainer : IProfilingSessionContainer
    {
        private readonly bool _useWeakReference;

        private const string CurrentProfilingSessionCacheKey = "nano_profiler::current_profiling_session";
        private const string CurrentProfilingStepIdCacheKey = "nano_profiler::current_profiling_step_id";

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="CallContextProfilingSessionContainer"/>.
        /// </summary>
        /// <param name="useWeakReference">Whether or not to use WeakReference to wrap <see cref="ProfilingSession"/>.</param>
        public CallContextProfilingSessionContainer(bool useWeakReference = false)
        {
            _useWeakReference = useWeakReference;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets or sets the current ProfilingSession.
        /// </summary>
        public ProfilingSession CurrentSession
        {
            get
            {
                var obj = CallContext.GetData(CurrentProfilingSessionCacheKey);
                if (obj == null)
                {
                    return null;
                }

                // try to cast as WeakReference first
                var weakProfilingSession = obj as WeakReference;
                if (weakProfilingSession != null)
                {
                    if (weakProfilingSession.IsAlive)
                    {
                        return weakProfilingSession.Target as ProfilingSession;
                    }

                    // set null CurrentSession and return null if weak reference is no longer alive
                    CurrentSession = null;
                    CurrentSessionStepId = null;
                    return null;
                }

                // try to cast as ProfilingSession directly
                return obj as ProfilingSession;
            }
            set
            {
                var obj = (value != null && _useWeakReference ? new WeakReference(value) : (object)value);
                CallContext.LogicalSetData(CurrentProfilingSessionCacheKey, obj);
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

        #endregion
    }
}
