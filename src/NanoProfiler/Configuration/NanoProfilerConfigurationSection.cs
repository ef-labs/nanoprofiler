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

using System.Configuration;

namespace EF.Diagnostics.Profiling.Configuration
{
    /// <summary>
    /// The configuration section for nanoprofiler.
    /// </summary>
    public sealed class NanoProfilerConfigurationSection : ConfigurationSection
    {
        private const string ProfilingFilterElementCollectionName = "filters";
        private static readonly ConfigurationProperty PropFilters = new ConfigurationProperty(ProfilingFilterElementCollectionName, typeof(ProfilingFilterElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationPropertyCollection Props = new ConfigurationPropertyCollection();

        static NanoProfilerConfigurationSection()
        {
            Props.Add(PropFilters);
        }

        /// <summary>
        /// Profiling filters.
        /// </summary>
        [ConfigurationProperty(ProfilingFilterElementCollectionName, IsDefaultCollection = true)]
        public ProfilingFilterElementCollection Filters
        {
            get { return (ProfilingFilterElementCollection) base[PropFilters]; }
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
