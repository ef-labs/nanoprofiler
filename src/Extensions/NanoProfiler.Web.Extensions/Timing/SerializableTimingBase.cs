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
using EF.Diagnostics.Profiling.Timing;

namespace EF.Diagnostics.Profiling.Web.Extensions.Timing
{
    internal abstract class SerializableTimingBase : ITiming
    {
        public long DurationMilliseconds { get; set; }

        public string ExecuteType { get; set; }

        public Guid Id { get; set; }

        public string MachineName { get; set; }

        public string Name { get; set; }

        public Guid? ParentId { get; set; }

        public long StartMilliseconds { get; set; }

        public DateTime Started { get; set; }

        public HashSet<string> Tags { get; set; }

        public string Type { get; set; }

        public long Sort { get; set; }

        #region ITiming Members

        long ITiming.DurationMilliseconds
        {
            get { return DurationMilliseconds; }
        }

        string ITiming.ExecuteType
        {
            get
            {
                return ExecuteType;
            }
            set
            {
                ExecuteType = value;
            }
        }

        Guid ITiming.Id
        {
            get { return Id; }
        }

        string ITiming.MachineName
        {
            get { return MachineName; }
        }

        string ITiming.Name
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        Guid? ITiming.ParentId
        {
            get { return ParentId; }
        }

        long ITiming.StartMilliseconds
        {
            get { return StartMilliseconds; }
        }

        DateTime ITiming.Started
        {
            get { return Started; }
        }

        TagCollection ITiming.Tags
        {
            get
            {
                return Tags == null ? null : new TagCollection(Tags);
            }
            set
            {
                Tags = value;
            }
        }

        string ITiming.Type
        {
            get { return Type; }
        }

        #endregion
    }
}
