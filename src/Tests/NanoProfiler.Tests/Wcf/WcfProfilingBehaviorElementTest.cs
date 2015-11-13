using System.Reflection;
using EF.Diagnostics.Profiling.ServiceModel.Configuration;
using EF.Diagnostics.Profiling.ServiceModel.Description;
using NUnit.Framework;

namespace EF.Diagnostics.Profiling.Tests.Wcf
{
    [TestFixture]
    public class WcfProfilingBehaviorElementTest
    {
        [Test]
        public void TestWcfProfilingBehaviorElement()
        {
            var target = new WcfProfilingBehaviorElement();

            Assert.AreEqual(typeof(WcfProfilingBehavior), target.BehaviorType);

            var methodCreateBehavior = typeof(WcfProfilingBehaviorElement).GetMethod(
                "CreateBehavior", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = methodCreateBehavior.Invoke(target, null);
            Assert.IsTrue(result is WcfProfilingBehavior);
        }
    }
}
