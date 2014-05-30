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
        private const string CurrentProfilingSessionCacheKey = "tiny_profiler::current_profiling_session";
        private const string CurrentProfilingStepIdCacheKey = "tiny_profiler::current_profiling_step_id";

        #region Public Methods

        /// <summary>
        /// Gets or sets the current ProfilingSession.
        /// </summary>
        public ProfilingSession CurrentSession
        {
            get { return CallContext.GetData(CurrentProfilingSessionCacheKey) as ProfilingSession; }
            set { CallContext.LogicalSetData(CurrentProfilingSessionCacheKey, value); }
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
