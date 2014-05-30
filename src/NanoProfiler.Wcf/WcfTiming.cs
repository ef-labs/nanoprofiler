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
using System.ServiceModel.Channels;

using EF.Diagnostics.Profiling.Timing;

namespace EF.Diagnostics.Profiling.ServiceModel
{
    /// <summary>
    /// Represents the WCF timing of a WCF service call.
    /// </summary>
    public class WcfTiming : CustomTiming
    {
        private readonly IProfiler _profiler;
        
        #region Constructors

        /// <summary>
        /// Initializes a new WCF timing.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the <see cref="CustomTiming"/> to when stops.
        /// </param>
        /// <param name="requestMessage">
        ///     The request message of the WCF service method being called &amp; profiled.
        /// </param>
        public WcfTiming(IProfiler profiler, Message requestMessage)
            : base(profiler, "wcf", requestMessage.Headers.Action)
        {
            if (requestMessage == null)
            {
                throw new ArgumentNullException("requestMessage");
            }

            _profiler = profiler;
            InputData = ToXml(requestMessage);
            InputSize = InputData.Length;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Stops the current WCF timing.
        /// </summary>
        public void Stop()
        {
            DurationMilliseconds = _profiler.DurationMilliseconds - StartMilliseconds;

            _profiler.AddCustomTiming(this);
        }

        #endregion

        #region Private Methods

        private static string ToXml(Message message)
        {
            if (message == null)
            {
                return null;
            }

            return message.ToString();
        }

        #endregion
    }
}
