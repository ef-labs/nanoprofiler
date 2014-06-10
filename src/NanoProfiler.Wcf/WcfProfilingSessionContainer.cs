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

using EF.Diagnostics.Profiling.Web;

namespace EF.Diagnostics.Profiling.ServiceModel
{
    /// <summary>
    /// A IProfilingSessionContainer implementation
    /// which stores current profiling session in WcfInstanceContext, HttpContext and CallContext,
    /// So that ProfilingSession.Current could work consistently in WCF application.
    /// </summary>
    public class WcfProfilingSessionContainer: IProfilingSessionContainer
    {
        private const string CurrentProfilingSessionCacheKey = "nano_profiler::current_profiling_session";
        private const string CurrentProfilingStepIdCacheKey = "nano_profiler::current_profiling_step_id";
        private const string WcfProfilingSessionType = "wcf";

        private readonly IProfilingSessionContainer _webProfilingSessionContainer
             = new WebProfilingSessionContainer();

        #region IProfilingSessionContainer Members

        ProfilingSession IProfilingSessionContainer.CurrentSession
        {
            get
            {
                ProfilingSession profilingSession = null;
                if (WcfContext.Current != null)
                {
                    // Try to get current profiling session from WcfInstanceContext.Items first
                    profilingSession = WcfContext.Current.Items[CurrentProfilingSessionCacheKey] as ProfilingSession;
                }

                return profilingSession
                    ?? _webProfilingSessionContainer.CurrentSession;

            }
            set
            {
                // Cache current profiler session in HttpContext and CallContext first
                _webProfilingSessionContainer.CurrentSession = value;

                if (WcfContext.Current != null)
                {
                    if (value != null)
                    {
                        var profiler = value.Profiler;
                        if (profiler != null)
                        {
                            // set the profiler's execute type to "wcf"
                            profiler.ExecuteType = WcfProfilingSessionType;
                        }
                    }

                    // Cache current profiler session in WcfInstanceContext.Items
                    // if WcfInstanceContext accessible
                    WcfContext.Current.Items[CurrentProfilingSessionCacheKey] = value;
                }
            }
        }

        Guid? IProfilingSessionContainer.CurrentSessionStepId
        {
            get
            {
                Guid? stepId = null;
                if (WcfContext.Current != null)
                {
                    // Try to get current step id from WcfInstanceContext.Items first
                    stepId = WcfContext.Current.Items[CurrentProfilingStepIdCacheKey] as Guid?;
                }

                return stepId ?? _webProfilingSessionContainer.CurrentSessionStepId;
            }
            set
            {
                // Cache current steo id in HttpContext and CallContext first
                _webProfilingSessionContainer.CurrentSessionStepId = value;

                if (WcfContext.Current != null)
                {
                    // Cache current step id in WcfInstanceContext.Items
                    // if WcfInstanceContext accessible
                    WcfContext.Current.Items[CurrentProfilingStepIdCacheKey] = value;
                }
            }
        }

        #endregion
    }
}
