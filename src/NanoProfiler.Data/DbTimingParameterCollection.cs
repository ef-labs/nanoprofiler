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

using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// Represents a collection of <see cref="DbTimingParameter"/>s.
    /// </summary>
    public class DbTimingParameterCollection : List<DbTimingParameter>
    {
        /// <summary>
        /// Gets the size of all the parameters.
        /// </summary>
        public int Size
        {
            get
            {
                return this.Sum(parameter => parameter.Size);
            }
        }

        #region Constructors
        
        /// <summary>
        /// Initializes a <see cref="DbTimingParameterCollection"/> from <see cref="IDataParameterCollection"/>.
        /// </summary>
        /// <param name="dataParameterCollection">The <see cref="IDataParameterCollection"/>.</param>
        public DbTimingParameterCollection(IDataParameterCollection dataParameterCollection)
        {
            if (dataParameterCollection != null)
            {
                foreach (IDataParameter dataParameter in dataParameterCollection)
                {
                    Add(new DbTimingParameter(dataParameter));
                }
            }
        }

        #endregion
    }
}
