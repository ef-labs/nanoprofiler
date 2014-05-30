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

using System.Linq;
using EF.Diagnostics.Profiling.Unity;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NanoProfiler.Unity.Tests
{
    [TestClass]
    public class DeepProfilingExtensionTest
    {
        [TestMethod]
        public void TestDeepProfilingExtensionWithEmptyFilter()
        {
            var strategies = new StagedStrategyChain<UnityBuildStage>();
            var mockContext = new Mock<ExtensionContext>();
            mockContext.Setup(c => c.Strategies).Returns(strategies);
            var target = new DeepProfilingExtension(null);
            target.InitializeExtension(mockContext.Object);
            Assert.AreEqual(0, strategies.MakeStrategyChain().Count());
        }

        [TestMethod]
        public void TestDeepProfilingExtension()
        {
            var strategies = new StagedStrategyChain<UnityBuildStage>();
            var mockContext = new Mock<ExtensionContext>();
            mockContext.Setup(c => c.Strategies).Returns(strategies);
            var mockFilter = new Mock<IDeepProfilingFilter>();

            var target = new DeepProfilingExtension(mockFilter.Object);
            target.InitializeExtension(mockContext.Object);
            Assert.AreEqual(1, strategies.MakeStrategyChain().Count());
        }
    }
}
