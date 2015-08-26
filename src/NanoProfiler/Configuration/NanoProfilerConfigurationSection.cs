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
