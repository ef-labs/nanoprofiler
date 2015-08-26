using System.Configuration;

namespace EF.Diagnostics.Profiling.Configuration
{
    /// <summary>
    /// A profiling filter element.
    /// </summary>
    public sealed class ProfilingFilterElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty PropKey = new ConfigurationProperty("key", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty PropValue = new ConfigurationProperty("value", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty PropType = new ConfigurationProperty("type", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationPropertyCollection Props = new ConfigurationPropertyCollection();

        #region Constructors

        static ProfilingFilterElement()
        {
            Props.Add(PropKey);
            Props.Add(PropValue);
            Props.Add(PropType);
        }

        /// <summary>
        /// Initializes a <see cref="ProfilingFilterElement"/>.
        /// </summary>
        public ProfilingFilterElement()
        {
        }

        /// <summary>
        /// Initializes a <see cref="ProfilingFilterElement"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public ProfilingFilterElement(string key, string value, string type)
        {
            Key = key;
            Value = value;
            Type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key
        {
            get { return (string)base[PropKey]; }
            set { base[PropKey] = value; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value
        {
            get { return (string)base[PropValue]; }
            set { base[PropValue] = value; }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public string Type
        {
            get { return (string)base[PropType]; }
            set { base[PropType] = value; }
        }

        #endregion

        #region Non-Public Members

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

        #endregion
    }
}
