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
