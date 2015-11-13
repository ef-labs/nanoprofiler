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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EF.Diagnostics.Profiling.ProfilingFilters
{
    internal sealed class ProfilingFilterList : ICollection<IProfilingFilter>
    {
        private readonly List<IProfilingFilter> _innerList;

        public ProfilingFilterList(List<IProfilingFilter> innerList)
        {
            _innerList = innerList;
        }

        #region ICollection<IProfilingFilter> Members

        public IEnumerator<IProfilingFilter> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public void Add(IProfilingFilter item)
        {
            // ignore duplicated NameContainsProfilingFilter
            var nameContains = item as NameContainsProfilingFilter;
            if (nameContains != null && _innerList.Any(i => i is NameContainsProfilingFilter && ((NameContainsProfilingFilter)i).SubString == nameContains.SubString))
            {
                return;
            }

            // ignore duplicated RegexProfilingFilter
            var regex = item as RegexProfilingFilter;
            if (regex != null && _innerList.Any(i => i is RegexProfilingFilter && ((RegexProfilingFilter)i).RegexString == regex.RegexString))
            {
                return;
            }

            _innerList.Add(item);
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(IProfilingFilter item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(IProfilingFilter[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(IProfilingFilter item)
        {
            return _innerList.Remove(item);
        }

        public int Count
        {
            get { return _innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }
}
