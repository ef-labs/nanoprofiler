using System;
using System.Data;
using System.Linq;
using System.Threading;
using EF.Diagnostics.Profiling.Data;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;
using Moq;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestFixture]
    public class ProfilerTest
    {
        [Test]
        public void TestProfiler()
        {
            var resultSaved = false;
            var name = "test";
            var stepName = "step1";
            var tag = "tag1";
            var mockStorage = new Mock<IProfilingStorage>();
            var target = new Profiler(name, mockStorage.Object, new TagCollection(new[] { tag })) as IProfiler;

            Assert.AreNotEqual(default(Guid), target.Id);
            Assert.IsTrue(target.GetTimingSession().Started.AddMinutes(1) > DateTime.UtcNow);
            Thread.Sleep(100);
            Assert.IsTrue(target.GetTimingSession().DurationMilliseconds > 0);
            Assert.AreEqual(1, target.GetTimingSession().Timings.Count());
            Assert.AreEqual(name, target.GetTimingSession().Name);
            Assert.AreEqual("root", target.GetTimingSession().Timings.First().Name);
            Assert.AreEqual(tag, target.GetTimingSession().Tags.First());
            Assert.AreEqual(0, target.GetTimingSession().Timings.Count(t => t.Type != "step"));

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(cmd => cmd.CommandText).Returns("test");
            var dbTiming = new DbTiming(
                target, DbExecuteType.Reader, mockCommand.Object);
            using (target.Step(stepName, null))
            {
                target.GetTimingSession().AddTiming(dbTiming);
            }

            Assert.AreEqual(2, target.GetTimingSession().Timings.Count(t => t.Type == "step"));
            Assert.AreEqual(stepName, target.GetTimingSession().Timings.Last(t => t.Type == "step").Name);
            Assert.AreEqual(dbTiming, target.GetTimingSession().Timings.First(t => t.Type != "step"));

            using (target.Ignore()) { }

            Assert.AreEqual(2, target.GetTimingSession().Timings.Count(t => t.Type == "step"));

            using (var step = target.Step(stepName, null))
            {
                step.Discard();
            }

            Assert.AreEqual(2, target.GetTimingSession().Timings.Count(t => t.Type == "step"));

            mockStorage.Setup(storage => storage.SaveSession(target.GetTimingSession())).Callback<ITimingSession>(a =>
            {
                resultSaved = true;
            });

            target.Stop();

            Assert.IsTrue(resultSaved);
        }

        [Test]
        public void TestProfiler_ctor_InvalidName()
        {
            var mockStorage = new Mock<IProfilingStorage>();
            Assert.Throws<ArgumentNullException>(() => new Profiler(null, mockStorage.Object, null));
        }

        [Test]
        public void TestProfiler_ctor_InvalidStorage()
        {
            Assert.Throws<ArgumentNullException>(() => new Profiler("test", null, null));
        }
    }
}
