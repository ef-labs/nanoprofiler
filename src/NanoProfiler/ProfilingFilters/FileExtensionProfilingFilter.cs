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

using System.Text;
using System.Text.RegularExpressions;

namespace EF.Diagnostics.Profiling.ProfilingFilters
{
    /// <summary>
    /// An <see cref="IProfilingFilter"/> implement for ignoring file requests by extension.
    /// </summary>
    public class FileExtensionProfilingFilter : RegexProfilingFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a <see cref="FileExtensionProfilingFilter"/>.
        /// </summary>
        /// <param name="extensions">One or many file extensions</param>
        public FileExtensionProfilingFilter(params string[] extensions)
            : base(CreateRegex(extensions))
        {
        }

        /// <summary>
        /// Initializes a <see cref="FileExtensionProfilingFilter"/>.
        /// </summary>
        /// <param name="fileExts">Separated file extentions.</param>
        public FileExtensionProfilingFilter(string fileExts)
            : this(fileExts.Split("|,;".ToCharArray()))
        {
        }

        private static Regex CreateRegex(string[] extensions)
        {
            if (extensions != null && extensions.Length > 0)
            {
                var sb = new StringBuilder();
                sb.Append("\\.(");
                var separator = "";
                foreach (var extension in extensions)
                {
                    sb.Append(separator);
                    sb.Append(extension.Trim(" .".ToCharArray()));
                    sb.Append("\\?|");
                    sb.Append(extension);
                    sb.Append("$");

                    separator = "|";
                }
                sb.Append(")");

                return new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            return null;
        }

        #endregion
    }
}
