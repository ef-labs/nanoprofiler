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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EF.Diagnostics.Profiling.Storages
{
    /// <summary>
    /// The base data class for serializing timings.
    /// </summary>
    [DataContract]
    public abstract class TimingDataBase
    {
        /// <summary>
        /// The id of the profiling session.
        /// </summary>
        [DataMember(Name = "sessionId")]
        public Guid SessionId { get; set; }

        /// <summary>
        /// The type of this timing.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        /// The machine name where generated this timing.
        /// </summary>
        [DataMember(Name = "machine")]
        public string MachineName { get; set; }

        /// <summary>
        /// The id of this timing.
        /// </summary>
        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The id of the parent timing, if exists.
        /// </summary>
        [DataMember(Name = "parentId")]
        public Guid? ParentId { get; set; }

        /// <summary>
        /// The name of this timing.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The execute type of this timing.
        /// </summary>
        [DataMember(Name = "executeType")]
        public string ExecuteType { get; set; }

        /// <summary>
        /// The sortable date time format string of when this timing was started.
        /// </summary>
        [DataMember(Name = "started")]
        public string Started { get; set; }

        /// <summary>
        /// The milliseconds of when this timing started since the start of the profiling session.
        /// </summary>
        [DataMember(Name = "start")]
        public long StartMilliseconds { get; set; }

        /// <summary>
        /// The duration of this timing.
        /// </summary>
        [DataMember(Name = "duration")]
        public long DurationMilliseconds { get; set; }

        /// <summary>
        /// The tags of this timing.
        /// </summary>
        [DataMember(Name = "tags")]
        public List<string> Tags { get; set; }
    }
}
