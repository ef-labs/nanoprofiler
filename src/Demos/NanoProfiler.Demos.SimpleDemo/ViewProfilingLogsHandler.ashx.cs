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
using System.IO;
using System.Linq;
using System.Web;
using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.Web.Import.LogParsers;

namespace NanoProfiler.Demos.SimpleDemo
{
    /// <summary>
    /// Summary description for ViewProfilingLogsHandler
    /// </summary>
    public class ViewProfilingLogsHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            context.Response.Write("<h2>Latest Profiling Results From Log Files</h2><hr />");

            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            var logFile = Directory.GetFiles(logDir, "*.log").ToList().OrderByDescending(f => f).FirstOrDefault();

            if (string.IsNullOrEmpty(logFile))
            {
                return;
            }

            var logParser = new FileProfilingLogParser(logFile);
            var sessions = logParser.LoadLatestSessionSummaries();
            foreach (var item in sessions)
            {
                var session = logParser.LoadSession(item.Id);
                if (ProfilingSession.CircularBuffer.All(s => s.Id != session.Id))
                {
                    ProfilingSession.CircularBuffer.Add(session);
                }

                context.Response.Write("<a target=\"_blank\" href=\"./nanoprofiler/view/" + session.Id + "\">" + session.Name + "</a>, " + session.DurationMilliseconds + "ms @" + session.Started.ToString("s") + "<br />");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
