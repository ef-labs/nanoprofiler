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
using System.Web;

namespace EF.Diagnostics.Profiling.Web
{
    /// <summary>
    /// A IProfilingSessionContainer implementation
    /// which stores current profiling session in both HttpContext.Items and CallContext,
    /// So that ProfilingSession.Current could work consistently in web application.
    /// </summary>
    public class WebProfilingSessionContainer : IProfilingSessionContainer
    {
        private const string CurrentProfilingSessionCacheKey = "nano_profiler::current_profiling_session";
        private const string CurrentProfilingStepIdCacheKey = "nano_profiler::current_profiling_step_id";
        private const string WebProfilingRequestType = "web";

        private readonly IProfilingSessionContainer _callContextProfilingSessionContainer
             = new CallContextProfilingSessionContainer();

        #region Public Methods

        /// <summary>
        /// Gets or sets the current ProfilingSession.
        /// </summary>
        public ProfilingSession CurrentSession
        {
            get
            {
                ProfilingSession profilingSession = null;
                if (HttpContext.Current != null)
                {
                    // Try to get current profiling session from HttpContext.Items first
                    profilingSession = HttpContext.Current.Items[CurrentProfilingSessionCacheKey] as ProfilingSession;
                }

                // when ProfilingSession.Start() executes in begin request event handler in a different thread
                // the callcontext might not contain the current profiling session correctly
                // so on reading of the current session from WebProfilingSessionContainer
                // double check to ensure current session stored in callcontext
                if (profilingSession != null && _callContextProfilingSessionContainer.CurrentSession == null)
                {
                    _callContextProfilingSessionContainer.CurrentSession = profilingSession;
                }

                return profilingSession
                    ?? _callContextProfilingSessionContainer.CurrentSession;
            }
            set
            {
                // Cache current profiler session in CallContext
                _callContextProfilingSessionContainer.CurrentSession = value;

                if (HttpContext.Current != null)
                {
                    if (value != null)
                    {
                        var profiler = value.Profiler;
                        if (profiler != null)
                        {
                            // set the profiler's request type to "web"
                            profiler.GetTimingSession().Data["requestType"] = WebProfilingRequestType;

                            // set client IP address
                            profiler.GetTimingSession().Data["clientIp"] = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Current.Request.UserHostAddress;
                        }
                    }

                    // Cache current profiler session in HttpContext.Items if HttpContext accessible
                    HttpContext.Current.Items[CurrentProfilingSessionCacheKey] = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current profiling step id.
        /// </summary>
        public Guid? CurrentSessionStepId
        {
            get
            {
                Guid? stepId = null;
                if (HttpContext.Current != null)
                {
                    // Try to get current step id from HttpContext.Items first
                    stepId = HttpContext.Current.Items[CurrentProfilingStepIdCacheKey] as Guid?;
                }

                // when ProfilingSession.Start() executes in begin request event handler in a different thread
                // the callcontext might not contain the current profiling session step id correctly
                // so on reading of the step id from WebProfilingSessionContainer
                // double check to ensure step id stored in callcontext
                if (stepId != null && _callContextProfilingSessionContainer.CurrentSessionStepId == null)
                {
                    _callContextProfilingSessionContainer.CurrentSessionStepId = stepId;
                }

                return stepId ?? _callContextProfilingSessionContainer.CurrentSessionStepId;
            }
            set
            {
                // Cache current step if in CallContext
                _callContextProfilingSessionContainer.CurrentSessionStepId = value;

                if (HttpContext.Current != null)
                {
                    // Cache current step id in HttpContext.Items if HttpContext accessible
                    HttpContext.Current.Items[CurrentProfilingStepIdCacheKey] = value;
                }
            }
        }

        /// <summary>
        /// Clears the current profiling session &amp; step id.
        /// </summary>
        public void Clear()
        {
            // clear callcontext container
            _callContextProfilingSessionContainer.Clear();

            // clear current session
            CurrentSession = null;

            // clear step id
            CurrentSessionStepId = null;
        }

        #endregion

        #region IProfilingSessionContainer Members

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
    }
}
