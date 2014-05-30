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
using EF.Diagnostics.Profiling.Data;
using EF.Diagnostics.Profiling.Timing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EF.Diagnostics.Profiling.Tests.Data
{
    [TestClass]
    public class DbTimingTest
    {
        [TestMethod]
        public void TestDbTiming()
        {
            var stepId = Guid.NewGuid();
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = stepId;
            var profilerDurationMilliseconds = 10;
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.DurationMilliseconds).Returns(() => profilerDurationMilliseconds++);
            var executeType = DbExecuteType.Reader;
            var commandString = "test sql";
            var mockParameters = new Mock<IDataParameterCollection>();
            mockParameters.Setup(p => p.GetEnumerator()).Returns(new IDataParameter[0].GetEnumerator());
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(cmd => cmd.CommandText).Returns(commandString);
            mockCommand.Setup(cmd => cmd.Parameters).Returns(mockParameters.Object);

            var target = new DbTiming(mockProfiler.Object, executeType, mockCommand.Object);
            target.FirstFetch();

            var profilerAddCustomTimingCalled = false;
            mockProfiler.Setup(profiler => profiler.AddCustomTiming(It.IsAny<DbTiming>()))
                .Callback<CustomTiming>(a =>
                    {
                        Assert.AreEqual(target, a);
                        profilerAddCustomTimingCalled = true;
                    });

            target.Stop();

            Assert.AreNotEqual(default(Guid), target.Id);
            Assert.AreEqual(stepId, target.ParentId);
            Assert.AreEqual(executeType, target.DbExecuteType);
            Assert.AreEqual(commandString, target.Name);
            Assert.IsTrue(target.Parameters != null);
            Assert.AreEqual(10, target.StartMilliseconds);
            Assert.AreEqual(1, target.OutputStartMilliseconds);
            Assert.AreEqual(2, target.DurationMilliseconds);

            Assert.IsTrue(profilerAddCustomTimingCalled);

            // when firstFetchDurationMilliseconds is not set and stoppped is called, 
            // the value of firstFetchDurationMilliseconds should be copied from durationmilliseconds
            target.OutputStartMilliseconds = null;
            target.Stop();

            Assert.AreEqual(target.DurationMilliseconds, target.OutputStartMilliseconds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDbTiming_ctor_InvalidProfiler()
        {
            var mockCommamnd = new Mock<IDbCommand>();
            new DbTiming(null, DbExecuteType.NonQuery, mockCommamnd.Object);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDbTiming_ctor_InvalidCommand()
        {
            var mockProfiler = new Mock<IProfiler>();
            new DbTiming(mockProfiler.Object, DbExecuteType.NonQuery, null);
        }
    }
}
