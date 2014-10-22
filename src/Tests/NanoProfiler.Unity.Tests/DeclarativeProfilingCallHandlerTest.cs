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

using System.Collections.Generic;

using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.Unity;

using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NanoProfiler.Unity.Tests
{
    [TestClass]
    public class DeclarativeProfilingCallHandlerTest
    {
        [TestMethod]
        public void TestDeclarativeProfilingCallHandler()
        {
            var mockProfiler = new Mock<IProfiler>();
            var mockProfilerProvider = new Mock<IProfilerProvider>();
            mockProfilerProvider.Setup(provider => provider.Start(It.IsAny<string>(), It.IsAny<IProfilingStorage>(), It.IsAny<string[]>())).Returns(mockProfiler.Object);
            ProfilingSession.ProfilerProvider = mockProfilerProvider.Object;
            ProfilingSession.Start("test");
            var stepCalled = false;
            mockProfiler.Setup(p => p.Step(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).Callback<string, IEnumerable<string>, string>((name, tags, executeType) =>
            {
                stepCalled = true;
            });

            var testObj = new TestClass();
            var method1 = typeof(TestClass).GetMethod("Method1");

            var target = new PolicyInjectionProfilingCallHandler() as ICallHandler;
            
            var mockInput1 = new Mock<IMethodInvocation>();
            mockInput1.Setup(i => i.MethodBase).Returns(method1);
            mockInput1.Setup(i => i.Target).Returns(testObj);

            target.Invoke(mockInput1.Object, () => ((input, next) => { testObj.Method1(); return null; }));
            Assert.IsTrue(stepCalled);
            Assert.IsTrue(testObj.Method1Invoked);
        }


        private class TestClass
        {
            public bool Method1Invoked;

            public void Method1()
            {
                Method1Invoked = true;
            }
        }
    }
}
