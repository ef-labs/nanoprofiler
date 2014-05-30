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
using System.Globalization;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// Represents a DB parameter of <see cref="DbTiming"/>.
    /// </summary>
    public class DbTimingParameter
    {
        /// <summary>
        /// The <see cref="DbType"/>.
        /// </summary>
        public DbType DbType;

        /// <summary>
        /// The <see cref="ParameterDirection"/>.
        /// </summary>
        public ParameterDirection Direction;

        /// <summary>
        /// Whether or not the value is nullable.
        /// </summary>
        public bool IsNullable;

        /// <summary>
        /// The parameter name.
        /// </summary>
        public string ParameterName;

        /// <summary>
        /// The parameter value.
        /// </summary>
        public string Value;

        /// <summary>
        /// The parameter size.
        /// </summary>
        public int Size;

        #region Constructors
        
        /// <summary>
        /// Initializes a <see cref="DbTimingParameter"/> from a <see cref="IDataParameter"/>.
        /// </summary>
        /// <param name="dataParameter">The <see cref="IDataParameter"/>.</param>
        public DbTimingParameter(IDataParameter dataParameter)
        {
            if (dataParameter != null)
            {
                DbType = dataParameter.DbType;
                Direction = dataParameter.Direction;
                IsNullable = dataParameter.IsNullable;
                ParameterName = dataParameter.ParameterName;
                Value = GetDataParameterValue(dataParameter);
                Size = GetDataParameterSize(dataParameter);
            }
        }

        #endregion

        #region Private Methods

        private static int GetDataParameterSize(IDataParameter parameter)
        {
            var dbDataParameter = parameter as IDbDataParameter;
            if (dbDataParameter != null)
            {
                return dbDataParameter.Size;
            }

            return 0;
        }

        private static string GetDataParameterValue(IDataParameter parameter)
        {
            object rawValue = parameter.Value;
            if ((rawValue == null) || (rawValue == DBNull.Value))
            {
                return null;
            }

            if (parameter.DbType == DbType.Binary)
            {
                var bytes = rawValue as byte[];
                if ((bytes != null) && (bytes.Length <= 0x200))
                {
                    return ("0x" + BitConverter.ToString(bytes).Replace("-", string.Empty));
                }
                return null;
            }

            if (rawValue is DateTime)
            {
                var dtValue = (DateTime)rawValue;
                return dtValue.ToString("s", CultureInfo.InvariantCulture);
            }

            var rawType = rawValue.GetType();
            if (rawType.IsEnum)
            {
                return Convert.ChangeType(rawValue, Enum.GetUnderlyingType(rawType)).ToString();
            }

            return rawValue.ToString();
        }

        #endregion
    }
}
