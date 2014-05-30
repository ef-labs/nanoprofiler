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
using System.Data;
using System.Linq;
using System.Threading;
using EF.Diagnostics.Profiling.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EF.Diagnostics.Profiling
{
    [TestClass]
    public class ProfilerTest
    {
        [TestMethod]
        public void TestProfiler()
        {
            var resultSaved = false;
            var name = "test";
            var stepName = "step1";
            var mockStorage = new Mock<IProfilingStorage>();
            var target = new Profiler(name, mockStorage.Object, new[] { "test" }) as IProfiler;

            Assert.AreNotEqual(default(Guid), target.Id);
            Assert.IsTrue(target.Started.AddMinutes(1) > DateTime.UtcNow);
            Thread.Sleep(100);
            Assert.IsTrue(target.DurationMilliseconds > 0);
            Assert.AreEqual(1, target.StepTimings.Count());
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual("root", target.StepTimings.First().Name);
            Assert.AreEqual("TEST", target.Tags.First());
            Assert.AreEqual(0, target.CustomTimings.Count());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(cmd => cmd.CommandText).Returns("test");
            var dbTiming = new DbTiming(
                target, DbExecuteType.Reader, mockCommand.Object);
            using (target.Step(stepName, null, null))
            {
                target.AddCustomTiming(dbTiming);
            }

            Assert.AreEqual(2, target.StepTimings.Count());
            Assert.AreEqual(stepName, target.StepTimings.Last().Name);
            Assert.AreEqual(dbTiming, target.CustomTimings.First());

            using (target.Ignore()) { }

            Assert.AreEqual(2, target.StepTimings.Count());

            using (var step = target.Step(stepName, null, null))
            {
                step.Discard();
            }

            Assert.AreEqual(2, target.StepTimings.Count());

            mockStorage.Setup(storage => storage.SaveResult(target)).Callback<IProfiler>(a =>
                {
                    resultSaved = true;
                });

            target.Stop();

            Assert.IsTrue(resultSaved);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfiler_ctor_InvalidName()
        {
            var mockStorage = new Mock<IProfilingStorage>();
            new Profiler(null, mockStorage.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfiler_ctor_InvalidStorage()
        {
            new Profiler("test", null, null);
        }
    }
}
