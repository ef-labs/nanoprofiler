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
    /// Represents the timing of a <see cref="EF.Diagnostics.Profiling.IProfilingStep"/>.
    /// </summary>
    public class StepTiming : TimingBase, IProfilingStep
    {
        private readonly IProfiler _profiler;
        private bool _isDiscarded;
        private bool _isStopped;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="StepTiming"/>.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the <see cref="StepTiming"/> to when stops.
        /// </param>
        /// <param name="name">The name of the <see cref="StepTiming"/>.</param>
        public StepTiming(IProfiler profiler, string name)
            : base(
                profiler
                , "step"
                , GetParentId(profiler)
                , name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            _profiler = profiler;
            StartMilliseconds = _profiler.DurationMilliseconds;
            Sort = profiler.GetDurationTicks();
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = Id;
        }

        #endregion

        /// <summary>
        /// Discards the current profiling step.
        /// </summary>
        public void Discard()
        {
            _isDiscarded = true;
        }

        #region IProfilingStep Members

        void IProfilingStep.Discard()
        {
            Discard();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the current <see cref="StepTiming"/>.
        /// </summary>
        ~StepTiming()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                Stop(true);
            }

            // free unmanaged resources
        }

        /// <summary>
        /// Stops the current <see cref="StepTiming"/> and adds the <see cref="StepTiming"/> to profiler.
        /// </summary>
        /// <param name="addToProfiler">
        ///     Whether or not add the current <see cref="StepTiming"/> to profiler when stops.
        /// </param>
        public void Stop(bool addToProfiler)
        {
            if (!_isDiscarded && !_isStopped)
            {
                DurationMilliseconds = _profiler.DurationMilliseconds - StartMilliseconds;
                _isStopped = true;
                ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = ParentId;

                if (addToProfiler)
                {
                    _profiler.AddStepTiming(this);
                }
            }
        }

        #endregion

        #region Private Methods

        private static Guid? GetParentId(IProfiler profiler)
        {
            var parentStepId = ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId;
            if (parentStepId.HasValue)
            {
                return parentStepId;
            }

            if (profiler != null)
            {
                return profiler.Id;
            }

            return null;
        }

        #endregion
    }
}
