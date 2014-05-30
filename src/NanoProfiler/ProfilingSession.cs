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
using EF.Diagnostics.Profiling.Storages;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Represents a profiling session.
    /// </summary>
    public sealed class ProfilingSession : MarshalByRefObject
    {
        private static IProfilerProvider _profilerProvider;
        private static IProfilingSessionContainer _profilingSessionContainer;
        private static IProfilingStorage _profilingStorage;

        private readonly IProfiler _profiler;
        private bool _isStopped;

        private const string InlineStepExecuteType = "inline";

        #region Properties

        /// <summary>
        /// Gets the <see cref="IProfiler"/> attached to the current profiling session.
        /// </summary>
        public IProfiler Profiler
        {
            get { return _profiler; }
        }

        /// <summary>
        /// Gets the current profiling session if exists.
        /// </summary>
        public static ProfilingSession Current
        {
            get { return _profilingSessionContainer.CurrentSession; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IProfilerProvider"/>.
        /// </summary>
        public static IProfilerProvider ProfilerProvider
        {
            get { return _profilerProvider; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _profilerProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IProfilingSessionContainer"/>.
        /// </summary>
        public static IProfilingSessionContainer ProfilingSessionContainer
        {
            get { return _profilingSessionContainer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _profilingSessionContainer = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IProfilingStorage"/>.
        /// </summary>
        public static IProfilingStorage ProfilingStorage
        {
            get { return _profilingStorage; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _profilingStorage = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="IProfilingFilter"/>s globally registered.
        /// Adds or removes items of this property to control the filtering of profiling sessions.
        /// </summary>
        public static List<IProfilingFilter> ProfilingFilters { get; private set; }

        #endregion

        #region Constructors

        static ProfilingSession()
        {
            // by default, use CallContextProfilingSessionContainer
            _profilingSessionContainer = new CallContextProfilingSessionContainer();

            // by default, use ProfilerProvider
            _profilerProvider = new ProfilerProvider();

            // by default, use JsonProfilingStorage
            _profilingStorage = new JsonProfilingStorage();

            ProfilingFilters = new List<IProfilingFilter>();
        }

        /// <summary>
        /// Initializes a <see cref="ProfilingSession"/> from an <see cref="IProfiler"/> instance.
        /// </summary>
        /// <param name="profiler">The attached <see cref="IProfiler"/> instance.</param>
        internal ProfilingSession(IProfiler profiler)
        {
            if (profiler == null)
            {
                throw new ArgumentNullException("profiler");
            }

            _profiler = profiler;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the profiling.
        /// </summary>
        /// <param name="name">The name of the profiling session.</param>
        /// <param name="tags">The tags of the profiling session.</param>
        public static void Start(string name, params string[] tags)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (ProfilingFilters.Count > 0)
            {
                foreach (var filter in ProfilingFilters)
                {
                    if (filter == null)
                    {
                        continue;
                    }

                    if (filter.ShouldBeExculded(name, tags))
                    {
                        return;
                    }
                }
            }

            IProfiler profiler = null;
            try
            {
                profiler = _profilerProvider.Start(name, _profilingStorage, tags);
            }
            catch (Exception ex)
            {
                _profilerProvider.HandleException(ex, typeof(ProfilingSession));
            }

            if (profiler != null)
            {
                // Create the current ProfilingSession
                _profilingSessionContainer.CurrentSession = new ProfilingSession(profiler);
            }
        }

        /// <summary>
        /// Stops the current profiling session.
        /// </summary>
        /// <param name="discardResults">
        /// When true, discards the profiling results of the entire profiling session.
        /// </param>
        public static void Stop(bool discardResults = false)
        {
            var profilingSession = Current;
            if (profilingSession != null && !profilingSession._isStopped)
            {
                try
                {
                    profilingSession._profiler.Stop(discardResults);
                }
                catch (Exception ex)
                {
                    _profilerProvider.HandleException(ex, typeof(ProfilingSession));
                }

                profilingSession._isStopped = true;
            }

            // Clear the current profiling session on stopping
            _profilingSessionContainer.CurrentSession = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns>Returns the created <see cref="IProfilingStep"/>.</returns>
        internal IProfilingStep StepImpl(string name, string[] tags)
        {
            IProfilingStep step = null;

            try
            {
                step = _profiler.Step(name, tags, InlineStepExecuteType);
            }
            catch (Exception ex)
            {
                _profilerProvider.HandleException(ex, this);
            }

            return step;
        }

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <returns>Returns the created <see cref="System.IDisposable"/> as the ignored step.</returns>
        internal IDisposable IgnoreImpl()
        {
            IDisposable ignoredStep = null;
            try
            {
                ignoredStep = _profiler.Ignore();
            }
            catch (Exception ex)
            {
                _profilerProvider.HandleException(ex, this);
            }

            return ignoredStep;
        }

        #endregion
    }
}
