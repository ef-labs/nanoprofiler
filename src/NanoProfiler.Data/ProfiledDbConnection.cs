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
    /// A <see cref="IDbConnection"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbConnection : DbConnection
    {
        private readonly IDbConnection _connection;
        private readonly DbConnection _dbConnection;
        private readonly IDbProfiler _dbProfiler;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbConnection"/>.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        public ProfiledDbConnection(IDbConnection connection, IDbProfiler dbProfiler)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }

            _connection = connection;
            _dbConnection = connection as DbConnection;
            if (_dbConnection != null)
            {
                _dbConnection.StateChange += StateChangeHandler;
            }
            _dbProfiler = dbProfiler;
        }

        #endregion

        #region DbConnection Members

        /// <summary>
        /// Starts a database transaction. 
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction. </param>
        /// <returns>An object representing the new transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var transaction = _connection.BeginTransaction(isolationLevel);
            var profiledTransaction = transaction as ProfiledDbTransaction;
            if (profiledTransaction != null)
            {
                return profiledTransaction;
            }

            return new ProfiledDbTransaction(transaction, _dbProfiler);
        }

        /// <summary>
        /// Changes the current database for an open connection. 
        /// </summary>
        /// <param name="databaseName">Specifies the name of the database for the connection to use.</param>
        public override void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection. 
        /// </summary>
        public override void Close()
        {
            _connection.Close();
        }

        /// <summary>
        /// Gets or sets the string used to open the connection. 
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                return _connection.ConnectionString;
            }
            set
            {
                _connection.ConnectionString = value;
            }
        }

        /// <summary>
        /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection. 
        /// </summary>
        /// <returns></returns>
        protected override DbCommand CreateDbCommand()
        {
            var command = _connection.CreateCommand();
            var profiledCommand = command as ProfiledDbCommand;
            if (profiledCommand != null)
            {
                return profiledCommand;
            }

            return new ProfiledDbCommand(command, _dbProfiler);
        }

        /// <summary>
        /// Gets the name of the database server to which to connect. 
        /// </summary>
        public override string DataSource
        {
            get
            {
                if (_dbConnection != null)
                {
                    return _dbConnection.DataSource;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the name of the current database after a connection is opened, or the database name specified in the connection string before the connection is opened. 
        /// </summary>
        public override string Database
        {
            get { return _connection.Database; }
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString. 
        /// </summary>
        public override void Open()
        {
            _connection.Open();
        }

        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected. 
        /// </summary>
        public override string ServerVersion
        {
            get
            {
                if (_dbConnection != null)
                {
                    return _dbConnection.ServerVersion;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a string that describes the state of the connection. 
        /// </summary>
        public override ConnectionState State
        {
            get { return _connection.State; }
        }

        /// <summary>
        /// Gets whether or not can raise events.
        /// </summary>
        protected override bool CanRaiseEvents
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the time to wait while establishing a connection before terminating the attempt and generating an error. 
        /// </summary>
        public override int ConnectionTimeout
        {
            get
            {
                return _connection.ConnectionTimeout;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProfiledDbConnection"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dbConnection != null)
                {
                    _dbConnection.StateChange -= StateChangeHandler;
                }

                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Enlists in the specified transaction. 
        /// </summary>
        /// <param name="transaction"></param>
        public override void EnlistTransaction(System.Transactions.Transaction transaction)
        {
            if (_dbConnection != null)
            {
                _dbConnection.EnlistTransaction(transaction);
            }
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="DbConnection"/>. 
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchema()
        {
            if (_dbConnection != null)
            {
                return _dbConnection.GetSchema();
            }

            return null;
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="DbConnection"/> using the specified string for the schema name. 
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public override DataTable GetSchema(string collectionName)
        {
            if (_dbConnection != null)
            {
                return _dbConnection.GetSchema(collectionName);
            }

            return null;
        }

        /// <summary>
        /// Returns schema information for the data source of this <see cref="DbConnection"/> using the specified string for the schema name and the specified string array for the restriction values. 
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="restrictionValues"></param>
        /// <returns></returns>
        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            if (_dbConnection != null)
            {
                return _dbConnection.GetSchema(collectionName, restrictionValues);
            }

            return null;
        }
 
        #endregion

        #region Private Methods

        private void StateChangeHandler(object sender, StateChangeEventArgs stateChangeEventArgs)
        {
            OnStateChange(stateChangeEventArgs);
        }

        #endregion
    }
}
