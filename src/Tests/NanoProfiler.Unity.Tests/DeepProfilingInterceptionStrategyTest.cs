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

using EF.Diagnostics.Profiling.Unity;

using Microsoft.Practices.ObjectBuilder2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NanoProfiler.Unity.Tests
{
    [TestClass]
    public class DeepProfilingInterceptionStrategyTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDeepProfilingInterceptionStrategyWithEmptyContext()
        {
            var mockFilter = new Mock<IDeepProfilingFilter>();
            var target = new DeepProfilingInterceptionStrategy(mockFilter.Object);
            target.PostBuildUp(null);
        }

        [TestMethod]
        public void TestDeepProfilingInterceptionStrategy()
        {
            var mockFilter = new Mock<IDeepProfilingFilter>();
            mockFilter.Setup(f => f.ShouldBeProfiled(typeof(ITestClass))).Returns(true);
            var target = new DeepProfilingInterceptionStrategy(mockFilter.Object);
            var mockContext = new Mock<IBuilderContext>();
            var testObj = new TestClass();
            mockContext.Setup(c => c.Existing).Returns(testObj);
            mockContext.Setup(c => c.OriginalBuildKey).Returns(new NamedTypeBuildKey(typeof(ITestClass)));
            var proxyCreated = false;
            mockContext.SetupSet(c => c.Existing = It.IsAny<object>()).Callback<object>(value =>
            {
                proxyCreated = true;
            });
            target.PostBuildUp(mockContext.Object);
            Assert.IsTrue(proxyCreated);
        }

        [TestMethod]
        public void TestDeepProfilingInterceptionStrategyWithNonInterfaceType()
        {
            var mockFilter = new Mock<IDeepProfilingFilter>();
            var target = new DeepProfilingInterceptionStrategy(mockFilter.Object);
            var mockContext = new Mock<IBuilderContext>();
            var testObj = new TestClass();
            mockContext.Setup(c => c.Existing).Returns(testObj);
            mockContext.Setup(c => c.OriginalBuildKey).Returns(new NamedTypeBuildKey(typeof(TestClass)));
            var proxyCreated = false;
            mockContext.SetupSet(c => c.Existing = It.IsAny<object>()).Callback<object>(value =>
            {
                proxyCreated = true;
            });
            target.PostBuildUp(mockContext.Object);
            Assert.IsFalse(proxyCreated);
        }

        public interface ITestClass
        {
        }

        private class TestClass : ITestClass
        {
        }
    }
}
