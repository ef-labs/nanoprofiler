using EF.Diagnostics.Profiling.Timings;
using EF.Diagnostics.Profiling.Web.Import;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EF.Diagnostics.Profiling.Tests
{
    [TestFixture]
    public class ImportSerializerTest
    {
        [Test]
        public void TestSerializeSessions()
        {
            ITimingSession timingSession = new TimingSession();
            var json = ImportSerializer.SerializeSessions(new[] { timingSession });

            var deserializedSession = ImportSerializer.DeserializeSessions(json).First();
        }
    }
}
