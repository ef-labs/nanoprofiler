using System;
using System.Linq;
using EF.Diagnostics.Profiling.Web.Extensions.LogParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.Text;

namespace NanoProfiler.Web.Extensions.Tests.LogParsers
{
    [TestClass]
    public class ElasticsearchProfilingLogParserTest
    {
        static ElasticsearchProfilingLogParserTest()
        {
            JsConfig.ExcludeTypeInfo = true;
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;
        }

#if DEBUG
        [TestMethod]
#endif
        public void TestElasticsearchProfilingLogParser_LoadProfilingSession()
        {
            var target = new ElasticsearchProfilingLogParser(new Uri("http://10.128.34.153:9200/_search"));
            var firstSession = target.LoadLatestProfilingSessionSummaries(1).FirstOrDefault();
            if (firstSession != null)
            {
                var session = target.LoadProfilingSession(firstSession.Id);
                Console.WriteLine(JsonSerializer.SerializeToString(session));
            }
        }

#if DEBUG
        [TestMethod]
#endif
        public void TestElasticsearchProfilingLogParser_LoadLatestProfilingSessionSummaries()
        {
            var target = new ElasticsearchProfilingLogParser(new Uri("http://10.128.34.153:9200/_search"));
            var sessions = target.LoadLatestProfilingSessionSummaries(10, 20);
            Console.WriteLine(JsonSerializer.SerializeToString(sessions));
        }
    }
}
