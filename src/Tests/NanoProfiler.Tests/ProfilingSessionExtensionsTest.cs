using System;
using System.Linq;
using EF.Diagnostics.Profiling.Timings;
using Moq;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestFixture]
    public class ProfilingSessionExtensionsTest
    {
        [Test]
        public void TestProfilingSessionExtensions_Step_InvalidProfilingSession()
        {
            Assert.IsNull(((ProfilingSession)null).Step("test"));
        }

        [Test]
        public void TestProfilingSessionExtensions_Step_InvalidName()
        {
            var mockProfiler = new Mock<IProfiler>();
            var target = new ProfilingSession(mockProfiler.Object);
            Assert.IsNull(target.Step((string)null));
        }

        [Test]
        public void TestProfilingSessionExtensions_Step()
        {
            var mockStep = new Mock<IProfilingStep>();
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Step(It.IsAny<string>(), It.IsAny<TagCollection>())).Returns(mockStep.Object);
            var target = new ProfilingSession(mockProfiler.Object);
            Assert.AreEqual(mockStep.Object, target.Step(() => "test"));
        }

        [Test]
        public void TestProfilingSessionExtensions_Step_InvalidGetName()
        {
            var mockProfiler = new Mock<IProfiler>();
            var target = new ProfilingSession(mockProfiler.Object);
            Assert.IsNull(target.Step((Func<string>)null));
        }

        [Test]
        public void TestProfilingSessionExtensions_Ignore_InvalidProfilingSession()
        {
            Assert.IsNull(((ProfilingSession)null).Ignore());
        }

        [Test]
        public void TestProfilingSessionExtensions_Ignore()
        {
            var mockStep = new Mock<IDisposable>();
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Ignore()).Returns(mockStep.Object);
            var target = new ProfilingSession(mockProfiler.Object);
            Assert.AreEqual(mockStep.Object, target.Ignore());
        }

        [Test]
        public void TestProfilingSessionExtensions_AddTag_EmptySession()
        {
            ((ProfilingSession)null).AddTag(""); // no exception thrown
        }

        [Test]
        public void TestProfilingSessionExtensions_AddTag_InvalidTag()
        {
            // mock profiler
            var profilerId = Guid.NewGuid();
            var mockProfiler = new Mock<IProfiler>();
            var timingSession = new TimingSession(mockProfiler.Object, "test", null);
            mockProfiler.Setup(p => p.Id).Returns(profilerId);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(timingSession);

            var target = new ProfilingSession(mockProfiler.Object);
            target.AddTag("");

            Assert.IsNull(timingSession.Tags);
        }

        [Test]
        public void TestProfilingSessionExtensions_AddTag()
        {
            // mock profiler
            var profilerId = Guid.NewGuid();
            var mockProfiler = new Mock<IProfiler>();
            var timingSession = new TimingSession(mockProfiler.Object, "test", null);
            mockProfiler.Setup(p => p.Id).Returns(profilerId);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(timingSession);

            var target = new ProfilingSession(mockProfiler.Object);
            target.AddTagImpl("tag1");

            Assert.AreEqual(1, timingSession.Tags.Count);
            Assert.AreEqual("tag1", timingSession.Tags.First());
        }

        [Test]
        public void TestProfilingSessionExtensions_AddField_EmptySession()
        {
            ((ProfilingSession)null).AddField("field1", "value1"); // no exception thrown
        }

        [Test]
        public void TestProfilingSessionExtensions_AddField_InvalidFieldKey()
        {
            // mock profiler
            var profilerId = Guid.NewGuid();
            var mockProfiler = new Mock<IProfiler>();
            var timingSession = new TimingSession(mockProfiler.Object, "test", null);
            mockProfiler.Setup(p => p.Id).Returns(profilerId);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(timingSession);

            var target = new ProfilingSession(mockProfiler.Object);
            target.AddField(null, "value1");

            Assert.AreEqual(0, timingSession.Data.Count);
        }

        [Test]
        public void TestProfilingSessionExtensions_AddField()
        {
            // mock profiler
            var profilerId = Guid.NewGuid();
            var mockProfiler = new Mock<IProfiler>();
            var timingSession = new TimingSession(mockProfiler.Object, "test", null);
            mockProfiler.Setup(p => p.Id).Returns(profilerId);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(timingSession);

            var target = new ProfilingSession(mockProfiler.Object);
            target.AddField("field1", "value1");

            Assert.AreEqual("value1", timingSession.Data["field1"]);
        }
    }
}
