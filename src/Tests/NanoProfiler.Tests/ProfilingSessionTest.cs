/*
    The MIT License (MIT)
    Copyright © 2015 Englishtown <opensource@englishtown.com>
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
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Storages.Json;
using EF.Diagnostics.Profiling.Timings;
using EF.Diagnostics.Profiling.Web;
using Moq;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestFixture]
    public class ProfilingSessionTest
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            HttpContext.Current = null;
            ProfilingSession.CreateProfilerHandler = (name, storage, tags) => new Profiler(name, storage, tags);
            ProfilingSession.ProfilingSessionContainer = new WebProfilingSessionContainer();
            ProfilingSession.ProfilingStorage = new JsonProfilingStorage();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            HttpContext.Current = null;
            ProfilingSession.CreateProfilerHandler = (name, storage, tags) => new Profiler(name, storage, tags);
            ProfilingSession.ProfilingSessionContainer = new WebProfilingSessionContainer();
            ProfilingSession.ProfilingStorage = new JsonProfilingStorage();
        }

        [Test]
        public void TestProfilingSession_ctor_InvalidProfiler()
        {
            Assert.Throws<ArgumentNullException>(() => new ProfilingSession(null));
        }

        [Test]
        public void TestProfilingSession_getCurrent_FromHttpContext()
        {
            var mockProfiler = new Mock<IProfiler>();
            var expected = new ProfilingSession(mockProfiler.Object);
            var mockHttpContext = new HttpContextMock();
            mockHttpContext.Object.Items["nano_profiler::current_profiling_session"] = expected;
            HttpContext.Current = mockHttpContext.Object;

            Assert.AreEqual(expected, ProfilingSession.Current);
        }

        [Test]
        public void TestProfilingSession_getCurrent_FromCallContext()
        {
            lock (typeof(CallContextProfilingSessionContainer))
            {
                var mockProfiler = new Mock<IProfiler>();
                var expected = new ProfilingSession(mockProfiler.Object);
                ProfilingSession.ProfilingSessionContainer.CurrentSession = expected;

                Assert.AreEqual(expected, ProfilingSession.Current);
            }
        }

        [Test]
        public void TestProfilingSession_getProfiler()
        {
            var mockProfiler = new Mock<IProfiler>();
            var target = new ProfilingSession(mockProfiler.Object);

            Assert.AreEqual(mockProfiler.Object, target.Profiler);
        }

        [Test]
        public void TestProfilingSession_getCurrentStepId()
        {
            var expected = Guid.NewGuid();
            CallContext.LogicalSetData("nano_profiler::current_profiling_step_id", expected);

            Assert.AreEqual(expected, ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId);
        }

        [Test]
        public void TestProfilingSession_setCurrentStepId()
        {
            var expected = Guid.NewGuid();
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = expected;

            Assert.AreEqual(expected, CallContext.LogicalGetData("nano_profiler::current_profiling_step_id"));
        }

        [Test]
        public void TestProfilingSession_Start_InvalidName()
        {
            Assert.Throws<ArgumentNullException>(() => ProfilingSession.Start(null));
        }

        [Test]
        public void TestProfilingSession_Start()
        {
            // mock http context
            var mockHttpContext = new HttpContextMock();
            HttpContext.Current = mockHttpContext.Object;

            // ensure HttpContextCallContextProfilingSessionContainer is in use
            Assert.IsTrue(ProfilingSession.ProfilingSessionContainer is WebProfilingSessionContainer);

            // mock profiler and provider
            var profilerId = Guid.NewGuid();
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(p => p.Id).Returns(profilerId);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(new TimingSession(mockProfiler.Object, "test", null));
            ProfilingSession.CreateProfilerHandler = (s, storage, arg3) => mockProfiler.Object;

            // mock profiling storage
            var mockProfilingStorage = new Mock<IProfilingStorage>();
            ProfilingSession.ProfilingStorage = mockProfilingStorage.Object;
            Assert.AreEqual(mockProfilingStorage.Object, ProfilingSession.ProfilingStorage);

            // execute
            ProfilingSession.Start("test");

            Assert.AreEqual(mockProfiler.Object, ProfilingSession.Current.Profiler);
            Assert.AreEqual(mockProfiler.Object, (mockHttpContext.Object.Items["nano_profiler::current_profiling_session"] as ProfilingSession).Profiler);
            Assert.AreEqual(mockProfiler.Object.Id, CallContext.LogicalGetData("nano_profiler::current_profiling_session_id"));
        }

        [Test]
        public void TestProfilingSession_Start_HandleException()
        {
            var exceptionHandled = false;
            var expectedException = new Exception();

            ProfilingSession.CreateProfilerHandler = (s, storage, arg3) => { throw expectedException; };
            ProfilingSession.HandleExceptionHandler = (a, b) =>
                {
                    Assert.AreEqual(expectedException, a);
                    Assert.AreEqual(typeof (ProfilingSession), b);

                    exceptionHandled = true;
                };

            // mock profiling storage
            var mockProfilingStorage = new Mock<IProfilingStorage>();
            ProfilingSession.ProfilingStorage = mockProfilingStorage.Object;

            // execute
            ProfilingSession.Start("test");

            Assert.IsTrue(exceptionHandled);
        }

        [Test]
        public void TestProfilingSession_Stop()
        {
            var discardResults = true;
            var stopOfProfilerCalled = false;

            // mock profiler
            var profilerId = Guid.NewGuid();
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(p => p.Id).Returns(profilerId);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(new TimingSession(mockProfiler.Object, "test", null));
            mockProfiler.Setup(profiler => profiler.Stop(It.IsAny<bool>()))
                .Callback<bool>(a =>
                {
                    Assert.AreEqual(discardResults, a);
                    stopOfProfilerCalled = true;
                });
            var expected = new ProfilingSession(mockProfiler.Object);
            ProfilingSession.ProfilingSessionContainer.CurrentSession = expected;

            // execute
            ProfilingSession.Stop(discardResults);

            Assert.IsTrue(stopOfProfilerCalled);
        }

        [Test]
        public void TestProfilingSession_Stop_HandleException()
        {
            var discardResults = true;
            var exceptionHandled = false;
            var expectedException = new Exception();

            // mock profiler
            var profilerId = Guid.NewGuid();
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(p => p.Id).Returns(profilerId);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(new TimingSession(mockProfiler.Object, "test", null));
            mockProfiler.Setup(profiler => profiler.Stop(It.IsAny<bool>())).Throws(expectedException);
            var expected = new ProfilingSession(mockProfiler.Object);
            ProfilingSession.ProfilingSessionContainer.CurrentSession = expected;

            ProfilingSession.HandleExceptionHandler = (a, b) =>
                {
                    Assert.AreEqual(expectedException, a);
                    Assert.AreEqual(typeof(ProfilingSession), b);

                    exceptionHandled = true;
                };

            // execute
            ProfilingSession.Stop(discardResults);

            Assert.IsTrue(exceptionHandled);
        }

        [Test]
        public void TestProfilingSession_SetProfilingStorage_InvalidStorage()
        {
            Assert.Throws<ArgumentNullException>(() => ProfilingSession.ProfilingStorage = null);
        }

        [Test]
        public void TestProfilingSession_SetProfilingSessionContainer_InvalidContainer()
        {
            Assert.Throws<ArgumentNullException>(() => ProfilingSession.ProfilingSessionContainer = null);
        }

        [Test]
        public void TestProfilingSession_StepImpl()
        {
            var name = "test";

            // mock step
            var mockStep = new Mock<IProfilingStep>();

            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Step(name, null)).Returns(mockStep.Object);
            var target = new ProfilingSession(mockProfiler.Object);


            Assert.AreEqual(mockStep.Object, target.StepImpl(name, null));
        }

        [Test]
        public void TestProfilingSession_StepImpl_HandleException()
        {
            var name = "test";
            var expectedException = new Exception();
            var exceptionHandled = false;

            // mock step
            var mockStep = new Mock<IProfilingStep>();

            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Step(name, null)).Throws(expectedException);
            var target = new ProfilingSession(mockProfiler.Object);

            ProfilingSession.HandleExceptionHandler = (a, b) =>
                {
                    Assert.AreEqual(expectedException, a);
                    Assert.AreEqual(target, b);

                    exceptionHandled = true;
                };

            // execute
            Assert.IsNull(target.StepImpl(name, null));
        }
        [Test]
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

        [Test]
        public void TestProfilingSession_IgnoreImpl_HandleException()
        {
            var expectedException = new Exception();
            var exceptionHandled = false;
            
            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            mockProfiler.Setup(profiler => profiler.Ignore()).Throws(expectedException);
            var target = new ProfilingSession(mockProfiler.Object);

            ProfilingSession.HandleExceptionHandler = (a, b) =>
                {
                    Assert.AreEqual(expectedException, a);
                    Assert.AreEqual(target, b);

                    exceptionHandled = true;
                };

            // execute
            Assert.IsNull(target.IgnoreImpl());
        }

        [Test]
        public void TestProfilingSession_SetCurrentProfilingSession()
        {
            // mock profiler
            var mockProfiler = new Mock<IProfiler>();
            var timingSession = new TimingSession(mockProfiler.Object, "test", null);
            timingSession.AddTiming(new Timing(mockProfiler.Object, "step", timingSession.Id, "root", null));
            timingSession.AddTiming(new Timing(mockProfiler.Object, "step", timingSession.Timings.First().Id, "step2", null));
            mockProfiler.Setup(p => p.Id).Returns(timingSession.Id);
            mockProfiler.Setup(p => p.GetTimingSession()).Returns(timingSession);

            var session = new ProfilingSession(mockProfiler.Object);
            var success = false;

            // execute in async thread to avoid conflicts
            var thread = new Thread(() =>
                {
                    lock (typeof(ProfilingSession))
                    {
                        ProfilingSession.SetCurrentProfilingSession(session);

                        Assert.AreEqual(session, ProfilingSession.ProfilingSessionContainer.CurrentSession);
                        Assert.AreEqual(timingSession.Timings.First().Id, ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId);

                        ProfilingSession.SetCurrentProfilingSession(session, timingSession.Timings.Last().Id);
                        Assert.AreEqual(timingSession.Timings.Last().Id, ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId);
                    }
                    
                    success = true;
                });
            thread.Start();
            thread.Join();

            Assert.IsTrue(success);
        }
    }
}