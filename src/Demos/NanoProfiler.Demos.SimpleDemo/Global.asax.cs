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
using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.ProfilingFilters;
using Microsoft.Practices.Unity;
using NanoProfiler.Demos.SimpleDemo.Code.Biz;
using NanoProfiler.Demos.SimpleDemo.Code.Data;

namespace NanoProfiler.Demos.SimpleDemo
{
    public class Global : System.Web.HttpApplication
    {
        public static readonly IUnityContainer Container = new UnityContainer();

        protected void Application_Start(object sender, EventArgs e)
        {
            // register types to unity container
            Container.RegisterType<IDemoDBDataService, DemoDBDataService>(
                new ContainerControlledLifetimeManager());
            Container.RegisterType<IDemoDBService, DemoDBService>(
                new ContainerControlledLifetimeManager());
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // for web applications, profiling is started ad stopped automatically via NanoProfilerModule by default
            // you could add addtional tags or fields like below
            ProfilingSession.Current.AddTag("session tag 1");
            ProfilingSession.Current.AddField("sessioField1", "test1");

            // if you want to disable profiling
            // you could specify a global profiler filter like below
            // ProfilingSession.ProfilingFilters.Add(new DisableProfilingFilter());
            // or, you could also disable profiling globally
            // by adding the disable filter configuration in web.config
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            
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
