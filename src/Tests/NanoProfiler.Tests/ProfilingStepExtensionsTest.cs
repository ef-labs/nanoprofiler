using System;
using Moq;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestFixture]
    public class ProfilingStepExtensionsTest
    {
        [Test]
        public void TestProfilingStepExtensions_Discard_EmptyStep()
        {
            ((IDisposable)null).Discard(); // no exception thrown
        }

        [Test]
        public void TestProfilingStepExtensions_Discard()
        {
            var mockStep = new Mock<IProfilingStep>();
            var discarded = false;
            mockStep.Setup(s => s.Discard()).Callback(() => discarded = true);

            ((IDisposable)mockStep.Object).Discard();

            Assert.IsTrue(discarded);
        }

        [Test]
        public void TestProfilingStepExtensions_AddTag_EmptyStep()
        {
            ((IDisposable)null).AddTag("tag1"); // no exception thrown
        }

        [Test]
        public void TestProfilingStepExtensions_AddTag()
        {
            var mockStep = new Mock<IProfilingStep>();
            var tagAdded = false;
            mockStep.Setup(s => s.AddTag(It.IsAny<string>())).Callback<string>(a =>
            {
                Assert.AreEqual("tag1", a);
                tagAdded = true;
            });

            ((IDisposable)mockStep.Object).AddTag("tag1");

            Assert.IsTrue(tagAdded);
        }

        [Test]
        public void TestProfilingStepExtensions_AddField_EmptyStep()
        {
            ((IDisposable)null).AddField("field1", "value1"); // no exception thrown
        }

        [Test]
        public void TestProfilingStepExtensions_AddField()
        {
            var mockStep = new Mock<IProfilingStep>();
            var fieldAdded = false;
            mockStep.Setup(s => s.AddField(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((a,b) =>
            {
                Assert.AreEqual("field1", a);
                Assert.AreEqual("value1", b);
                fieldAdded = true;
            });

            ((IDisposable)mockStep.Object).AddField("field1", "value1");

            Assert.IsTrue(fieldAdded);
        }
    }
}
