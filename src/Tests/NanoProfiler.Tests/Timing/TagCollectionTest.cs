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
using System.Linq;
using EF.Diagnostics.Profiling.Timing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EF.Diagnostics.Profiling.Tests.Timing
{
    [TestClass]
    public class TagCollectionTest
    {
        [TestMethod]
        public void TestTagCollection()
        {
            var target = new TagCollection(new[] {"t1", "", ",", "t2"});
            Assert.AreEqual("T1,T2", target.ToString());

            var deserialized = TagCollection.FromString("T1,T2");
            Assert.AreEqual(target.Count, deserialized.Count);
            Assert.AreEqual(target.First(), deserialized.First());
            Assert.AreEqual(target.Last(), deserialized.Last());

            Assert.IsNull(TagCollection.FromString(null));
        }
    }
}
