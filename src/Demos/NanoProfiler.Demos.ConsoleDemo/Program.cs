using System;
using System.Threading;
using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.Web.Extensions.Storages;
using EF.Diagnostics.Profiling.Web.Storages;
using Microsoft.Owin.Hosting;

namespace NanoProfiler.Demos.ConsoleDemo
{
    class Program
    {
        private static ProfilingSession _profilingSession;

        static void Main(string[] args)
        {
            // use CircularBufferedProfilingStorage to keep latest results in-memory so that we could expose them at runtime
            // by default the profiling results are not kept in memory
            ProfilingSession.ProfilingStorage = new CircularBufferedProfilingStorage();

            // start the Owin host to expose in-memory profiling results via Web
            WebApp.Start<OwinProfilingResultHost>("http://*:2222");

            ProfilingSession.Start("my task starts: " + DateTime.Now.ToString());
            
            // keep an instance of profiling session somewhere to ensure it is not collected by GC
            _profilingSession = ProfilingSession.Current;

            using (ProfilingSession.Current.Step("do something 1"))
            {
                Thread.Sleep(200);

                // you could use a passed-through profiling session instance as well.
                // ProfilingSession.Current holds the profiling session internally by logic CallContext,
                // in complex multi-threading cases where logic CallContext might not work,
                // you might have to pass through the profiling session instead of ProfilingSession.Current
                using (_profilingSession.Step("do something 2"))
                {
                    Thread.Sleep(200);
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
