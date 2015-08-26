using System;
using System.Configuration;

namespace EF.Diagnostics.Profiling.Configuration
{
    /// <summary>
    /// A configuration element collection for profiling filters.
    /// </summary>
    public sealed class ProfilingFilterElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Initializes a <see cref="ProfilingFilterElementCollection"/>.
        /// </summary>
        public ProfilingFilterElementCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Adds a profileing filter element.
        /// </summary>
        /// <param name="element">The filter element to be added.</param>
        public void Add(ProfilingFilterElement element)
        {
            BaseAdd(element);
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        /// <summary>
        /// Removes a profileing filter element.
        /// </summary>
        /// <param name="element">The filter element to be removed.</param>
        public void Remove(ProfilingFilterElement element)
        {
            BaseRemove(element);
        }

        /// <summary>
        /// Gets a profiling filter element.
        /// </summary>
        /// <param name="elementKey">The element key.</param>
        /// <returns></returns>
        public ProfilingFilterElement Get(string elementKey)
        {
            return (ProfilingFilterElement)BaseGet(elementKey);
        }

        /// <summary>
        /// Creates new configuration element.
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProfilingFilterElement();
        }

        /// <summary>
        /// Gets configuration element key.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProfilingFilterElement)element).Key;
        }
    }
}
