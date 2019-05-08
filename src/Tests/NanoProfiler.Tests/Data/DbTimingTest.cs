using System;
using System.Data;
using EF.Diagnostics.Profiling.Data;
using EF.Diagnostics.Profiling.Timings;
using Moq;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests.Data
{
    [TestFixture]
    public class DbTimingTest
    {
        [Test]
        public void TestDbTiming()
        {
            var stepId = Guid.NewGuid();
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = stepId;
            var profilerDurationMilliseconds = 10;
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Elapsed).Returns(() => TimeSpan.FromMilliseconds(profilerDurationMilliseconds++));
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
            var mockSession = new Mock<ITimingSession>();
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(mockSession.Object);
            mockSession.Setup(s => s.AddTiming(It.IsAny<ITiming>()))
                .Callback<ITiming>(a =>
                {
                    Assert.AreEqual(target, a);
                    profilerAddCustomTimingCalled = true;
                });

            target.Stop();

            Assert.AreNotEqual(default(Guid), target.Id);
            Assert.AreEqual(stepId, target.ParentId);
            Assert.AreEqual(executeType.ToString().ToLowerInvariant(), target.Data["executeType"]);
            Assert.AreEqual(commandString, target.Name);
            Assert.AreEqual(10, target.StartMilliseconds);
            Assert.AreEqual("2", target.Data["readStart"]);
            Assert.AreEqual(3, target.DurationMilliseconds);

            Assert.IsTrue(profilerAddCustomTimingCalled);

            // when firstFetchDurationMilliseconds is not set and stoppped is called, 
            // the value of firstFetchDurationMilliseconds should be copied from durationmilliseconds
            string temp;
            target.Data.TryRemove("readStart", out temp);
            target.Stop();

            Assert.AreEqual(target.DurationMilliseconds.ToString(), target.Data["readStart"]);
        }

        [Test]
        public void TestDbTiming_ctor_InvalidProfiler()
        {
            var mockCommamnd = new Mock<IDbCommand>();
            Assert.Throws<ArgumentNullException>(() => new DbTiming(null, DbExecuteType.NonQuery, mockCommamnd.Object));
        }

        [Test]
        public void TestDbTiming_ctor_InvalidCommand()
        {
            var mockProfiler = new Mock<IProfiler>();
            Assert.Throws<ArgumentNullException>(() => new DbTiming(mockProfiler.Object, DbExecuteType.NonQuery, null));
        }
    }
}
