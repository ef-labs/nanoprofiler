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
using System.Runtime.Remoting.Messaging;
using System.Web;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestClass]
    public class ProfilingSessionTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            HttpContext.Current = null;
            ProfilingSession.ProfilerProvider = new ProfilerProvider();
            ProfilingSession.ProfilingSessionContainer = new WebProfilingSessionContainer();
            ProfilingSession.ProfilingStorage = new JsonProfilingStorage();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            HttpContext.Current = null;
            ProfilingSession.ProfilerProvider = new ProfilerProvider();
            ProfilingSession.ProfilingSessionContainer = new WebProfilingSessionContainer();
            ProfilingSession.ProfilingStorage = new JsonProfilingStorage();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfilingSession_ctor_InvalidProfiler()
        {
            new ProfilingSession(null);
        }

        [TestMethod]
        public void TestProfilingSession_getCurrent_FromHttpContext()
        {
            var mockProfiler = new Mock<IProfiler>();
            var expected = new ProfilingSession(mockProfiler.Object);
            var mockHttpContext = new HttpContextMock();
            mockHttpContext.Object.Items["nano_profiler::current_profiling_session"] = expected;
            HttpContext.Current = mockHttpContext.Object;

            Assert.AreEqual(expected, ProfilingSession.Current);
        }

        [TestMethod]
        public void TestProfilingSession_getCurrent_FromCallContext()
        {
            var mockProfiler = new Mock<IProfiler>();
            var expected = new ProfilingSession(mockProfiler.Object);
            CallContext.LogicalSetData("nano_profiler::current_profiling_session", expected);

            Assert.AreEqual(expected, ProfilingSession.Current);
        }

        [TestMethod]
        public void TestProfilingSession_getProfiler()
        {
            var mockProfiler = new Mock<IProfiler>();
            var target = new ProfilingSession(mockProfiler.Object);

            Assert.AreEqual(mockProfiler.Object, target.Profiler);
        }

        [TestMethod]
        public void TestProfilingSession_getCurrentStepId()
        {
            var expected = Guid.NewGuid();
            CallContext.LogicalSetData("nano_profiler::current_profiling_step_id", expected);

            Assert.AreEqual(expected, ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId);
        }

        [TestMethod]
        public void TestProfilingSession_setCurrentStepId()
        {
            var expected = Guid.NewGuid();
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = expected;

            Assert.AreEqual(expected, CallContext.LogicalGetData("nano_profiler::current_profiling_step_id"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfilingSession_Start_InvalidName()
        {
            ProfilingSession.Start(null);
        }

        [TestMethod]
        public void TestProfilingSession_Start()
        {
            // mock http context
            var mockHttpContext = new HttpContextMock();
            HttpContext.Current = mockHttpContext.Object;

            // ensure HttpContextCallContextProfilingSessionContainer is in use
            Assert.IsTrue(ProfilingSession.ProfilingSessionContainer is WebProfilingSessionContainer);

            // mock profiler and provider
            var mockProfiler = new Mock<IProfiler>();
            var mockProfilerProvider = new Mock<IProfilerProvider>();
            mockProfilerProvider.Setup(provider => provider.Start(It.IsAny<string>(), It.IsAny<IProfilingStorage>(), It.IsAny<string[]>())).Returns(mockProfiler.Object);
            ProfilingSession.ProfilerProvider = mockProfilerProvider.Object;
            Assert.AreEqual(mockProfilerProvider.Object, ProfilingSession.ProfilerProvider);

            // mock profiling storage
            var mockProfilingStorage = new Mock<IProfilingStorage>();
            ProfilingSession.ProfilingStorage = mockProfilingStorage.Object;
            Assert.AreEqual(mockProfilingStorage.Object, ProfilingSession.ProfilingStorage);
            
            // execute
            ProfilingSession.Start("test");

            Assert.AreEqual(mockProfiler.Object, ProfilingSession.Current.Profiler);
            Assert.AreEqual(mockProfiler.Object, (mockHttpContext.Object.Items["nano_profiler::current_profiling_session"] as ProfilingSession).Profiler);
            Assert.AreEqual(mockProfiler.Object, ((CallContext.LogicalGetData("nano_profiler::current_profiling_session") as WeakReference).Target as ProfilingSession).Profiler);
        }

        [TestMethod]
        public void TestProfilingSession_Start_HandleException()
        {
            var exceptionHandled = false;
            var expectedException = new Exception();

            // mock provider
            var mockProfilerProvider = new Mock<IProfilerProvider>();
            mockProfilerProvider.Setup(provider => provider.Start(It.IsAny<string>(), It.IsAny<IProfilingStorage>(), It.IsAny<string[]>())).Throws(expectedException);
            mockProfilerProvider.Setup(provider => provider.HandleException(It.IsAny<Exception>(), It.IsAny<object>()))
                .Callback<Exception, object>((a, b) =>
                    {
                        Assert.AreEqual(expectedException, a);
                        Assert.AreEqual(typeof(ProfilingSession), b);

                        exceptionHandled = true;
                    });
            ProfilingSession.ProfilerProvider = mockProfilerProvider.Object;

            // mock profiling storage
            var mockProfilingStorage = new Mock<IProfilingStorage>();
            ProfilingSession.ProfilingStorage = mockProfilingStorage.Object;

            // execute
            ProfilingSession.Start("test");

            Assert.IsTrue(exceptionHandled);
        }

        [TestMethod]
        public void TestProfilingSession_Stop()
        {
            var discardResults = true;
            var stopOfProfilerCalled = false;

            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Stop(It.IsAny<bool>()))
                .Callback<bool>(a =>
                    {
                        Assert.AreEqual(discardResults, a);
                        stopOfProfilerCalled = true;
                    });
            var expected = new ProfilingSession(mockProfiler.Object);
            CallContext.LogicalSetData("nano_profiler::current_profiling_session", expected);

            // execute
            ProfilingSession.Stop(discardResults);

            Assert.IsTrue(stopOfProfilerCalled);
        }

        [TestMethod]
        public void TestProfilingSession_Stop_HandleException()
        {
            var discardResults = true;
            var exceptionHandled = false;
            var expectedException = new Exception();

            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Stop(It.IsAny<bool>())).Throws(expectedException);
            var expected = new ProfilingSession(mockProfiler.Object);
            CallContext.LogicalSetData("nano_profiler::current_profiling_session", expected);

            // mock provider
            var mockProflingProvider = new Mock<IProfilerProvider>();
            mockProflingProvider.Setup(provider => provider.HandleException(It.IsAny<Exception>(), It.IsAny<object>()))
                .Callback<Exception, object>((a, b) =>
                    {
                        Assert.AreEqual(expectedException, a);
                        Assert.AreEqual(typeof(ProfilingSession), b);

                        exceptionHandled = true;
                    });
            ProfilingSession.ProfilerProvider = mockProflingProvider.Object;

            // execute
            ProfilingSession.Stop(discardResults);

            Assert.IsTrue(exceptionHandled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfilingSession_SetProfilerProvider_InvalidProvider()
        {
            ProfilingSession.ProfilerProvider = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfilingSession_SetProfilingStorage_InvalidStorage()
        {
            ProfilingSession.ProfilingStorage = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfilingSession_SetProfilingSessionContainer_InvalidContainer()
        {
            ProfilingSession.ProfilingSessionContainer = null;
        }

        [TestMethod]
        public void TestProfilingSession_StepImpl()
        {
            var name = "test";

            // mock step
            var mockStep = new Mock<IProfilingStep>();

            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Step(name, null, It.IsAny<string>())).Returns(mockStep.Object);
            var target = new ProfilingSession(mockProfiler.Object);


            Assert.AreEqual(mockStep.Object, target.StepImpl(name, null));
        }

        [TestMethod]
        public void TestProfilingSession_StepImpl_HandleException()
        {
            var name = "test";
            var expectedException = new Exception();
            var exceptionHandled = false;

            // mock step
            var mockStep = new Mock<IProfilingStep>();

            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Step(name, null, null)).Throws(expectedException);
            var target = new ProfilingSession(mockProfiler.Object);

            // mock provider
            var mockProflingProvider = new Mock<IProfilerProvider>();
            mockProflingProvider.Setup(provider => provider.HandleException(It.IsAny<Exception>(), It.IsAny<object>()))
                .Callback<Exception, object>((a, b) =>
                {
                    Assert.AreEqual(expectedException, a);
                    Assert.AreEqual(target, b);

                    exceptionHandled = true;
                });
            ProfilingSession.ProfilerProvider = mockProflingProvider.Object;

            // execute
            Assert.IsNull(target.StepImpl(name, null));
        }
        [TestMethod]
        public void TestProfilingSession_IgnoreImpl()
        {
            // mock step
            var mockStep = new Mock<IDisposable>();

            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Ignore()).Returns(mockStep.Object);
            var target = new ProfilingSession(mockProfiler.Object);


            Assert.AreEqual(mockStep.Object, target.IgnoreImpl());
        }

        [TestMethod]
        public void TestProfilingSession_IgnoreImpl_HandleException()
        {
            var expectedException = new Exception();
            var exceptionHandled = false;

            // mock step
            var mockStep = new Mock<IDisposable>();

            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Ignore()).Throws(expectedException);
            var target = new ProfilingSession(mockProfiler.Object);

            // mock provider
            var mockProflingProvider = new Mock<IProfilerProvider>();
            mockProflingProvider.Setup(provider => provider.HandleException(It.IsAny<Exception>(), It.IsAny<object>()))
                .Callback<Exception, object>((a, b) =>
                {
                    Assert.AreEqual(expectedException, a);
                    Assert.AreEqual(target, b);

                    exceptionHandled = true;
                });
            ProfilingSession.ProfilerProvider = mockProflingProvider.Object;

            // execute
            Assert.IsNull(target.IgnoreImpl());
        }
    }
}
