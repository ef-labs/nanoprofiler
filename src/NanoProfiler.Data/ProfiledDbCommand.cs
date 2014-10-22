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
using System.Data;
using System.Data.Common;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// A <see cref="IDbCommand"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbCommand : DbCommand
    {
        private readonly IDbCommand _command;
        private readonly DbCommand _dbCommand;
        private readonly IDbProfiler _dbProfiler;
        private List<string> _tags;
        private DbConnection _dbConnection;
        private DbParameterCollection _dbParameterCollection;
        private DbTransaction _dbTransaction;

        #region Properties

        /// <summary>
        /// Gets the tags of the <see cref="DbTiming"/> which will be created internally.
        /// </summary>
        public ICollection<string> Tags
        {
            get { return _tags ?? (_tags = new List<string>()); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="IDbCommand"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public ProfiledDbCommand(IDbCommand command, IDbProfiler dbProfiler, IEnumerable<string> tags = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }

            _command = command;
            _dbCommand = command as DbCommand;
            _dbProfiler = dbProfiler;

            if (tags != null)
            {
                _tags = new List<string>(tags);
            }
        }

        #endregion

        #region DbCommand Members

        /// <summary>
        /// Attempts to cancels the execution of a <see cref="DbCommand"/>.
        /// </summary>
        public override void Cancel()
        {
            _command.Cancel();
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source. 
        /// </summary>
        public override string CommandText
        {
            get
            {
                return _command.CommandText;
            }
            set
            {
                _command.CommandText = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error. 
        /// </summary>
        public override int CommandTimeout
        {
            get
            {
                return _command.CommandTimeout;
            }
            set
            {
                _command.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Indicates or specifies how the <see cref="CommandText"/> property is interpreted. 
        /// </summary>
        public override CommandType CommandType
        {
            get
            {
                return _command.CommandType;
            }
            set
            {
                _command.CommandType = value;
            }
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DbParameter"/> object. 
        /// </summary>
        /// <returns>Returns the created <see cref="DbParameter"/>.</returns>
        protected override DbParameter CreateDbParameter()
        {
            if (_dbCommand != null)
            {
                return _dbCommand.CreateParameter();
            }

            return new DbParameterWrapper(_command.CreateParameter());
        }

        /// <summary>
        /// Gets or sets the <see cref="DbConnection"/> used by this DbCommand. 
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                if (_command.Connection == null)
                {
                    return null;
                }

                if (_dbConnection == null)
                {

                    var profiledDbConnection = _command.Connection as ProfiledDbConnection;
                    if (profiledDbConnection != null)
                    {
                        _dbConnection = profiledDbConnection;
                    }
                    else
                    {
                        _dbConnection = new ProfiledDbConnection(_command.Connection, _dbProfiler);
                    }
                }

                return _dbConnection;
            }
            set
            {
                _dbConnection = value;
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="DbParameter"/> objects. 
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                if (_command.Parameters == null)
                {
                    return null;
                }

                if (_dbParameterCollection == null)
                {
                    if (_dbCommand != null)
                    {
                        _dbParameterCollection = _dbCommand.Parameters;
                    }
                    else
                    {
                        _dbParameterCollection = new DbParameterCollectionWrapper(_command.Parameters);
                    }
                }

                return _dbParameterCollection;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DbTransaction"/> within which this <see cref="DbCommand"/> object executes. 
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get
            {
                if (_command.Transaction == null)
                {
                    return null;
                }

                if (_dbTransaction == null)
                {
                    var profiledDbTransaction = _command.Transaction as ProfiledDbTransaction;
                    if (profiledDbTransaction != null)
                    {
                        _dbTransaction = profiledDbTransaction;
                    }
                    else
                    {
                        _dbTransaction = new ProfiledDbTransaction(_command.Transaction, _dbProfiler);
                    }
                }

                return _dbTransaction;
            }
            set
            {
                _dbTransaction = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command object should be visible in a customized interface control. 
        /// </summary>
        public override bool DesignTimeVisible
        {
            get
            {
                if (_dbCommand != null)
                {
                    return _dbCommand.DesignTimeVisible;
                }

                return false;
            }
            set
            {
                if (_dbCommand != null)
                {
                    _dbCommand.DesignTimeVisible = value;
                }
            }
        }

        /// <summary>
        /// Executes the command text against the connection. 
        /// </summary>
        /// <param name="behavior">The <see cref="CommandBehavior"/>.</param>
        /// <returns></returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            IDataReader reader = null;
            _dbProfiler.ExecuteDbCommand(
                DbExecuteType.Reader, _command, () => reader = _command.ExecuteReader(behavior), _tags);

            var profiledReader = reader as ProfiledDbDataReader;
            if (profiledReader != null)
            {
                return profiledReader;
            }

            return new ProfiledDbDataReader(reader, _dbProfiler);
        }

        /// <summary>
        /// Executes a SQL statement against a connection object. 
        /// </summary>
        /// <returns>Returns The number of rows affected. </returns>
        public override int ExecuteNonQuery()
        {
            int affected = 0;
            _dbProfiler.ExecuteDbCommand(
                DbExecuteType.NonQuery, _command, () => { affected = _command.ExecuteNonQuery(); return null; }, _tags);
            return affected;
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored. 
        /// </summary>
        /// <returns>The first column of the first row in the result set. </returns>
        public override object ExecuteScalar()
        {
            object returnValue = null;
            _dbProfiler.ExecuteDbCommand(
                DbExecuteType.Scalar, _command, () => { returnValue = _command.ExecuteScalar(); return null; }, _tags);
            return returnValue;
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public override void Prepare()
        {
            _command.Prepare();
        }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="DataRow"/> when used by the Update method of a <see cref="DbDataAdapter"/>. 
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return _command.UpdatedRowSource;
            }
            set
            {
                _command.UpdatedRowSource = value;
            }
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
        /// Releases the unmanaged resources used by the <see cref="ProfiledDbCommand"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _command.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
