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
using System.Globalization;
using System.IO;
using System.Xml;

using EF.Diagnostics.Profiling.Timing;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// Represents a DB timing of a <see cref="IDbCommand"/> execution.
    /// </summary>
    public sealed class DbTiming : CustomTiming
    {
        private readonly IProfiler _profiler;

        /// <summary>
        /// Gets the <see cref="DbExecuteType"/>.
        /// </summary>
        public DbExecuteType DbExecuteType { get; private set; }

        /// <summary>
        /// The parameters of the <see cref="IDbCommand"/> being profiled.
        /// </summary>
        public DbTimingParameterCollection Parameters { get; private set; }

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="DbTiming"/>.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the <see cref="CustomTiming"/> to when stops.
        /// </param>
        /// <param name="executeType">The <see cref="DbExecuteType"/> of the <see cref="IDbCommand"/> being executed &amp; profiled.</param>
        /// <param name="command">The <see cref="IDbCommand"/> being executed &amp; profiled.</param>
        public DbTiming(
            IProfiler profiler, DbExecuteType executeType, IDbCommand command)
            : base(profiler, "db", command == null ? null : command.CommandText)
        {
            _profiler = profiler;
            DbExecuteType = executeType;
            ExecuteType = executeType.ToString().ToLowerInvariant();
            if (command != null)
            {
                Parameters = new DbTimingParameterCollection(command.Parameters);
                InputSize = Parameters.Size;
                InputData = ToXml(Parameters);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the first fetch milliseconds of this DB operation.
        /// </summary>
        public void FirstFetch()
        {
            OutputStartMilliseconds = _profiler.DurationMilliseconds - StartMilliseconds;
        }

        /// <summary>
        /// Stops the current DB timing.
        /// </summary>
        public void Stop()
        {
            DurationMilliseconds = _profiler.DurationMilliseconds - StartMilliseconds;
            if (!OutputStartMilliseconds.HasValue)
            {
                OutputStartMilliseconds = DurationMilliseconds;
            }

            _profiler.AddCustomTiming(this);
        }

        #endregion

        #region Private Methods

        private static string ToXml(IEnumerable<DbTimingParameter> parameters)
        {
            using (var memoryStream = new MemoryStream())
            {
                var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };
                using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                {
                    xmlWriter.WriteStartElement("Params");
                    foreach (var parameter in parameters)
                    {
                        xmlWriter.WriteStartElement("Param");
                        xmlWriter.WriteAttributeString("name", parameter.ParameterName);
                        xmlWriter.WriteAttributeString("type", parameter.DbType.ToString());
                        xmlWriter.WriteAttributeString("direction", parameter.Direction.ToString());
                        xmlWriter.WriteAttributeString("size", parameter.Size.ToString(CultureInfo.InvariantCulture));
                        xmlWriter.WriteAttributeString("nullable", parameter.IsNullable ? "true" : "false");
                        xmlWriter.WriteStartElement("Value");
                        xmlWriter.WriteString(parameter.Value);
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(memoryStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        #endregion
    }
}
