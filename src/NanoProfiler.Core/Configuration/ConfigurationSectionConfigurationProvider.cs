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
using System.Text.RegularExpressions;
using EF.Diagnostics.Profiling.ProfilingFilters;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling.Configuration
{
    internal sealed class ConfigurationSectionConfigurationProvider : IConfigurationProvider
    {
        #region Constructors

        public ConfigurationSectionConfigurationProvider()
        {
            var nanoProfilerConfig = ConfigurationManager.GetSection("nanoprofiler") as NanoProfilerConfigurationSection;
            if (nanoProfilerConfig == null) return;

            // load filters
            var filters = nanoProfilerConfig.Filters;
            if (filters != null)
            {
                var filterList = new List<IProfilingFilter>();

                foreach (ProfilingFilterElement filter in filters)
                {
                    if (string.IsNullOrWhiteSpace(filter.Type) ||
                        string.Equals(filter.Type, "contain", StringComparison.OrdinalIgnoreCase))
                    {
                        filterList.Add(new NameContainsProfilingFilter(filter.Value));
                    }
                    else if (string.Equals(filter.Type, "regex", StringComparison.OrdinalIgnoreCase))
                    {
                        filterList.Add(new RegexProfilingFilter(new Regex(filter.Value, RegexOptions.Compiled | RegexOptions.IgnoreCase)));
                    }
                    else
                    {
                        var filterType = Type.GetType(filter.Type, true);
                        if (!typeof(IProfilingFilter).IsAssignableFrom(filterType))
                        {
                            throw new ConfigurationErrorsException("Invalid type name: " + filter.Type);
                        }

                        try
                        {
                            filterList.Add((IProfilingFilter)Activator.CreateInstance(filterType, new object[] { filter.Value }));
                        }
                        catch (Exception ex)
                        {
                            throw new ConfigurationErrorsException("Invalid type name: " + filter.Type, ex);
                        }
                    }
                }

                Filters = filterList;
            }

            // set ProfilingStorage
            if (!string.IsNullOrEmpty(nanoProfilerConfig.Storage))
            {
                var type = Type.GetType(nanoProfilerConfig.Storage, true);
                Storage = Activator.CreateInstance(type) as IProfilingStorage;
            }

            // set CircularBuffer
            if (nanoProfilerConfig.CircularBufferSize > 0)
            {
                CircularBuffer = new CircularBuffer<ITimingSession>(nanoProfilerConfig.CircularBufferSize);
            }
        }

        #endregion

        #region IConfigurationProvider Members

        public IProfilingStorage Storage { get; private set; }

        public IEnumerable<IProfilingFilter> Filters { get; private set; }

        public ICircularBuffer<ITimingSession> CircularBuffer { get; private set; }

        #endregion
    }
}
