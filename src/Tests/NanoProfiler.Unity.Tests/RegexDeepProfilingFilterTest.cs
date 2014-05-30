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
using System.Text.RegularExpressions;
using EF.Diagnostics.Profiling.Unity;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NanoProfiler.Unity.Tests
{
    [TestClass]
    public class RegexDeepProfilingFilterTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestRegexDeepProfilingFilterWithEmptyRegexPattern()
        {
            new RegexDeepProfilingFilter(null);
        }

        [TestMethod]
        public void TestRegexDeepProfilingFilterWithEmptyType()
        {
            var target = new RegexDeepProfilingFilter(new Regex(".*")) as IDeepProfilingFilter;
            var result = target.ShouldBeProfiled(null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestRegexDeepProfilingFilter()
        {
            var testType = typeof(object);

            var target = new RegexDeepProfilingFilter(new Regex(testType.FullName)) as IDeepProfilingFilter;
            var result = target.ShouldBeProfiled(testType);
            Assert.IsTrue(result);

            target = new RegexDeepProfilingFilter(new Regex("xxx"));
            result = target.ShouldBeProfiled(testType);
            Assert.IsFalse(result);
        }
    }
}
