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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestClass]
    public class ProfilingSessionExtensionsTest
    {
        [TestMethod]
        public void TestProfilingSessionExtensions_Step_InvalidProfilingSession()
        {
            Assert.IsNull(((ProfilingSession)null).Step("test"));
        }

        [TestMethod]
        public void TestProfilingSessionExtensions_Step_InvalidName()
        {
            var mockProfiler = new Mock<IProfiler>();
            var target = new ProfilingSession(mockProfiler.Object);
            Assert.IsNull(target.Step((string)null));
        }

        [TestMethod]
        public void TestProfilingSessionExtensions_Step()
        {
            var mockStep = new Mock<IProfilingStep>();
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Step(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).Returns(mockStep.Object);
            var target = new ProfilingSession(mockProfiler.Object);
            Assert.AreEqual(mockStep.Object, target.Step(() => "test"));
        }

        [TestMethod]
        public void TestProfilingSessionExtensions_Step_InvalidGetName()
        {
            var mockProfiler = new Mock<IProfiler>();
            var target = new ProfilingSession(mockProfiler.Object);
            Assert.IsNull(target.Step((Func<string>)null));
        }

        [TestMethod]
        public void TestProfilingSessionExtensions_Ignore_InvalidProfilingSession()
        {
            Assert.IsNull(((ProfilingSession)null).Ignore());
        }

        [TestMethod]
        public void TestProfilingSessionExtensions_Ignore()
        {
            var mockStep = new Mock<IDisposable>();
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Ignore()).Returns(mockStep.Object);
            var target = new ProfilingSession(mockProfiler.Object);
            Assert.AreEqual(mockStep.Object, target.Ignore());
        }
    }
}
