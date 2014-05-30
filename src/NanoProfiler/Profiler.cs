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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using EF.Diagnostics.Profiling.Timing;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// The default <see cref="IProfiler"/> implementation.
    /// </summary>
    public class Profiler : TimingBase, IProfiler
    {
        private readonly IProfilingStorage _storage;
        private readonly DateTime _started;
        private readonly Stopwatch _stopwatch;
        private readonly ConcurrentQueue<StepTiming> _stepTimings;
        private readonly ConcurrentQueue<CustomTiming> _customTimings;

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        public string Client { get; set; }

        /// <summary>
        /// Gets or sets the local address.
        /// </summary>
        public string LocalAddress { get; set; }

        /// <summary>
        /// Gets the time when the <see cref="IProfiler"/> is started.
        /// </summary>
        public override DateTime Started
        {
            get { return _started; }
        }

        /// <summary>
        /// Gets or sets the duration milliseconds since the start of the <see cref="IProfiler"/>.
        /// </summary>
        public override long DurationMilliseconds
        {
            get
            {
                return _stopwatch.ElapsedMilliseconds;
            }
            set
            {
                throw new InvalidOperationException("The Profiler.DurationMilliseconds property doesn't allow to be set.");
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="Profiler"/> class instance.
        /// </summary>
        /// <param name="name">The profiler name.</param>
        /// <param name="storage">The profiler storage.</param>
        /// <param name="tags">Tags of the profiler.</param>
        public Profiler(string name, IProfilingStorage storage, IEnumerable<string> tags)
            : base("session", name)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }

            _storage = storage;

            Tags = (tags == null ? null : new TagCollection(tags));
            _started = DateTime.UtcNow;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _stepTimings = new ConcurrentQueue<StepTiming>();
            _customTimings = new ConcurrentQueue<CustomTiming>();

            _stepTimings.Enqueue(new StepTiming(this, "root"));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <param name="executeType">The executeType of this step.</param>
        /// <returns>Returns the created <see cref="IProfilingStep"/>.</returns>
        public IProfilingStep Step(string name, IEnumerable<string> tags, string executeType)
        {
            var step = new StepTiming(this, name) { ExecuteType = executeType };
            if (tags != null && tags.Any())
            {
                step.Tags = new TagCollection(tags);
            }

            return step;
        }

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <returns>Returns the created <see cref="System.IDisposable"/> as the ignored step.</returns>
        public IDisposable Ignore()
        {
            IProfilingStep ignoredStep = new StepTiming(this, "ignored step");
            ignoredStep.Discard();
            return ignoredStep;
        }

        /// <summary>
        /// Stops the current profiler.
        /// </summary>
        /// <param name="discardResults">
        /// When true, ignore the profiling results of the profiler.
        /// </param>
        public void Stop(bool discardResults)
        {
            _stopwatch.Stop();

            // stop the root step timing
            _stepTimings.First().Stop(false);

            if (!discardResults)
            {
                _storage.SaveResult(this);
            }
        }

        /// <summary>
        /// Gets the collection of the profiled raw step timings of the current profilier.
        /// </summary>
        public IEnumerable<StepTiming> StepTimings
        {
            get { return _stepTimings; }
        }

        /// <summary>
        /// Adds a step timing to the raw step timing collection.
        /// </summary>
        /// <param name="stepTiming">The step timing to be added.</param>
        public virtual void AddStepTiming(StepTiming stepTiming)
        {
            if (stepTiming != null)
            {
                _stepTimings.Enqueue(stepTiming);
            }
        }

        /// <summary>
        /// Gets the collection of the profiled raw custom timings of the current profilier.
        /// </summary>
        public IEnumerable<CustomTiming> CustomTimings
        {
            get { return _customTimings; }
        }

        /// <summary>
        /// Adds a custom timing to the raw custom timing collection.
        /// </summary>
        /// <param name="customTiming"></param>
        public virtual void AddCustomTiming(CustomTiming customTiming)
        {
            if (customTiming != null)
            {
                _customTimings.Enqueue(customTiming);
            }
        }

        #endregion

        #region IProfiler Members

        string IProfiler.Client {
            get { return Client; }
            set { Client = value; }
        }

        string IProfiler.LocalAddress
        {
            get { return LocalAddress; }
            set { LocalAddress = value; }
        }

        IProfilingStep IProfiler.Step(string name, IEnumerable<string> tags, string executeType)
        {
            return Step(name, tags, executeType);
        }

        IDisposable IProfiler.Ignore()
        {
            return Ignore();
        }

        void IProfiler.Stop(bool discardResults)
        {
            Stop(discardResults);
        }

        IEnumerable<StepTiming> IProfiler.StepTimings
        {
            get { return StepTimings; }
        }

        void IProfiler.AddStepTiming(StepTiming stepTiming)
        {
            AddStepTiming(stepTiming);
        }

        IEnumerable<CustomTiming> IProfiler.CustomTimings
        {
            get { return CustomTimings; }
        }

        void IProfiler.AddCustomTiming(CustomTiming customTiming)
        {
            AddCustomTiming(customTiming);
        }

        #endregion
    }
}
