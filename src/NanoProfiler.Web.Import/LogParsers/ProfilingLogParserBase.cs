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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EF.Diagnostics.Profiling.Timings;
using Newtonsoft.Json.Linq;

namespace EF.Diagnostics.Profiling.Web.Import.LogParsers
{
    /// <summary>
    /// The base class of <see cref="IProfilingLogParser"/>.
    /// </summary>
    public abstract class ProfilingLogParserBase : IProfilingLogParser
    {
        /// <summary>
        /// Loads latest top profiling session summaries from log.
        /// </summary>
        /// <param name="top"></param>
        /// <param name="minDuration"></param>
        /// <returns></returns>
        public abstract IEnumerable<ITimingSession> LoadLatestSessionSummaries(uint? top = 100, uint? minDuration = 0);

        /// <summary>
        /// Loads a full profiling session from log.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public abstract ITimingSession LoadSession(Guid sessionId);

        /// <summary>
        /// Drill down profiling session from log.
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public abstract ITimingSession DrillDownSession(Guid correlationId);

        /// <summary>
        /// Drill up profiling session from log.
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        public abstract ITimingSession DrillUpSession(Guid correlationId);

        #region Protected Methods

        /// <summary>
        /// Parses session fields.
        /// </summary>
        /// <param name="sessionJson"></param>
        /// <returns></returns>
        protected ITimingSession ParseSessionFields(JObject sessionJson)
        {
            var session = new TimingSession();

            foreach (var keyValue in sessionJson)
            {
                switch (keyValue.Key)
                {
                    case "machine":
                        session.MachineName = keyValue.Value.ToObject<string>();
                        break;
                    default:
                        ParseTimingField(session, keyValue.Key, keyValue.Value.ToString());
                        break;
                }
            }

            return session;
        }

        /// <summary>
        /// Parses timing fields.
        /// </summary>
        /// <param name="timingJson"></param>
        /// <returns></returns>
        protected ITiming ParseTimingFields(JObject timingJson)
        {
            var timing = new Timing();

            foreach (var keyValue in timingJson)
            {
                ParseTimingField(timing, keyValue.Key, keyValue.Value.ToString());
            }

            return timing;
        }

        /// <summary>
        /// Sorts session timings.
        /// </summary>
        /// <param name="timingsToSort"></param>
        /// <returns></returns>
        protected IEnumerable<ITiming> SortSessionTimings(IEnumerable<ITiming> timingsToSort)
        {
            if (timingsToSort == null) return null;

            // ensure the first step is root
            var timings = timingsToSort.ToList();
            if (timings.Count > 0 && timings[0].Name != "root")
            {
                var temp = timings[0];
                for (var i = 0; i < timings.Count; ++i)
                {
                    if (timings[i].Name == "root")
                    {
                        timings[0] = timings[i];
                        timings[i] = temp;
                        break;
                    }
                }
            }

            // order timings by value of sort and then start ms
            return timings.OrderBy(s => s.Sort).ThenBy(s => s.StartMilliseconds);
        }

        /// <summary>
        /// Whether or not this data field should be ignored.
        /// </summary>
        /// <param name="timing"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool IsIgnoreDataField(ITiming timing, string key)
        {
            return key == "sessionId" || key == "machine";
        }

        #endregion

        #region Private Methods

        private void ParseTimingField(ITiming timing, string key, JToken value)
        {
            if (IsIgnoreDataField(timing, key)) return;

            switch (key)
            {
                case "type":
                    timing.Type = value.ToObject<string>();
                    break;
                case "id":
                    timing.Id = value.ToObject<Guid>();
                    break;
                case "parentId":
                    timing.ParentId = value.ToObject<Guid?>();
                    break;
                case "name":
                    timing.Name = value.ToObject<string>();
                    break;
                case "started":
                    timing.Started = value.ToObject<DateTime>();
                    break;
                case "start":
                    timing.StartMilliseconds = value.ToObject<long>();
                    break;
                case "duration":
                    timing.DurationMilliseconds = value.ToObject<long>();
                    break;
                case "tags":
                    timing.Tags = ParseTags(value.ToString());
                    break;
                case "sort":
                    timing.Sort = value.ToObject<long>();
                    break;
                default:
                    if (timing.Data == null)
                    {
                        timing.Data = new ConcurrentDictionary<string, string>();
                    }

                    timing.Data[key] = value.ToString();
                    break;
            }
        }

        private static TagCollection ParseTags(string value)
        {
            var result = new TagCollection();
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

        #endregion
    }
}
