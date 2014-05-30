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
using System.Linq;

using EF.Diagnostics.Profiling.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EF.Diagnostics.Profiling.Tests.Data
{
    [TestClass]
    public class DbProfilerTest
    {
        [TestMethod]
        public void TestDbProfiler_ExecuteDbCommand_InvalidExecute()
        {
            var mockStorage = new Mock<IProfilingStorage>();
            var profiler = new Profiler("test", mockStorage.Object, null);
            var mockCommand = new Mock<IDbCommand>();
            var target = new DbProfiler(profiler) as IDbProfiler;

            // execute empty execute should not throw exception
            target.ExecuteDbCommand(DbExecuteType.Reader, mockCommand.Object, null, null);
        }

        [TestMethod]
        public void TestDbProfiler_ExecuteDbCommand_InvalidCommand()
        {
            var mockStorage = new Mock<IProfilingStorage>();
            var profiler = new Profiler("test", mockStorage.Object, null);
            var target = new DbProfiler(profiler) as IDbProfiler;

            var executeCalled = false;
            target.ExecuteDbCommand(DbExecuteType.Reader, null, () =>
            {
                executeCalled = true;
                return null;
            }, null);

            Assert.IsTrue(executeCalled);
        }

        [TestMethod]
        public void TestDbProfiler()
        {
            var mockStorage = new Mock<IProfilingStorage>();
            var profiler = new Profiler("test", mockStorage.Object, null);
            var target = new DbProfiler(profiler) as IDbProfiler;

            var stepId = Guid.NewGuid();
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = stepId;
            var executeCalled = false;
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(cmd => cmd.CommandText).Returns("test");
            var mockReader = new Mock<IDataReader>();

            // test execute reader
            target.ExecuteDbCommand(DbExecuteType.Reader, mockCommand.Object, () =>
            {
                executeCalled = true;
                return mockReader.Object;
            }, new[] { "tag1" });
            Assert.IsTrue(executeCalled);
            Assert.AreEqual(0, (profiler as IProfiler).CustomTimings.Count());
            target.DataReaderFinished(new ProfiledDbDataReader(mockReader.Object, target));
            Assert.AreEqual(1, (profiler as IProfiler).CustomTimings.Count());
            Assert.AreEqual("TAG1", (profiler as IProfiler).CustomTimings.Last().Tags.First());

            // test execute nonquery
            executeCalled = false;
            target.ExecuteDbCommand(DbExecuteType.NonQuery, mockCommand.Object, () =>
            {
                executeCalled = true;
                return null;
            }, null);
            Assert.IsTrue(executeCalled);
            Assert.AreEqual(2, (profiler as IProfiler).CustomTimings.Count());
            
            // test DataReaderFinished with invalid reader, it should not throw exception
            target.DataReaderFinished(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDbProfiler_ctor_InvalidProfiler()
        {
            new DbProfiler(null);
        }
    }
}
