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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using EF.Diagnostics.Profiling.Configuration;
using EF.Diagnostics.Profiling.ProfilingFilters;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Represents a profiling session.
    /// </summary>
    public sealed class ProfilingSession : MarshalByRefObject
    {
        private static readonly slf4net.ILogger Logger = slf4net.LoggerFactory.GetLogger(typeof(ProfilingSession));

        private static IProfilingSessionContainer _profilingSessionContainer;
        private static IProfilingStorage _profilingStorage;
        private readonly IProfiler _profiler;

        #region Properties

        /// <summary>
        /// Gets the <see cref="IProfiler"/> attached to the current profiling session.
        /// </summary>
        public IProfiler Profiler
        {
            get { return _profiler; }
        }

        /// <summary>
        /// Gets the current profiling session.
        /// </summary>
        public static ProfilingSession Current
        {
            get { return _profilingSessionContainer.CurrentSession; }
        }

        /// <summary>
        /// Sets current profiling session as specified session and sets the parent step as specified.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="parentStepId">if parentStepId not specified, use the root step of session as parent step by default.</param>
        public static void SetCurrentProfilingSession(
            ProfilingSession session, Guid? parentStepId = null)
        {
            ProfilingSessionContainer.CurrentSession = null;
            ProfilingSessionContainer.CurrentSessionStepId = null;

            if (session == null || session.Profiler == null) return;

            var timingSession = session.Profiler.GetTimingSession();
            if (timingSession == null 
                || timingSession.Timings == null
                || timingSession.Timings.All(t => t.ParentId != timingSession.Id)) return;

            ProfilingSessionContainer.CurrentSession = session;

            if (parentStepId.HasValue && timingSession.Timings.Any(t => t.Id == parentStepId.Value && string.Equals(t.Type, "step")))
            {
                ProfilingSessionContainer.CurrentSessionStepId = parentStepId.Value;
            }
            else // if parentStepId not specified, use the root step of session as parent step by default
            {
                var rootStep = timingSession.Timings.FirstOrDefault(t => t.ParentId == timingSession.Id);
                if (rootStep == null) return;

                ProfilingSessionContainer.CurrentSessionStepId = rootStep.Id;
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
        public static ICollection<IProfilingFilter> ProfilingFilters { get; private set; }

        /// <summary>
        /// Gets or sets a circular buffer for latest profiling sessions.
        /// </summary>
        public static ICircularBuffer<ITimingSession> CircularBuffer { get; set; }

        /// <summary>
        /// Default handler for creating a profiler.
        /// </summary>
        internal static Func<string, IProfilingStorage, TagCollection, IProfiler> CreateProfilerHandler = (name, storage, tags) => new Profiler(name, storage, tags);

        /// <summary>
        /// Default handler for handling exception.
        /// </summary>
        internal static Action<Exception, object> HandleExceptionHandler = HandleException;

        #endregion

        #region Constructors

        static ProfilingSession()
        {
            // by default, use CallContextProfilingSessionContainer
            _profilingSessionContainer = new CallContextProfilingSessionContainer();

            // by default, use JsonProfilingStorage
            _profilingStorage = new NoOperationProfilingStorage();

            // intialize filters
            ProfilingFilters = new ProfilingFilterList(new List<IProfilingFilter>());

            InitializeConfigurationFromConfig();
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

            // set null the current profiling session if exists
            ProfilingSession.SetCurrentProfilingSession(null);

            if (ProfilingFilters.Count > 0)
            {
                foreach (var filter in ProfilingFilters)
                {
                    if (filter == null) continue;
                    if (filter.ShouldBeExculded(name, tags)) return;
                }
            }

            IProfiler profiler = null;
            try
            {
                profiler = CreateProfilerHandler(name, _profilingStorage, tags == null || !tags.Any() ? null : new TagCollection(tags));
            }
            catch (Exception ex)
            {
                HandleExceptionHandler(ex, typeof(ProfilingSession));
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
            if (profilingSession != null)
            {
                try
                {
                    if (CircularBuffer != null)
                    {
                        CircularBuffer.Add(profilingSession.Profiler.GetTimingSession());
                    }

                    profilingSession._profiler.Stop(discardResults);
                }
                catch (Exception ex)
                {
                    HandleExceptionHandler(ex, typeof(ProfilingSession));
                }
            }

            // Clear the current profiling session on stopping
            _profilingSessionContainer.Clear();
        }

        #endregion

        #region Non-Public Methods

        private static void HandleException(Exception ex, object origin)
        {
            Logger.Error(ex, "Unexpected exception thrown from {0}: {1}", origin, ex.Message);
        }

        private static void InitializeConfigurationFromConfig()
        {
            var configSection = ConfigurationManager.GetSection("nanoprofiler");
            if (configSection != null && !(configSection is NanoProfilerConfigurationSection))
            {
                throw new ConfigurationErrorsException("Invalid configuration, check the 'nanoprofiler' configuration section.");
            }
            if (configSection == null) return;

            var nanoProfilerConfig = (configSection as NanoProfilerConfigurationSection);
            var providerType = Type.GetType(nanoProfilerConfig.Provider, true);
            var provider = (IConfigurationProvider)Activator.CreateInstance(providerType);
            ProfilingStorage = provider.Storage;
            if (provider.Filters != null)
            {
                foreach (var filter in provider.Filters)
                {
                    ProfilingFilters.Add(filter);
                }
            }
            CircularBuffer = provider.CircularBuffer;
        }

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns></returns>
        internal IDisposable StepImpl(string name, string[] tags)
        {
            IProfilingStep step = null;

            try
            {
                step = _profiler.Step(name, tags == null || !tags.Any() ? null : new TagCollection(tags));
            }
            catch (Exception ex)
            {
                HandleExceptionHandler(ex, this);
            }

            return step;
        }

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <returns></returns>
        internal IDisposable IgnoreImpl()
        {
            IDisposable ignoredStep = null;
            try
            {
                ignoredStep = _profiler.Ignore();
            }
            catch (Exception ex)
            {
                HandleExceptionHandler(ex, this);
            }

            return ignoredStep;
        }

        /// <summary>
        /// Add a tag to current profiling session.
        /// </summary>
        /// <param name="tag"></param>
        internal void AddTagImpl(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;

            if (Profiler == null) return;

            var session = Profiler.GetTimingSession();
            if (session == null) return;

            if (session.Tags == null) session.Tags = new TagCollection();
            session.Tags.Add(tag);
        }

        /// <summary>
        /// Add a custom data field to current profiling session.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void AddFieldImpl(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            if (Profiler == null) return;

            var session = Profiler.GetTimingSession();
            if (session == null) return;

            if (session.Data == null) session.Data = new Dictionary<string, string>();
            session.Data[key] = value;
        }

        #endregion
    }
}
