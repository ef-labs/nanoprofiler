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

using System.Configuration;
using EF.Diagnostics.Profiling.Storages;

namespace EF.Diagnostics.Profiling.Configuration
{
    /// <summary>
    /// The configuration section for nanoprofiler.
    /// </summary>
    public sealed class NanoProfilerConfigurationSection : ConfigurationSection
    {
        private const string ProviderPropertyName = "provider";
        private static readonly ConfigurationProperty PropProvider = new ConfigurationProperty(ProviderPropertyName, typeof(string), typeof(ConfigurationSectionConfigurationProvider).AssemblyQualifiedName);
        private const string ProfilingFilterElementCollectionName = "filters";
        private static readonly ConfigurationProperty PropFilters = new ConfigurationProperty(ProfilingFilterElementCollectionName, typeof(ProfilingFilterElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private const string ProfilingStoragePropertyName = "storage";
        private static readonly ConfigurationProperty PropStorage = new ConfigurationProperty(ProfilingStoragePropertyName, typeof(string), typeof(NoOperationProfilingStorage).AssemblyQualifiedName);
        private const string CircularBufferSizePropertyName = "circularBufferSize";
        private static readonly ConfigurationProperty PropCircularBufferSize = new ConfigurationProperty(CircularBufferSizePropertyName, typeof(int), 100);
        private static readonly ConfigurationPropertyCollection Props = new ConfigurationPropertyCollection();

        static NanoProfilerConfigurationSection()
        {
            Props.Add(PropFilters);
            Props.Add(PropStorage);
            Props.Add(PropCircularBufferSize);
            Props.Add(PropProvider);
        }

        /// <summary>
        /// Profiling configuration provider type.
        /// </summary>
        [ConfigurationProperty(ProviderPropertyName)]
        public string Provider
        {
            get { return (string)base[PropProvider]; }
        }

        /// <summary>
        /// Profiling filters.
        /// </summary>
        [ConfigurationProperty(ProfilingFilterElementCollectionName, IsDefaultCollection = true)]
        public ProfilingFilterElementCollection Filters
        {
            get { return (ProfilingFilterElementCollection)base[PropFilters]; }
        }

        /// <summary>
        /// Profiling storage type.
        /// </summary>
        [ConfigurationProperty(ProfilingStoragePropertyName)]
        public string Storage
        {
            get { return (string)base[PropStorage]; }
        }

        /// <summary>
        /// Latest profiling sessions circular buffer size.
        /// </summary>
        [ConfigurationProperty(CircularBufferSizePropertyName)]
        public int CircularBufferSize
        {
            get { return (int)base[PropCircularBufferSize]; }
        }

        /// <summary>
        /// Gets configuration properties.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return Props;
            }
        }
    }
}
