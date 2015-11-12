using System.Linq;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestFixture]
    public class CircularBufferTest
    {
        [Test]
        public void TestCircularBuffer()
        {
            var target = new CircularBuffer<string>(2);

            Assert.AreEqual(0, target.Count());

            target.Add("item1");
            
            Assert.AreEqual(1, target.Count());
            Assert.AreEqual("item1", target.First());

            target.Add("item2");

            Assert.AreEqual(2, target.Count());
            Assert.AreEqual("item2", target.Last());

            target.Add("item3");
            Assert.AreEqual(2, target.Count());
            Assert.AreEqual("item2", target.First());
            Assert.AreEqual("item3", target.Last());
        }
    }
}
