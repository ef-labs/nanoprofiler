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
using System.Data;
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// Represents a generic DB profiler for profiling execution of <see cref="IDbCommand"/>.
    /// </summary>
    public interface IDbProfiler
    {
        /// <summary>
        /// Executes &amp; profiles the execution of the specified <see cref="IDbCommand"/>.
        /// </summary>
        /// <param name="executeType">The <see cref="DbExecuteType"/>.</param>
        /// <param name="command">The <see cref="IDbCommand"/> to be executed &amp; profiled.</param>
        /// <param name="execute">
        ///     The execute handler, 
        ///     which should return the <see cref="IDataReader"/> instance if it is an ExecuteReader operation.
        ///     If it is not ExecuteReader, it should return null.
        /// </param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        void ExecuteDbCommand(DbExecuteType executeType, IDbCommand command, Func<IDataReader> execute, TagCollection tags);

        /// <summary>
        /// Notifies the profiler that the data reader has finished reading
        /// so that the DB timing attached to the data reading could be stopped.
        /// </summary>
        /// <param name="dataReader">The <see cref="IDataReader"/>.</param>
        void DataReaderFinished(IDataReader dataReader);
    }
}
