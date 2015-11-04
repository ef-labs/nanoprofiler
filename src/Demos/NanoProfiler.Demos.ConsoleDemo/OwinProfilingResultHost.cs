using EF.Diagnostics.Profiling.Web.Extensions.Handlers;
using Owin;

namespace NanoProfiler.Demos.ConsoleDemo
{
    public class OwinProfilingResultHost
    {
        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(ProfilingResultsModule.GetProfilingResultsJson());
            });
        }
    }
}
