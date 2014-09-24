using System;
using System.ServiceModel.Dispatcher;

namespace EF.Diagnostics.Profiling.ServiceModel.Dispatcher
{
    /// <summary>
    /// Error handler of WCF profiling. To ignore profiling results on WCF service error.
    /// </summary>
    public sealed class WcfProfilingErrorHandler : IErrorHandler
    {
        bool IErrorHandler.HandleError(Exception error)
        {
            ProfilingSession.Stop(discardResults: true);

            // we don't really handle the error, so always return false to move on to next error handler
            return false;
        }

        void IErrorHandler.ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
        {
            // we don't really handle the error, so do nothing here
        }
    }
}
