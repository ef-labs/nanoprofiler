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
using System.Data;
using System.Data.Common;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// A <see cref="IDbDataAdapter"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbDataAdapter : DbDataAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes an <see cref="ProfiledDbDataAdapter"/>.
        /// </summary>
        /// <param name="dataAdapter">The <see cref="IDbDataAdapter"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        public ProfiledDbDataAdapter(IDbDataAdapter dataAdapter, IDbProfiler dbProfiler)
        {
            if (dataAdapter == null)
            {
                throw new ArgumentNullException("dataAdapter");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }
            
            if (dataAdapter.SelectCommand != null)
            {
                var profiledSelectCommand = dataAdapter.SelectCommand as ProfiledDbCommand;
                if (profiledSelectCommand != null)
                {
                    SelectCommand = profiledSelectCommand;
                }
                else
                {
                    SelectCommand = new ProfiledDbCommand(dataAdapter.SelectCommand, dbProfiler);
                }
            }

            if (dataAdapter.InsertCommand != null)
            {
                var profiledInsertCommand = dataAdapter.InsertCommand as ProfiledDbCommand;
                if (profiledInsertCommand != null)
                {
                    InsertCommand = profiledInsertCommand;
                }
                else
                {
                    InsertCommand = new ProfiledDbCommand(dataAdapter.InsertCommand, dbProfiler);
                }
            }

            if (dataAdapter.UpdateCommand != null)
            {
                var profiledUpdateCommand = dataAdapter.UpdateCommand as ProfiledDbCommand;
                if (profiledUpdateCommand != null)
                {
                    UpdateCommand = profiledUpdateCommand;
                }
                else
                {
                    UpdateCommand = new ProfiledDbCommand(dataAdapter.UpdateCommand, dbProfiler);
                }
            }

            if (dataAdapter.DeleteCommand != null)
            {
                var profiledDeleteCommand = dataAdapter.DeleteCommand as ProfiledDbCommand;
                if (profiledDeleteCommand != null)
                {
                    DeleteCommand = profiledDeleteCommand;
                }
                else
                {
                    DeleteCommand = new ProfiledDbCommand(dataAdapter.DeleteCommand, dbProfiler);
                }
            }
        }

        #endregion
    }
}
