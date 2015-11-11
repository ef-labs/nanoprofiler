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

using EF.Diagnostics.Profiling.ProfilingFilters;
using EF.Diagnostics.Profiling.Web.Handlers;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace EF.Diagnostics.Profiling.Web
{
    /// <summary>
    /// The PreApplicationStart helper class for setting HttpContextCallContextProfilingSessionContainer
    /// as the default profiling session container to make
    /// ProfilingSession.Current work consistently in web application.
    /// </summary>
    public static class PreApplicationStart
    {
        /// <summary>
        /// The init method to be called in app startup.
        /// </summary>
        public static void Init()
        {
            // set WebProfilingSessionContainer as the default profiling session container
            // if the current one is CallContextProfilingSessionContainer
            if (ProfilingSession.ProfilingSessionContainer.GetType() == typeof(CallContextProfilingSessionContainer))
            {
                ProfilingSession.ProfilingSessionContainer = new WebProfilingSessionContainer();
            }

            // register NanoProfilerModule
            DynamicModuleUtility.RegisterModule(typeof(NanoProfilerModule));

            // ignore nanoprofiler view-result requests from profiling
            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/nanoprofiler"));
        }
    }
}
