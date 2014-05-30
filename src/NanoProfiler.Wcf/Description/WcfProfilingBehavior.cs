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

using System.ServiceModel.Description;

using EF.Diagnostics.Profiling.ServiceModel.Dispatcher;
using EF.Diagnostics.Profiling.Web;

namespace EF.Diagnostics.Profiling.ServiceModel.Description
{
    /// <summary>
    /// A WCF profiling behavior which starts/stops service profiling if configured as service endpoint behavior,
    /// and profiles client timing of WCF service calls if configured as client endpoint behavior.
    /// </summary>
    public class WcfProfilingBehavior : IEndpointBehavior
    {
        #region Constructors

        static WcfProfilingBehavior()
        {
            // set WcfProfilingSessionContainer as the default profiling session container
            // if the current one is CallContextProfilingSessionContainer or WebProfilingSessionContainer
            var type = ProfilingSession.ProfilingSessionContainer.GetType();
            if (type == typeof(CallContextProfilingSessionContainer) || type == typeof(WebProfilingSessionContainer))
            {
                ProfilingSession.ProfilingSessionContainer = new WcfProfilingSessionContainer();
            }
        }

        #endregion

        #region IEndpointBehavior Members

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            var inspector = new WcfTimingClientMessageInspector();
            clientRuntime.MessageInspectors.Add(inspector);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            var inspector = new WcfProfilingDispatchMessageInspector();
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}
