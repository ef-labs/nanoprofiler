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

namespace EF.Diagnostics.Profiling.ProfilingFilters
{
    /// <summary>
    /// An <see cref="IProfilingFilter"/> implementation for ignoring profiling is name contains specified substr.
    /// </summary>
    public sealed class NameContainsProfilingFilter : IProfilingFilter
    {
        private readonly string _substr;

        /// <summary>
        /// The sub string to be checked for contains.
        /// </summary>
        public string SubString { get { return _substr; } }

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="NameContainsProfilingFilter"/>.
        /// </summary>
        /// <param name="substr">The substr to check contains.</param>
        public NameContainsProfilingFilter(string substr)
        {
            _substr = substr;
        }

        #endregion

        #region IProfilingFilter Members

        bool IProfilingFilter.ShouldBeExculded(string name, IEnumerable<string> tags)
        {
            if (name == null)
            {
                return true;
            }

            return name.IndexOf(_substr, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion
    }
}
