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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EF.Diagnostics.Profiling.Timings;
using Newtonsoft.Json.Linq;

namespace EF.Diagnostics.Profiling.Web.Import.LogParsers
{
    /// <summary>
    /// Loads profiling sessions from log4net log files.
    /// </summary>
    public sealed class FileProfilingLogParser : ProfilingLogParserBase
    {
        private readonly string[] _logFileLines;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="FileProfilingLogParser"/>.
        /// </summary>
        /// <param name="logFileName"></param>
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
            {
                var sr = new StreamReader(inStream);
                var lines = new List<string>();
                while (!sr.EndOfStream) lines.Add(sr.ReadLine());
                _logFileLines = lines.ToArray();
            }
        }

        #endregion

        #region ProfilingLogParserBase Members

        /// <summary>
        /// Loads latest top profiling session summaries from log.
        /// </summary>
        /// <param name="top"></param>
        /// <param name="minDuration"></param>
        /// <returns></returns>
        public override IEnumerable<ITimingSession> LoadLatestSessionSummaries(uint? top = 100, uint? minDuration = 0)
        {
            var results = new List<ITimingSession>();

            for (var i = _logFileLines.Length - 1; i >= 0; --i)
            {
                var sessionJson = JObject.Parse(_logFileLines[i]);
                if (sessionJson["type"].ToObject<string>() == "session" && sessionJson["duration"].ToObject<long>() >= minDuration.GetValueOrDefault())
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

        /// <summary>
        /// Loads a full profiling session from log.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public override ITimingSession LoadSession(Guid sessionId)
        {
            var jsonArray = new List<JObject>();

            // parse json array of specified session
            for (var i = _logFileLines.Length - 1; i >= 0; --i)
            {
                var json = JObject.Parse(_logFileLines[i]);
                if (json["sessionId"].ToObject<Guid>() == sessionId)
                {
                    jsonArray.Add(json);
                }
            }

            // parse session
            var sessionJson = jsonArray.First(json => json["type"].ToObject<string>() == "session");
            var session = ParseSessionFields(sessionJson);
            var timings = new List<ITiming>();

            // parse timings
            var timingJsons = jsonArray.Where(json => json["type"].ToObject<string>() != "session");
            foreach (var timingJson in timingJsons)
            {
                var timing = ParseTimingFields(timingJson);
                timings.Add(timing);
            }

            session.Timings = SortSessionTimings(timings);

            return session;
        }

        #endregion
    }
}
