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
using System.Data.Common;
using System.Security;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// A <see cref="DbProviderFactory"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbProviderFactory : DbProviderFactory
    {
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly IDbProfiler _dbProfiler;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbProviderFactory"/>.
        /// </summary>
        /// <param name="dbProviderFactory">The <see cref="DbProviderFactory"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        public ProfiledDbProviderFactory(DbProviderFactory dbProviderFactory, IDbProfiler dbProfiler)
        {
            if (dbProviderFactory == null)
            {
                throw new ArgumentNullException("dbProviderFactory");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }

            _dbProviderFactory = dbProviderFactory;
            _dbProfiler = dbProfiler;
        }

        #endregion

        #region DbProviderFactory Members

        /// <summary>
        /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection. 
        /// </summary>
        /// <returns>Returns the created <see cref="DbCommand"/></returns>
        public override DbCommand CreateCommand()
        {
            var command = _dbProviderFactory.CreateCommand();
            if (command == null)
            {
                return null;
            }

            var profiledCommand = command as ProfiledDbCommand;
            if (profiledCommand != null)
            {
                return profiledCommand;
            }

            return new ProfiledDbCommand(command, _dbProfiler);
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbCommandBuilder"/> class. 
        /// </summary>
        /// <returns></returns>
        public override DbCommandBuilder CreateCommandBuilder()
        {
            return _dbProviderFactory.CreateCommandBuilder();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the DbConnection class. 
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            var connection = _dbProviderFactory.CreateConnection();
            if (connection == null)
            {
                return null;
            }

            var profiledConnection = connection as ProfiledDbConnection;
            if (profiledConnection != null)
            {
                return profiledConnection;
            }

            return new ProfiledDbConnection(connection, _dbProfiler);
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbConnectionStringBuilder"/> class. 
        /// </summary>
        /// <returns></returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return _dbProviderFactory.CreateConnectionStringBuilder();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbDataAdapter"/> class. 
        /// </summary>
        /// <returns></returns>
        public override DbDataAdapter CreateDataAdapter()
        {
            var dataAdapter = _dbProviderFactory.CreateDataAdapter();
            if (dataAdapter == null)
            {
                return null;
            }

            var profiledDataAdapter = dataAdapter as ProfiledDbDataAdapter;
            if (profiledDataAdapter != null)
            {
                return profiledDataAdapter;
            }

            return new ProfiledDbDataAdapter(dataAdapter, _dbProfiler);
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the DbDataSourceEnumerator class. 
        /// </summary>
        /// <returns></returns>
        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return _dbProviderFactory.CreateDataSourceEnumerator();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbParameter"/> class. 
        /// </summary>
        /// <returns></returns>
        public override DbParameter CreateParameter()
        {
            return _dbProviderFactory.CreateParameter();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the provider's version of the <see cref="CodeAccessPermission"/> class. 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public override System.Security.CodeAccessPermission CreatePermission(System.Security.Permissions.PermissionState state)
        {
            return _dbProviderFactory.CreatePermission(state);
        }

        /// <summary>
        /// Specifies whether the specific <see cref="DbProviderFactory"/> supports the <see cref="DbDataSourceEnumerator"/> class. 
        /// </summary>
        public override bool CanCreateDataSourceEnumerator
        {
            get { return _dbProviderFactory.CanCreateDataSourceEnumerator; }
        }

        #endregion
    }
}
