using System;
using System.Data;
using System.Linq;
using EF.Diagnostics.Profiling.Data;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;
using Moq;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests.Data
{
    [TestFixture]
    public class DbProfilerTest
    {
        [Test]
        public void TestDbProfiler_ExecuteDbCommand_InvalidExecute()
        {
            var mockStorage = new Mock<IProfilingStorage>();
            var profiler = new Profiler("test", mockStorage.Object, null);
            var mockCommand = new Mock<IDbCommand>();
            var target = new DbProfiler(profiler) as IDbProfiler;

            // execute empty execute should not throw exception
            target.ExecuteDbCommand(DbExecuteType.Reader, mockCommand.Object, null, null);
        }

        [Test]
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

        [Test]
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
            }, new TagCollection(new[] { "tag1" }));
            Assert.IsTrue(executeCalled);
            Assert.AreEqual(0, profiler.GetTimingSession().Timings.Count(t => t.Type == "db"));
            target.DataReaderFinished(new ProfiledDbDataReader(mockReader.Object, target));
            Assert.AreEqual(1, profiler.GetTimingSession().Timings.Count(t => t.Type == "db"));
            Assert.AreEqual("tag1", profiler.GetTimingSession().Timings.Last(t => t.Type == "db").Tags.First());

            // test execute nonquery
            executeCalled = false;
            target.ExecuteDbCommand(DbExecuteType.NonQuery, mockCommand.Object, () =>
            {
                executeCalled = true;
                return null;
            }, null);
            Assert.IsTrue(executeCalled);
            Assert.AreEqual(2, profiler.GetTimingSession().Timings.Count(t => t.Type == "db"));

            // test DataReaderFinished with invalid reader, it should not throw exception
            target.DataReaderFinished(null);
        }

        [Test]
        public void TestDbProfiler_ctor_InvalidProfiler()
        {
            Assert.Throws<ArgumentNullException>(() => new DbProfiler(null));
        }
    }
}
