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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EF.Diagnostics.Profiling.Web.Extensions.Timing;
using ServiceStack.Text;

namespace EF.Diagnostics.Profiling.Web.Extensions.LogParsers
{
    public sealed class FileProfilingLogParser : ProfilingLogParserBase
    {
        private readonly string[] _logFileLines;

        public FileProfilingLogParser(string logFileName)
        {
            if (string.IsNullOrEmpty(logFileName))
            {
                throw new ArgumentNullException("logFileName");
            }

            if (!File.Exists(logFileName))
            {
                throw  new ArgumentException("Log file doesn't exist: " + logFileName);
            }

            using (var inStream = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(inStream))
            {
                _logFileLines = sr.ReadLines().ToArray();
            }
        }

        public override IEnumerable<IProfiler> LoadLatestProfilingSessionSummaries(uint? top = 100, uint? minDuration = 0)
        {
            var results = new List<IProfiler>();

            for (var i = _logFileLines.Length - 1; i >= 0; --i)
            {
                var sessionJson = JsonObject.Parse(_logFileLines[i]);
                if (sessionJson["type"] == "session" && long.Parse(sessionJson["duration"]) >= minDuration.GetValueOrDefault())
                {
                    var session = ParseSessionFields(sessionJson);
                    results.Add(session);

                    if (results.Count >= top.GetValueOrDefault(100))
                    {
                        break;
                    }
                }
            }

            return results;
        }

        public override IProfiler LoadProfilingSession(Guid sessionId)
        {
            var jsonArray = new JsonArrayObjects();

            // parse json array of specified session
            for (var i = _logFileLines.Length - 1; i >= 0; --i)
            {
                var json = JsonObject.Parse(_logFileLines[i]);
                if (Guid.Parse(json["sessionId"]) == sessionId)
                {
                    jsonArray.Add(json);
                }
            }

            // parse session
            var sessionJson = jsonArray.First(json => json["type"] == "session");
            var session = ParseSessionFields(sessionJson);
            session.StepTimings = new List<SerializableStepTiming>();
            session.CustomTimings = new List<SerializableCustomTiming>();

            // parse step timings
            var stepJsons = jsonArray.Where(json => json["type"] == "step");
            foreach (var stepJson in stepJsons)
            {
                var step = ParseStepFields(stepJson);
                session.StepTimings.Add(step);
            }

            // parse custom timings
            var customJsons = jsonArray.Where(json => json["type"] != "session" && json["type"] != "step");
            foreach (var customJson in customJsons)
            {
                var custom = ParseCustomFields(customJson);
                session.CustomTimings.Add(custom);
            }

            // sort session step & custom timings
            SortSessionTimings(session);

            return session;
        }
    }
}
