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

using System.Threading;
using System.Threading.Tasks;
using System.Web;

using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.Web;
using Microsoft.Practices.Unity;
using NanoProfiler.Demos.SimpleDemo.Code.Biz;
using NanoProfiler.Demos.SimpleDemo.DemoService;

namespace NanoProfiler.Demos.SimpleDemo
{
    public class AsyncHandler : HttpTaskAsyncHandler
    {
        public override bool IsReusable
        {
            get { return true; }
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            using (ProfilingSession.Current.Step("ProcessRequestAsync"))
            {
                context.Response.Write("<a href=\"nanoprofiler/view\">View Profiling Results</a><br /><br />");
                context.Response.Write("<a href=\"nanoprofiler/view?export\">View Profiling Results as JSON</a><br /><br />");

                await ExecuteTask(context);

                using (var client = new WcfDemoServiceClient())
                {
                    await client.DoWorkAsync("somework");
                }

                SimulateWebRequest("http://some.thing.com");
            }
        }

        private void SimulateWebRequest(string url)
        {
            var profilingSession = ProfilingSession.Current;
            if (profilingSession != null && profilingSession.Profiler != null)
            {
                var webTiming = new WebTiming(profilingSession.Profiler, url);

                // sleep to simulate web request call
                Thread.Sleep(200);

                webTiming.Data["customField1"] = "value1";

                webTiming.Stop();
            }
        }

        private Task ExecuteTask(HttpContext context)
        {
            return Task.Factory.StartNew(() => Execute(context));
        }

        private void Execute(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            var demoData = Global.Container.Resolve<IDemoDBService>().LoadActiveDemoData();
            Parallel.ForEach(demoData, (item, state) =>
                {
                    using (ProfilingSession.Current.Step(() => "Async print item: " + item.Id))
                    {
                        context.Response.Write(string.Format(@"Id={0}, Name={1}<br />", item.Id, item.Name));
                    }
                });
        }
    }

}
