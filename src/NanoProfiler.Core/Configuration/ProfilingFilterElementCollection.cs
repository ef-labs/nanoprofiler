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
