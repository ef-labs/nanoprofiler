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

namespace EF.Diagnostics.Profiling.Timing
{
    /// <summary>
    /// The base timing class.
    /// </summary>
    public abstract class TimingBase : ITiming
    {
        private readonly IProfiler _profiler;

        #region Properties

        /// <summary>
        /// Gets the type of timing.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the machine name.
        /// </summary>
        public string MachineName { get; private set; }

        /// <summary>
        /// Gets the Identity.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the identity of the parent timing.
        /// </summary>
        public Guid? ParentId { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the execute type of timing.
        /// </summary>
        public string ExecuteType { get; set; }

        /// <summary>
        /// Gets the UTC time of when the timing is started.
        /// </summary>
        public virtual DateTime Started
        {
            get
            {
                if (_profiler == null)
                {
                    return default(DateTime);
                }

                return _profiler.Started.AddMilliseconds(StartMilliseconds);
            }
        }

        /// <summary>
        /// Gets or sets the start milliseconds since the start of the profling session.
        /// </summary>
        public long StartMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the duration milliseconds of the current timing.
        /// </summary>
        public virtual long DurationMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the tags of this timing.
        /// </summary>
        public TagCollection Tags { get; set; }

        /// <summary>
        /// Gets or sets the ticks of this timing for sorting.
        /// </summary>
        public long Sort { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="TimingBase"/>
        /// </summary>
        /// <param name="profiler">The <see cref="IProfiler"/>.</param>
        /// <param name="type">The type of timing.</param>
        /// <param name="parentId">The identity of the parent timing.</param>
        /// <param name="name">The name of the timing.</param>
        protected TimingBase(IProfiler profiler, string type, Guid? parentId, string name)
            : this(type, name)
        {
            if (profiler == null)
            {
                throw new ArgumentNullException("profiler");
            }

            _profiler = profiler;
            ParentId = parentId;
        }

        /// <summary>
        /// Initializes a <see cref="TimingBase"/>
        /// </summary>
        /// <param name="type">The type of timing.</param>
        /// <param name="name">The name of the timing.</param>
        protected TimingBase(string type, string name)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException("type");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            Type = type;
            Id = Guid.NewGuid();
            Name = name;
            MachineName = Environment.MachineName;
        }

        #endregion
    }
}
