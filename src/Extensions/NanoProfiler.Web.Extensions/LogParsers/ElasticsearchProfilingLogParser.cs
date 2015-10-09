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
using System.Net;
using System.Text;
using EF.Diagnostics.Profiling.Web.Extensions.Timing;
using ServiceStack.Text;

namespace EF.Diagnostics.Profiling.Web.Extensions.LogParsers
{
    public sealed class ElasticsearchProfilingLogParser : ProfilingLogParserBase
    {
        private readonly Uri _searchPath;

        public ElasticsearchProfilingLogParser(Uri searchPath)
        {
            if (searchPath == null)
            {
                throw new ArgumentNullException("searchPath");
            }

            _searchPath = searchPath;
        }

        public override IEnumerable<IProfiler> LoadLatestProfilingSessionSummaries(uint? top = 100, uint? minDuration = 0)
        {
            var sessions = new List<IProfiler>();

            var request = WebRequest.Create(_searchPath) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.KeepAlive = true;
            request.UseDefaultCredentials = true;
            request.UserAgent = "ElasticsearchProfilingLogParser";
            request.AllowAutoRedirect = false;
            var postData = CreateQueryLatestJson(top.GetValueOrDefault(100), minDuration.GetValueOrDefault(0));
            var postBytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = postBytes.Length;

            using (var postStream = request.GetRequestStream())
            {
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();
            }

            using (var response = request.GetResponse() as HttpWebResponse)
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = stream.ReadToEnd();
                    var json = JsonObject.Parse(content);

                    var hitsJson = json.Object("hits").ArrayObjects("hits");
                    if (!hitsJson.Any())
                    {
                        return null;
                    }

                    var hasSession =
                        hitsJson.Select(hit => hit.Object("_source")).Any(source => source["type"] == "session");
                    if (!hasSession)
                    {
                        return null;
                    }

                    // parse session
                    var sessionJsons =
                        hitsJson.Select(hit => hit.Object("_source")).Where(source => source["type"] == "session");
                    sessions.AddRange(sessionJsons.Select(ParseSessionFields));
                }
            }

            return sessions;
        }

        public override IProfiler LoadProfilingSession(Guid sessionId)
        {
            var request = WebRequest.Create(_searchPath) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.KeepAlive = true;
            request.UseDefaultCredentials = true;
            request.UserAgent = "ElasticsearchProfilingLogParser";
            request.AllowAutoRedirect = false;
            var postData = CreateQueryBySessionIdJson(sessionId);
            var postBytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = postBytes.Length;

            using (var postStream = request.GetRequestStream())
            {
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();
            }

            using (var response = request.GetResponse() as HttpWebResponse)
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = stream.ReadToEnd();
                    var json = JsonObject.Parse(content);

                    var hitsJson = json.Object("hits").ArrayObjects("hits");
                    if (!hitsJson.Any())
                    {
                        return null;
                    }

                    var hasSession = hitsJson.Select(hit => hit.Object("_source")).Any(source => source["type"] == "session");
                    if (!hasSession)
                    {
                        return null;
                    }

                    // parse session
                    var sessionJson = hitsJson.Select(hit => hit.Object("_source")).First(source => source["type"] == "session");
                    var session = ParseSessionFields(sessionJson);
                    session.StepTimings = new List<SerializableStepTiming>();
                    session.CustomTimings = new List<SerializableCustomTiming>();

                    // parse step timings
                    var stepJsons = hitsJson.Select(hit => hit.Object("_source")).Where(source => source["type"] == "step");
                    foreach (var stepJson in stepJsons)
                    {
                        var step = ParseStepFields(stepJson);
                        session.StepTimings.Add(step);
                    }

                    // parse custom timings
                    var customJsons = hitsJson.Select(hit => hit.Object("_source")).Where(source => source["type"] != "session" && source["type"] != "step");
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

            return null;
        }

        #region Private Methods

        private string CreateQueryBySessionIdJson(Guid sessionId)
        {
            return "{\"query\":{\"filtered\":{\"query\":{\"bool\":{\"should\":[{\"query_string\":{\"query\":\"sessionId:" + sessionId.ToString("N") + "\"}}]}}}},\"size\":1000}";
        }

        private string CreateQueryLatestJson(uint top, uint minDuration)
        {
            return "{\"query\":{\"filtered\":{\"query\":{\"bool\":{\"should\":[{\"query_string\":{\"query\":\"type:session\"}}]}},\"filter\":{\"bool\":{\"must\":[{\"fquery\":{\"query\":{\"query_string\":{\"query\":\"duration:[" + minDuration + " TO *]\"}},\"_cache\": true}}]}}}},\"size\":" + top + ", \"sort\":[{\"@timestamp\":{\"order\": \"desc\",\"ignore_unmapped\": true}}]}";
        }

        #endregion
    }
}
