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

namespace EF.Diagnostics.Profiling.Configuration
{
    /// <summary>
    /// A profiling filter element.
    /// </summary>
    public sealed class ProfilingFilterElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty PropKey = new ConfigurationProperty("key", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty PropValue = new ConfigurationProperty("value", typeof(string), string.Empty);
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
