using System.Collections.Generic;

namespace EF.Diagnostics.Profiling.ProfilingFilters
{
    /// <summary>
    /// A predefined filter for disabling any profiling.
    /// </summary>
    public sealed class DisableProfilingFilter : IProfilingFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a <see cref="DisableProfilingFilter"/>.
        /// </summary>
        /// <param name="notUsed">The substr to check contains.</param>
        public DisableProfilingFilter(string notUsed = null)
        {
        }

        #endregion

        #region IProfilingFilter Members

        bool IProfilingFilter.ShouldBeExculded(string name, IEnumerable<string> tags)
        {
            return true;
        }

        #endregion
    }
}
