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
using System.Linq;
using EF.Diagnostics.Profiling.Web.Extensions.Timing;
using ServiceStack.Text;

namespace EF.Diagnostics.Profiling.Web.Extensions.LogParsers
{
    public abstract class ProfilingLogParserBase : IProfilingLogParser
    {
        internal ProfilingLogParserBase() { }

        public abstract IEnumerable<IProfiler> LoadLatestProfilingSessionSummaries(uint? top = 100, uint? minDuration = 0);
        public abstract IProfiler LoadProfilingSession(Guid sessionId);

        internal SerializableProfiler ParseSessionFields(JsonObject sessionJson)
        {
            var session = new SerializableProfiler();
            session.Client = sessionJson["client"];
            session.DurationMilliseconds = ParseInt64(sessionJson["duration"]).GetValueOrDefault();
            session.MachineName = sessionJson["machine"];
            session.StartMilliseconds = ParseInt64(sessionJson["start"]).GetValueOrDefault();
            session.Sort = ParseInt64(sessionJson["sort"]).GetValueOrDefault(session.StartMilliseconds);
            session.Started = ParseDateTime(sessionJson["started"]);
            session.ExecuteType = sessionJson["executeType"];
            session.Id = ParseGuid(sessionJson["id"]);
            session.LocalAddress = sessionJson["localAddress"];
            session.Name = sessionJson["name"];
            session.Type = sessionJson["type"];
            if (sessionJson.ContainsKey("tags")) session.Tags = ParseTags(sessionJson.Child("tags"));

            return session;
        }

        internal SerializableStepTiming ParseStepFields(JsonObject stepJson)
        {
            var step = new SerializableStepTiming();
            step.DurationMilliseconds = ParseInt64(stepJson["duration"]).GetValueOrDefault();
            step.ExecuteType = stepJson["executeType"];
            step.Id = ParseGuid(stepJson["id"]);
            step.MachineName = stepJson["machine"];
            step.StartMilliseconds = ParseInt64(stepJson["start"]).GetValueOrDefault();
            step.Sort = ParseInt64(stepJson["sort"]).GetValueOrDefault(step.StartMilliseconds);
            step.Started = ParseDateTime(stepJson["started"]);
            step.Name = stepJson["name"];
            if (stepJson.ContainsKey("tags")) step.Tags = ParseTags(stepJson.Child("tags"));
            step.Type = stepJson["type"];
            step.ParentId = ParseGuid(stepJson["parentId"]);

            return step;
        }

        internal SerializableCustomTiming ParseCustomFields(JsonObject customJson)
        {
            var custom = new SerializableCustomTiming();
            custom.DurationMilliseconds = ParseInt64(customJson["duration"]).GetValueOrDefault();
            custom.ExecuteType = customJson["executeType"];
            custom.Id = ParseGuid(customJson["id"]);
            custom.MachineName = customJson["machine"];
            custom.StartMilliseconds = ParseInt64(customJson["start"]).GetValueOrDefault();
            custom.Sort = ParseInt64(customJson["sort"]).GetValueOrDefault(custom.StartMilliseconds);
            custom.Started = ParseDateTime(customJson["started"]);
            custom.Name = customJson["name"];
            if (customJson.ContainsKey("tags")) custom.Tags = ParseTags(customJson.Child("tags"));
            custom.Type = customJson["type"];
            custom.ParentId = ParseGuid(customJson["parentId"]);
            custom.InputData = customJson["inputData"];
            custom.InputSize = ParseInt(customJson["inputSize"]);
            custom.OutputSize = ParseInt(customJson["outputSize"]).GetValueOrDefault();
            custom.OutputStartMilliseconds = ParseInt64(customJson["outputStartMilliseconds"]).GetValueOrDefault();

            return custom;
        }

        internal void SortSessionTimings(SerializableProfiler session)
        {
            // ensure the first step is root
            if (session.StepTimings != null && session.StepTimings.Count > 0
                && session.StepTimings[0].Name != "root")
            {
                var temp = session.StepTimings[0];
                for (var i = 0; i < session.StepTimings.Count; ++i)
                {
                    if (session.StepTimings[i].Name == "root")
                    {
                        session.StepTimings[0] = session.StepTimings[i];
                        session.StepTimings[i] = temp;
                        break;
                    }
                }
            }

            // order step timings by sort
            if (session.StepTimings != null && session.StepTimings.Count > 0)
            {
                session.StepTimings = session.StepTimings.OrderBy(s => s.Sort).ToList();
            }

            // order custom timings by start milliseconds
            if (session.CustomTimings != null && session.CustomTimings.Count > 0)
            {
                session.CustomTimings = session.CustomTimings.OrderBy(s => s.Sort).ToList();
            }
        }

        #region Private Methods

        private HashSet<string> ParseTags(string value)
        {
            var result = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var tags = value.TrimStart('[').TrimEnd(']').Split(',');
                foreach (var tag in tags)
                {
                    result.Add(tag.Trim(" \"".ToCharArray()));
                }
            }

            return result;
        }

        private Guid ParseGuid(string value)
        {
            Guid result;
            Guid.TryParse(value, out result);
            return result;
        }

        private int? ParseInt(string value)
        {
            int result;
            if (!int.TryParse(value, out result))
            {
                return null;
            }

            return result;
        }
        private long? ParseInt64(string value)
        {
            long result;
            if (!long.TryParse(value, out result))
            {
                return null;
            }

            return result;
        }

        private DateTime ParseDateTime(string value)
        {
            DateTime result;
            DateTime.TryParse(value, out result);
            return result;
        }

        #endregion
    }
}
