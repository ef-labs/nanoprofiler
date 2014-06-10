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
        private const string WebProfilingSessionType = "web";

        private readonly IProfilingSessionContainer _callContextProfilingSessionContainer
             = new CallContextProfilingSessionContainer(true);

        #region IProfilingSessionContainer Members

        ProfilingSession IProfilingSessionContainer.CurrentSession
        {
            get
            {
                ProfilingSession profilingSession = null;
                if (HttpContext.Current != null)
                {
                    // Try to get current profiling session from HttpContext.Items first
                    profilingSession = HttpContext.Current.Items[CurrentProfilingSessionCacheKey] as ProfilingSession;
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
                            // set the profiler's execute type to "web"
                            profiler.ExecuteType = WebProfilingSessionType;

                            // set local address
                            profiler.LocalAddress = HttpContext.Current.Request.Url.ToString();

                            // set client IP address
                            profiler.Client = HttpContext.Current.Request.UserHostAddress;
                        }
                    }

                    // Cache current profiler session in HttpContext.Items if HttpContext accessible
                    HttpContext.Current.Items[CurrentProfilingSessionCacheKey] = value;
                }
            }
        }

        Guid? IProfilingSessionContainer.CurrentSessionStepId
        {
            get
            {
                Guid? stepId = null;
                if (HttpContext.Current != null)
                {
                    // Try to get current step id from HttpContext.Items first
                    stepId = HttpContext.Current.Items[CurrentProfilingStepIdCacheKey] as Guid?;
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

        #endregion
    }
}
