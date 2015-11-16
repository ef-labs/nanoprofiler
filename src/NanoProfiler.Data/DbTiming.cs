/*
    The MIT License (MIT)
    Copyright © 2015 Englishtown <opensource@englishtown.com>

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
using System.Globalization;
using System.IO;
using System.Text;
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// Represents a DB timing of a <see cref="IDbCommand"/> execution.
    /// </summary>
    public sealed class DbTiming : Timing
    {
        private readonly IProfiler _profiler;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="DbTiming"/>.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the timing to when stops.
        /// </param>
        /// <param name="executeType">The <see cref="DbExecuteType"/> of the <see cref="IDbCommand"/> being executed &amp; profiled.</param>
        /// <param name="command">The <see cref="IDbCommand"/> being executed &amp; profiled.</param>
        public DbTiming(
            IProfiler profiler, DbExecuteType executeType, IDbCommand command)
            : base(profiler, "db", ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId, command == null ? null : command.CommandText, null)
        {
            if (profiler == null) throw new ArgumentNullException("profiler");
            if (command == null) throw new ArgumentNullException("command");

            _profiler = profiler;
            StartMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds;
            Sort = profiler.Elapsed.Ticks;
            Data = new Dictionary<string, string>();

            Data["executeType"] = executeType.ToString().ToLowerInvariant();

            if (command.Parameters == null || command.Parameters.Count == 0) return;

            Data["parameters"] = SerializeParameters(command.Parameters);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the first fetch milliseconds of this DB operation.
        /// </summary>
        public void FirstFetch()
        {
            Data["readStart"] = ((long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Stops the current DB timing.
        /// </summary>
        public void Stop()
        {
            DurationMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds;
            if (!Data.ContainsKey("readStart"))
            {
                Data["readStart"] = DurationMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            _profiler.GetTimingSession().AddTiming(this);
        }

        #endregion

        #region Private Methods

        private static string SerializeParameters(IDataParameterCollection parameters)
        {
            var sb = new StringBuilder();

            foreach (IDataParameter parameter in parameters)
            {
                sb.Append(parameter.ParameterName);
                sb.Append("(");
                sb.Append(parameter.DbType.ToString());
                sb.Append(", ");
                sb.Append(parameter.Direction.ToString());
                if (parameter.IsNullable)
                {
                    sb.Append(", nullable");
                }
                sb.Append("): ");
                sb.Append(parameter.Value == null || parameter.Value == DBNull.Value ? "NULL" : SerializeParameterValue(parameter.Value));
                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        private static string SerializeParameterValue(object value)
        {
            var table = value as DataTable;
            if (table == null)
            {
                return value.ToString();
            }

            if (string.IsNullOrEmpty(table.TableName))
            {
                table.TableName = "Table"; // ensure table name to avoid serialization error
            }

            // for DataTable, serialize and only take top 500 content
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                table.WriteXml(sw);
            }

            var text = sb.ToString();
            return text.Length > 500 ? text.Substring(0, 497) + "..." : text;
        }

        #endregion
    }
}
