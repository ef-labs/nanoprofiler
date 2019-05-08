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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel.Channels;

using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling.ServiceModel
{
    /// <summary>
    /// Represents the WCF timing of a WCF service call.
    /// </summary>
    public class WcfTiming : Timing
    {
        private readonly IProfiler _profiler;
        private const string WcfTimingType = "wcf";
        private const string CorrelationIdKey = "correlationId";

        /// <summary>
        /// Gets the correlationId of a web timing.
        /// </summary>
        public string CorrelationId
        {
            get { return Data[CorrelationIdKey]; }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new WCF timing.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the timing to when stops.
        /// </param>
        /// <param name="requestMessage">
        ///     The request message of the WCF service method being called &amp; profiled.
        /// </param>
        public WcfTiming(IProfiler profiler, ref Message requestMessage)
            : base(profiler, WcfTimingType, ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId, requestMessage.Headers.Action, null)
        {
            if (requestMessage == null)
            {
                throw new ArgumentNullException("requestMessage");
            }

            _profiler = profiler;
            StartMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds;
            Sort = profiler.Elapsed.Ticks;
            Data = new ConcurrentDictionary<string, string>();
            Data[CorrelationIdKey] = Guid.NewGuid().ToString("N");
            var requestMessageContent = ToXml(ref requestMessage);
            Data["requestMessage"] = requestMessageContent;
            Data["requestSize"] = requestMessageContent.Length.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Stops the current WCF timing.
        /// </summary>
        public void Stop()
        {
            DurationMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds;

            _profiler.GetTimingSession().AddTiming(this);
        }

        #endregion

        #region Private Methods

        private static string ToXml(ref Message message)
        {
            if (message == null)
            {
                return null;
            }

            using (var buffer = message.CreateBufferedCopy(int.MaxValue))
            {
                message = buffer.CreateMessage();

                using (var messageCopy = buffer.CreateMessage())
                using (var reader = messageCopy.GetReaderAtBodyContents())
                {
                    return reader.ReadOuterXml();
                }
            }
        }

        #endregion
    }
}
