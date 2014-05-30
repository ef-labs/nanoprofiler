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
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.ProfilingFilters;
using EF.Diagnostics.Profiling.Unity;
using EF.Diagnostics.Profiling.Web.ProfilingFilters;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

using NanoProfiler.Demos.SimpleDemo.Code.Biz;
using NanoProfiler.Demos.SimpleDemo.Code.Data;
using slf4net.Resolvers;

namespace NanoProfiler.Demos.SimpleDemo
{
    public class Global : System.Web.HttpApplication
    {
        public static readonly IUnityContainer Container = new UnityContainer();

        protected void Application_Start(object sender, EventArgs e)
        {
            // print profiling logs to debug output
            slf4net.LoggerFactory.SetFactoryResolver(new SimpleFactoryResolver(item => Debug.WriteLine(item.Message)));

            // register profiling filters to exclude some URLs from profiling
            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("_tools/"));
            ProfilingSession.ProfilingFilters.Add(new FileExtensionProfilingFilter("jpg", "js", "css"));

            #region Optional bootstrap code for unity based deep profiling and policy injection based profiling only

            // Register types to unity container to demo unity based deep profiling & policy injection based profiling.
            Container.RegisterType<IDemoDBDataService, DemoDBDataService>(
                new ContainerControlledLifetimeManager()
                , new InterceptionBehavior<PolicyInjectionBehavior>()); //enable policy injection
            Container.RegisterType<IDemoDBService, DemoDBService>(new ContainerControlledLifetimeManager());

            // Enable policy injection for interface types registered with PolicyInjectionBehavior
            Container.AddNewExtension<Interception>()
                .Configure<Interception>()
                .SetDefaultInterceptorFor<IDemoDBDataService>(new InterfaceInterceptor());

            // Enable deep profiling extension for profiling any interface methods on
            // interface types containing "DemoDBService".
            // When deep profiling is enabled on specified types, policy injection will be ignored.
            Container.AddExtension(new DeepProfilingExtension(new RegexDeepProfilingFilter(new Regex("DemoDBService"))));

            #endregion
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            ProfilingSession.Start(Request.Url.ToString(), "tag1");
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            ProfilingSession.Stop();
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}
