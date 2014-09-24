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

using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace EF.Diagnostics.Profiling.ServiceModel.Dispatcher
{
    /// <summary>
    /// The client endpoint message inspector for profiling WCF timing of WCF service calls.
    /// </summary>
    public sealed class WcfTimingClientMessageInspector : IClientMessageInspector
    {
        #region IClientMessageInspector Members

        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
            var wcfTiming = correlationState as WcfTiming;
            if (wcfTiming == null)
            {
                return;
            }

            var profilingSession = GetCurrentProfilingSession();
            if (profilingSession == null)
            {
                return;
            }

            // set the start output milliseconds as when we start reading the reply message
            wcfTiming.OutputStartMilliseconds = profilingSession.Profiler.DurationMilliseconds;

            if (reply != null)
            {
                // only if using HTTP binding, try to get content-length header value (if exists) as output size
                if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
                {
                    var property = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
                    int contentLength;
                    if (int.TryParse(property.Headers[HttpResponseHeader.ContentLength], out contentLength) && contentLength > 0)
                    {
                        wcfTiming.OutputSize = contentLength;
                    }
                }
            }
            wcfTiming.Stop();
        }

        object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var profilingSession = GetCurrentProfilingSession();
            if (profilingSession == null)
            {
                return null;
            }

            var wcfTiming = new WcfTiming(profilingSession.Profiler, ref request) { ExecuteType = channel.Via.Scheme };

            // we copies tags from the current profiling session to the remote WCF profiling session
            // so that we could group/wire client and server profiling session by tags in the future
            var tags = profilingSession.Profiler.Tags;
            if (tags != null)
            {
                if (!Equals(request.Headers.MessageVersion, MessageVersion.None))
                {
                    var untypedHeader = new MessageHeader<string>(tags.ToString()).GetUntypedHeader(
                        WcfProfilingMessageHeaderConstants.HeaderNameOfProfilingTags
                        , WcfProfilingMessageHeaderConstants.HeaderNamespace);
                    request.Headers.Add(untypedHeader);
                }
                else if (WebOperationContext.Current != null || channel.Via.Scheme == "http" || channel.Via.Scheme == "https")
                {
                    if (!request.Properties.ContainsKey(WcfProfilingMessageHeaderConstants.HeaderNameOfProfilingTags))
                    {
                        request.Properties.Add(
                            WcfProfilingMessageHeaderConstants.HeaderNameOfProfilingTags
                            , new HttpRequestMessageProperty());
                    }

                    if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
                    {
                        var httpRequestProperty = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
                        httpRequestProperty.Headers.Add(
                            WcfProfilingMessageHeaderConstants.HeaderNameOfProfilingTags
                            , WcfProfilingMessageHeaderConstants.HeaderNamespace);
                    }
                }
            }

            // return wcfTiming as correlationState of AfterReceiveReply() to stop the WCF timing in AfterReceiveReply()
            return wcfTiming;
        }

        #endregion

        #region Private Methods

        private static ProfilingSession GetCurrentProfilingSession()
        {
            var profilingSession = ProfilingSession.Current;
            if (profilingSession == null)
            {
                return null;
            }

            // set null current profiling session if the current session has already been stopped
            var isProfilingSessionStopped = (profilingSession.Profiler.Id == ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId);
            if (isProfilingSessionStopped)
            {
                ProfilingSession.ProfilingSessionContainer.CurrentSession = null;
                return null;
            }

            return profilingSession;
        }

        #endregion
    }
}
