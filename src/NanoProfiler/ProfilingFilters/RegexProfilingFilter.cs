using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EF.Diagnostics.Profiling.ProfilingFilters
{
    /// <summary>
    /// An <see cref="IProfilingFilter"/> implement for ignoring profiling by regex matches.
    /// </summary>
    public class RegexProfilingFilter : IProfilingFilter
    {
        private readonly Regex _regex;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="RegexProfilingFilter"/>.
        /// </summary>
        /// <param name="regex">The <see cref="Regex"/></param>
        public RegexProfilingFilter(Regex regex)
        {
            _regex = regex;
        }

        #endregion

        #region IProfilingFilter Members

        /// <summary>
        /// Returns whether or not the profiling session should NOT be started.
        /// </summary>
        /// <param name="name">The name of the profiling session to be started.</param>
        /// <param name="tags">The tags of the profiling session to be started.</param>
        /// <returns>Returns true, if the profiling session should NOT be started, otherwise, returns false.</returns>
        public bool ShouldBeExculded(string name, IEnumerable<string> tags)
        {
            if (_regex == null)
            {
                return false;
            }

            if (name == null)
            {
                return true;
            }

            return _regex.IsMatch(name);
        }

        bool IProfilingFilter.ShouldBeExculded(string name, IEnumerable<string> tags)
        {
            return ShouldBeExculded(name, tags);
        }

        #endregion
    }
}
