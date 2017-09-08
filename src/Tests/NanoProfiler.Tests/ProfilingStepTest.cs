using System;
using System.Linq;
using System.Threading;
using EF.Diagnostics.Profiling.Timings;
using Moq;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestFixture]
    public class ProfilingStepTest
    {
        [Test]
        public void TestProfilingStep_ctor_EmptyName()
        {
            Assert.Throws<ArgumentNullException>(() => new ProfilingStep(null, "", null));
        }

        [Test]
        public void TestProfilingStep_ctor()
        {
            // mock profiler
            var profilerId = Guid.NewGuid();
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(p => p.Id).Returns(profilerId);
            var elapsed = TimeSpan.FromMilliseconds(100);
            mockProfiler.Setup(p => p.Elapsed).Returns(elapsed);

            var target = new ProfilingStep(mockProfiler.Object, "test", new TagCollection(new []{"tag1"}));
            Assert.AreEqual(elapsed.TotalMilliseconds, target.StartMilliseconds);
            Assert.AreEqual(elapsed.Ticks, target.Sort);
            Assert.AreEqual(target.Id, ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId);
        }

        [Test]
        public void TestProfilingStep_GetStepTiming()
        {
            // mock profiler
            var mockProfiler = new Mock<IProfiler>();

            var target = new ProfilingStep(mockProfiler.Object, "test", null) as IProfilingStep;
            Assert.AreEqual(target, target.GetStepTiming());
        }

        [Test]
        public void TestProfilingStep_AddTag()
        {
            // mock profiler
            var mockProfiler = new Mock<IProfiler>();

            var target = new ProfilingStep(mockProfiler.Object, "test", null) as IProfilingStep;
            target.AddTag("tag1");

            Assert.AreEqual(1, target.GetStepTiming().Tags.Count);
            Assert.AreEqual("tag1", target.GetStepTiming().Tags.First());
        }

        [Test]
        public void TestProfilingStep_AddField()
        {
            // mock profiler
            var mockProfiler = new Mock<IProfiler>();

            var target = new ProfilingStep(mockProfiler.Object, "test", null) as IProfilingStep;
            target.AddField("field1", "value1");

            Assert.AreEqual(1, target.GetStepTiming().Data.Count);
            Assert.IsTrue(target.GetStepTiming().Data.ContainsKey("field1"));
        }

        [Test]
        public void TestProfilingStep_Stop()
        {
            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            var timingSession = new TimingSession(mockProfiler.Object, "test", null);
            mockProfiler.Setup(p => p.Id).Returns(timingSession.Id);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(timingSession);
            var elapsed = TimeSpan.FromMilliseconds(100);
            mockProfiler.Setup(p => p.Elapsed).Returns(elapsed);

            var target = new ProfilingStep(mockProfiler.Object, "test", null);
            var elapsed2 = TimeSpan.FromMilliseconds(200);
            mockProfiler.Setup(p => p.Elapsed).Returns(elapsed2);

            target.Stop(true);

            Assert.AreEqual((elapsed2 - elapsed).TotalMilliseconds, target.DurationMilliseconds);
            Assert.AreEqual(1, timingSession.Timings.Count());
            Assert.AreEqual(target, timingSession.Timings.First());

            // assert no duplicated steps
            target.Stop(true);
            Assert.AreEqual(1, timingSession.Timings.Count());
            Assert.AreEqual(target, timingSession.Timings.First());
        }
    }
}
