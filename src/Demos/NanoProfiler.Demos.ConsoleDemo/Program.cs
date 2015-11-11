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
using System.Globalization;
using System.Threading;
using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.Timings;
using Microsoft.Owin.Hosting;

namespace NanoProfiler.Demos.ConsoleDemo
{
    class Program
    {
        private static ProfilingSession _profilingSession;

        static void Main(string[] args)
        {
            // use CircularBuffer to keep latest results in-memory so that we could expose them at runtime
            // by default the profiling results are not kept in memory
            ProfilingSession.CircularBuffer = new CircularBuffer<ITimingSession>();

            // start the Owin host to expose in-memory profiling results via Web
            WebApp.Start<OwinProfilingResultHost>("http://*:2222");

            ProfilingSession.Start("my task starts: " + DateTime.Now.ToString(CultureInfo.InvariantCulture), "tag1", "ta\"\r\ng2");
            
            // add addtional fields to session
            ProfilingSession.Current.Profiler.GetTimingSession().Data["sessionAdditionalField1"] = "test1";
            ProfilingSession.Current.Profiler.GetTimingSession().Data["sessionAdditionalField2Size"] = "3";

            // keep an instance of profiling session somewhere to ensure it is not collected by GC
            _profilingSession = ProfilingSession.Current;

            using (ProfilingSession.Current.Step("do something 1"))
            {
                Thread.Sleep(200);

                // you could use a passed-through profiling session instance as well.
                // ProfilingSession.Current holds the profiling session internally by logic CallContext,
                // in complex multi-threading cases where logic CallContext might not work,
                // you might have to pass through the profiling session instead of ProfilingSession.Current
                using (var step = _profilingSession.Step("do something 2", "step tag 1"))
                {
                    // add addtional tag to step
                    step.AddTag("step tag 2");

                    // add addtional step profiling fields
                    step.AddField("stepField1", "test1");
                    step.AddField("stepField2", "test2");

                    Thread.Sleep(200);

                    using (ProfilingSession.Current.Step(() => "do something 3"))
                    {
                        Thread.Sleep(100);
                    }
                }
            }

            // the profiling results is saved asynchronously when Stop() is called
            // in this demo, the log is saved to *.log files to the output folder via log4net
            ProfilingSession.Stop();

            // usually, we import the profiling logs to elasticsearch for visualization in kibana
            // but if you want to view the profiling results at runtime in a web view
            // for the console app or windows service app, you could expose the results in-memory
            // and display the results in another web app.
            //
            // in this demo, we use Owin to expose the results to http://localhost:2222
            // and if you visit the URL below in the web SimpleDemo app,
            // you are able to see the profiling results of the console app.
            // http://localhost:64511/nanoprofiler/view?import=http://localhost:2222

            Console.WriteLine("You could view the profiling results of this ConsoleDemo in the SimpleDemo site:\nhttp://localhost:64511/nanoprofiler/view?import=http://localhost:2222");

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
