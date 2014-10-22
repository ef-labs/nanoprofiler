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

using System.Runtime.Serialization;

namespace EF.Diagnostics.Profiling.Storages
{
    /// <summary>
    /// Data class for serializing custom timing.
    /// </summary>
    [DataContract]
    public class CustomTimingData : TimingDataBase
    {
        /// <summary>
        /// The size of the custom timing input parameters.
        /// </summary>
        [DataMember(Name = "inputSize")]
        public int? InputSize { get; set; }

        /// <summary>
        /// The serialized data of the custom timing input parameters.
        /// </summary>
        [DataMember(Name = "inputData")]
        public string InputData { get; set; }

        /// <summary>
        /// The size of the custom timing output data.
        /// </summary>
        [DataMember(Name = "outputSize")]
        public int OutputSize { get; set; }

        /// <summary>
        /// The milliseconds of when output begins since the start of the profiling session.
        /// </summary>
        [DataMember(Name = "outputStart")]
        public long? OutputStartMilliseconds { get; set; }
    }
}
