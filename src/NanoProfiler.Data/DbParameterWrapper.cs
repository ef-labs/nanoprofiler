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

using System.Data;
using System.Data.Common;

namespace EF.Diagnostics.Profiling.Data
{
    internal sealed class DbParameterWrapper : DbParameter
    {
        private readonly IDbDataParameter _parameter;
        private readonly DbParameter _dbParameter;

        #region Constructors

        public DbParameterWrapper(IDbDataParameter parameter)
        {
            _parameter = parameter;
            _dbParameter = parameter as DbParameter;
        }

        #endregion

        #region DbParameter Members

        public override DbType DbType
        {
            get
            {
                return _parameter.DbType;
            }
            set
            {
                _parameter.DbType = value;
            }
        }

        public override ParameterDirection Direction
        {
            get
            {
                return _parameter.Direction;
            }
            set
            {
                _parameter.Direction = value;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return _parameter.IsNullable;
            }
            set
            {
                if (_dbParameter != null)
                {
                    _dbParameter.IsNullable = value;
                }
            }
        }

        public override string ParameterName
        {
            get
            {
                return _parameter.ParameterName;
            }
            set
            {
                _parameter.ParameterName = value;
            }
        }

        public override void ResetDbType()
        {
            if (_dbParameter != null)
            {
                _dbParameter.ResetDbType();
            }
        }

        public override int Size
        {
            get
            {
                return _parameter.Size;
            }
            set
            {
                _parameter.Size = value;
            }
        }

        public override string SourceColumn
        {
            get
            {
                return _parameter.SourceColumn;
            }
            set
            {
                _parameter.SourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                if (_dbParameter != null)
                {
                    return _dbParameter.SourceColumnNullMapping;
                }

                return false;
            }
            set
            {
                if (_dbParameter != null)
                {
                    _dbParameter.SourceColumnNullMapping = value;
                }
            }
        }

        public override DataRowVersion SourceVersion
        {
            get
            {
                return _dbParameter.SourceVersion;
            }
            set
            {
                _dbParameter.SourceVersion = value;
            }
        }

        public override object Value
        {
            get
            {
                return _dbParameter.Value;
            }
            set
            {
                _dbParameter.Value = value;
            }
        }

        #endregion
    }
}
