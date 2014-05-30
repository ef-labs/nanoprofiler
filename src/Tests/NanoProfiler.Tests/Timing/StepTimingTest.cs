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
using EF.Diagnostics.Profiling.Timing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EF.Diagnostics.Profiling.Tests.Timing
{
    [TestClass]
    public class ProfilingStepTimingTest
    {
        [TestMethod]
        public void TestStepTiming()
        {
            var name = "test";
            var stepId = Guid.NewGuid();
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = stepId;
            var profilerAddStepTimingCalled = false;
            var profilerDurationMilliseconds = 10;
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.DurationMilliseconds).Returns(() => profilerDurationMilliseconds++);

            var target = new StepTiming(mockProfiler.Object, name);
            mockProfiler.Setup(profiler => profiler.AddStepTiming(It.IsAny<StepTiming>()))
                .Callback<StepTiming>(a =>
                    {
                        Assert.AreEqual(target, a);
                        profilerAddStepTimingCalled = true;
                    });

            target.Dispose();

            Assert.AreNotEqual(default(Guid), target.Id);
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual(stepId, target.ParentId);
            Assert.AreEqual(10, target.StartMilliseconds);
            Assert.AreEqual(1, target.DurationMilliseconds);
            Assert.IsTrue(profilerAddStepTimingCalled);
        }

        [TestMethod]
        public void TestProfilingStepTiming_Discard()
        {
            var name = "test";
            var stepId = Guid.NewGuid();
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = stepId;
            var profilerAddStepTimingCalled = false;
            var profilerDurationMilliseconds = 10;
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.DurationMilliseconds).Returns(() => profilerDurationMilliseconds++);

            var target = new StepTiming(mockProfiler.Object, name);
            mockProfiler.Setup(profiler => profiler.AddStepTiming(It.IsAny<StepTiming>()))
                .Callback<StepTiming>(a =>
                {
                    Assert.AreEqual(target, a);
                    profilerAddStepTimingCalled = true;
                });

            (target as IProfilingStep).Discard();
            target.Dispose();

            Assert.AreNotEqual(default(Guid), target.Id);
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual(stepId, target.ParentId);
            Assert.AreEqual(10, target.StartMilliseconds);
            Assert.AreEqual(0, target.DurationMilliseconds);
            Assert.IsFalse(profilerAddStepTimingCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfilingStepTiming_ctor_InvalidProfiler()
        {
            new StepTiming(null, "test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfilingStepTiming_ctor_InvalidName()
        {
            var mockProfiler = new Mock<IProfiler>();
            new StepTiming(mockProfiler.Object, null);
        }
    }
}
