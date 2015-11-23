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
using System.Text;
using System.Web;
using System.Xml;
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling.Web.Handlers
{
    /// <summary>
    /// The HttpModule of NanoProfiler supports view-latest-profiling-results
    /// via ~/nanoprofiler/view and ~/nanoprofiler/view/{uuid}
    /// </summary>
    public class NanoProfilerModule : IHttpModule
    {
        private const string ViewUrl = "/nanoprofiler/view";
        private const string ETCorrelationId = "X-ET-Correlation-Id";

        /// <summary>
        /// The default Html of the view-result index page: ~/nanoprofiler/view
        /// </summary>
        public static string ViewResultIndexHeaderHtml = "<h1>NanoProfiler Latest Profiling Results</h1>";

        /// <summary>
        /// The default Html of the view-result page: ~/nanoprofiler/view/{uuid}
        /// </summary>
        public static string ViewResultHeaderHtml = "<h1>NanoProfiler Profiling Result</h1>";

        /// <summary>
        /// The handler to search for child profiling session by correlationId.
        /// </summary>
        public static Func<string, Guid?> DrillDownHandler { get; set; }

        /// <summary>
        /// The handler to search for parent profiling session by correlationId.
        /// </summary>
        public static Func<string, Guid?> DrillUpHandler { get; set; }

        #region IHttpModule Members

        /// <summary>
        /// Disposes the current <see cref="NanoProfilerModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes the current <see cref="NanoProfilerModule"/>.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Init(HttpApplication application)
        {
            application.BeginRequest += ApplicationOnBeginRequest;
            application.EndRequest += ApplicationOnEndRequest;
            application.Error += ApplicationOnError;
        }

        #endregion

        #region Private Methods

        private static void ClearIfCurrentProfilingSessionStopped()
        {
            var profilingSession = ProfilingSession.Current;
            if (profilingSession == null)
            {
                return;
            }

            if (profilingSession.Profiler.IsStopped)
            {
                ProfilingSession.ProfilingSessionContainer.Clear();
            }
        }

        private string GetCorrelationIdFromHeaders(HttpContext context)
        {
            if (context.Request.Headers.AllKeys.Contains(ETCorrelationId))
            {
                var correlationIds = context.Request.Headers.GetValues(ETCorrelationId);
                if (correlationIds != null)
                {
                    return correlationIds.FirstOrDefault();
                }
            }

            return null;
        }

        private void ApplicationOnBeginRequest(object sender, EventArgs eventArgs)
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return;
            }

            #region Start web profiling

            var url = context.Request.Url.ToString();

            // WCF profiling will be started in wcf profiling behavior
            // so it is not necessary to start profiling here
            if (url.Contains(".svc")) return;

            ProfilingSession.Start(url);

            var correlationId = GetCorrelationIdFromHeaders(context);
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                ProfilingSession.Current.AddField("correlationId", correlationId);
            }

            #endregion

            if (ProfilingSession.CircularBuffer == null)
            {
                return;
            }

            ClearIfCurrentProfilingSessionStopped();

            // only supports GET method for view results
            if (context.Request.HttpMethod != "GET")
            {
                return;
            }

            var path = context.Request.Path.TrimEnd('/');

            if (path.EndsWith("/nanoprofiler-resources/icons"))
            {
                context.Response.ContentType = "image/png";
                var iconsStream = GetType().Assembly.GetManifestResourceStream("EF.Diagnostics.Profiling.Web.Handlers.icons.png");
                using (var br = new BinaryReader(iconsStream))
                {
                    context.Response.BinaryWrite(br.ReadBytes((int)iconsStream.Length));
                    context.Response.End();
                }
                return;
            }

            if (path.EndsWith("/nanoprofiler-resources/css"))
            {
                context.Response.ContentType = "text/css";
                var cssStream = GetType().Assembly.GetManifestResourceStream("EF.Diagnostics.Profiling.Web.Handlers.treeview_timeline.css");
                using (var sr = new StreamReader(cssStream))
                {
                    context.Response.Write(sr.ReadToEnd());
                    context.Response.End();
                }
                return;
            }

            // view index of all latest results: ~/nanoprofiler/view
            if (path.EndsWith(ViewUrl, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.ContentType = "text/html";

                var sb = new StringBuilder();
                sb.Append("<head>");
                sb.Append("<title>NanoProfiler Latest Profiling Results</title>");
                sb.Append("<style>th { width: 200px; text-align: left; } .gray { background-color: #eee; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");
                sb.Append("</head");
                sb.Append("<body>");
                sb.Append(ViewResultIndexHeaderHtml);
                
                sb.Append("<table>");
                sb.Append("<tr><th class=\"nowrap\">Time (UTC)</th><th class=\"nowrap\">Duration (ms)</th><th>Url</th></tr>");
                var latestResults = ProfilingSession.CircularBuffer.OrderByDescending(r => r.Started);
                var i = 0;
                foreach (var result in latestResults)
                {
                    sb.Append("<tr");
                    if ((i++) % 2 == 1)
                    {
                        sb.Append(" class=\"gray\"");
                    }
                    sb.Append("><td class=\"nowrap\">");
                    sb.Append(result.Started.ToString("yyyy-MM-ddTHH:mm:ss.FFF"));
                    sb.Append("</td><td class=\"nowrap\">");
                    sb.Append(result.DurationMilliseconds);
                    sb.Append("</td><td><a href=\"view/");
                    sb.Append(result.Id.ToString());
                    sb.Append("\" target=\"_blank\">");
                    sb.Append(result.Name.Replace("\r\n", " "));
                    sb.Append("</a></td></tr>");
                }
                sb.Append("</table>");

                sb.Append("</body>");

                context.Response.Write(sb.ToString());
                context.Response.End();
                return;
            }

            // view specific result by uuid: ~/nanoprofiler/view/{uuid}
            if (path.IndexOf(ViewUrl, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var sb = new StringBuilder();
                sb.Append("<head>");
                sb.Append("<meta charset=\"utf-8\" />");
                sb.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
                sb.Append("<title>NanoProfiler Profiling Result</title>");
                sb.Append("<link rel=\"stylesheet\" href=\"./nanoprofiler-resources/css\" />");
                sb.Append("</head");
                sb.Append("<body>");
                sb.Append("<h1>NanoProfiler Profiling Result</h1>");

                var uuid = path.Split('/').Last();
                var result = ProfilingSession.CircularBuffer.FirstOrDefault(
                        r => r.Id.ToString().ToLowerInvariant() == uuid.ToLowerInvariant());
                if (result != null)
                {
                    sb.Append("<div class=\"css-treeview\">");

                    // print summary
                    sb.Append("<ul>");
                    sb.Append("<li class=\"summary\">");
                    PrintDrillUpLink(sb, result);
                    sb.Append(result.Name.Replace("\r\n", " "));
                    sb.Append("</li>");
                    sb.Append("<li class=\"summary\">");
                    if (result.Data != null)
                    {
                        foreach (var keyValue in result.Data)
                        {
                            if (string.IsNullOrWhiteSpace(keyValue.Value)) continue;

                            sb.Append("<b>");
                            sb.Append(keyValue.Key);
                            sb.Append(": </b>");
                            var encodedValue = HttpUtility.HtmlEncode(keyValue.Value);
                            if (keyValue.Key.EndsWith("Count") || keyValue.Key.EndsWith("Duration"))
                            {
                                sb.Append("<span class=\"");
                                sb.Append(keyValue.Key);
                                sb.Append("\">");
                                sb.Append(encodedValue);
                                sb.Append("</span>");
                            }
                            else
                            {
                                sb.Append(encodedValue);
                            }
                            sb.Append(" &nbsp; ");
                        }
                    }
                    sb.Append("<b>machine: </b>");
                    sb.Append(result.MachineName);
                    sb.Append(" &nbsp; ");
                    if (result.Tags != null && result.Tags.Any())
                    {
                        sb.Append("<b>tags: </b>");
                        sb.Append(string.Join(", ", result.Tags));
                        sb.Append(" &nbsp; ");
                    }
                    sb.Append("</li>");
                    sb.Append("</ul>");

                    var totalLength = result.DurationMilliseconds;
                    if (totalLength == 0)
                    {
                        totalLength = 1;
                    }
                    var factor = 300.0/totalLength;

                    // print ruler
                    sb.Append("<ul>");
                    sb.Append("<li class=\"ruler\"><span style=\"width:300px\">0</span><span style=\"width:80px\">");
                    sb.Append(totalLength);
                    sb.Append(
                        " (ms)</span><span style=\"width:20px\">&nbsp;</span><span style=\"width:60px\">Start</span><span style=\"width:60px\">Duration</span><span style=\"width:20px\">&nbsp;</span><span>Timing Hierarchy</span></li>");
                    sb.Append("</ul>");

                    // print timings
                    sb.Append("<ul class=\"timing\">");
                    PrintTimings(result, result.Id, sb, factor);
                    sb.Append("");
                    sb.Append("</ul>");
                    sb.Append("</div>");

                    // print timing data popups
                    foreach (var timing in result.Timings)
                    {
                        if (timing.Data == null || !timing.Data.Any()) continue;

                        sb.Append("<aside id=\"data_");
                        sb.Append(timing.Id.ToString());
                        sb.Append("\" style=\"display:none\" class=\"modal\">");
                        sb.Append("<div>");
                        sb.Append("<h4><code>");
                        sb.Append(timing.Name.Replace("\r\n", " "));
                        sb.Append("</code></h4>");
                        sb.Append("<textarea>");
                        foreach (var keyValue in timing.Data)
                        {
                            if (string.IsNullOrWhiteSpace(keyValue.Value)) continue;

                            sb.Append(keyValue.Key);
                            sb.Append(":\r\n");
                            var value = keyValue.Value.Trim();
                            if (value.StartsWith("<"))
                            {
                                // asuume it is XML
                                // try to format XML with indent
                                var doc = new XmlDocument();
                                try
                                {
                                    doc.LoadXml(value);
                                    var ms = new MemoryStream();
                                    var writer = new XmlTextWriter(ms, null) {Formatting = Formatting.Indented};
                                    doc.Save(writer);
                                    ms.Seek(0, SeekOrigin.Begin);
                                    using (var sr = new StreamReader(ms))
                                    {
                                        value = sr.ReadToEnd();
                                    }
                                }
                                catch
                                {
                                    //squash exception
                                }
                            }
                            sb.Append(value);
                            sb.Append("\r\n\r\n");
                        }
                        if (timing.Tags != null && timing.Tags.Any())
                        {
                            sb.Append("tags:\r\n");
                            sb.Append(timing.Tags);
                            sb.Append("\r\n");
                        }
                        sb.Append("</textarea>");
                        sb.Append(
                            "<a href=\"#close\" title=\"Close\" onclick=\"this.parentNode.parentNode.style.display='none'\">Close</a>");
                        sb.Append("</div>");
                        sb.Append("</aside>");
                    }
                }
                else
                {
                    sb.Append("Specified result does not exist!");
                }
                sb.Append("</body>");

                context.Response.Write(sb.ToString());
                context.Response.End();
                return;
            }

        #endregion
        }
        
        private void ApplicationOnEndRequest(object sender, EventArgs eventArgs)
        {
            ProfilingSession.Stop();
        }

        private void PrintTimings(ITimingSession session, Guid parentId, StringBuilder sb, double factor)
        {
            var timings = session.Timings.Where(s => s.ParentId == parentId);
            foreach (var timing in timings)
            {
                PrintTiming(session, timing, sb, factor);
            }
        }

        private void PrintTiming(ITimingSession session, ITiming timing, StringBuilder sb, double factor)
        {
            sb.Append("<li><span class=\"timing\" style=\"padding-left: ");
            var start = Math.Floor(timing.StartMilliseconds*factor);
            if (start > 300)
            {
                start = 300;
            }
            sb.Append(start);
            sb.Append("px\"><span class=\"bar ");
            sb.Append(timing.Type);
            sb.Append("\" title=\"");
            sb.Append(HttpUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
            sb.Append("\" style=\"width: ");
            var width = (int)Math.Round(timing.DurationMilliseconds*factor);
            if (width > 300)
            {
                width = 300;
            }
            else if (width == 0)
            {
                width = 1;
            }
            sb.Append(width);
            sb.Append("px\"></span><span class=\"start\">+");
            sb.Append(timing.StartMilliseconds);
            sb.Append("</span><span class=\"duration\">");
            sb.Append(timing.DurationMilliseconds);
            sb.Append("</span></span>");
            var hasChildTimings = session.Timings.Any(s => s.ParentId == timing.Id);
            if (hasChildTimings)
            {
                sb.Append("<input type=\"checkbox\" id=\"t_");
                sb.Append(timing.Id.ToString());
                sb.Append("\" checked=\"checked\" /><label for=\"t_");
                sb.Append(timing.Id.ToString());
                sb.Append("\">");
                PrintDataLink(sb, timing);
                PrintDrillDownLink(sb, timing);
                sb.Append(HttpUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
                sb.Append("</label>");
                sb.Append("<ul>");
                PrintTimings(session, timing.Id, sb, factor);
                sb.Append("</ul>");
            }
            else
            {
                sb.Append("<span class=\"leaf\">");
                PrintDataLink(sb, timing);
                PrintDrillDownLink(sb, timing);
                sb.Append(HttpUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
                sb.Append("</span>");
            }
            sb.Append("</li>");
        }

        private void PrintDataLink(StringBuilder sb, ITiming timing)
        {
            if (timing.Data == null || !timing.Data.Any()) return;

            sb.Append("[<a href=\"#data_");
            sb.Append(timing.Id.ToString());
            sb.Append("\" onclick=\"document.getElementById('data_");
            sb.Append(timing.Id.ToString());
            sb.Append("').style.display='block';\" class=\"openModal\">data</a>] ");
        }

        private void PrintDrillDownLink(StringBuilder sb, ITiming timing)
        {
            if (timing.Data == null || !timing.Data.ContainsKey("correlationId")) return;

            var correlationId = timing.Data["correlationId"];

            Guid? drillDownSessionId = null;
            if (DrillDownHandler == null)
            {
                var drillDownSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Data != null && s.Data.ContainsKey("correlationId") && s.Data["correlationId"] == correlationId);
                if (drillDownSession != null) drillDownSessionId = drillDownSession.Id;
            }
            else
            {
                drillDownSessionId = DrillDownHandler(correlationId);
            }

            if (!drillDownSessionId.HasValue) return;

            sb.Append("[<a href=\"./");
            sb.Append(drillDownSessionId);
            sb.Append("\">drill down</a>] ");
        }

        private void PrintDrillUpLink(StringBuilder sb, ITimingSession session)
        {
            if (session.Data == null || !session.Data.ContainsKey("correlationId")) return;

            var correlationId = session.Data["correlationId"];

            Guid? drillUpSessionId = null;
            if (DrillUpHandler == null)
            {
                var drillUpSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Timings != null && s.Timings.Any(t => t.Data != null && t.Data.ContainsKey("correlationId") && t.Data["correlationId"] == correlationId));
                if (drillUpSession != null) drillUpSessionId = drillUpSession.Id;
            }
            else
            {
                drillUpSessionId = DrillUpHandler(correlationId);
            }

            if (!drillUpSessionId.HasValue) return;

            sb.Append("[<a href=\"./");
            sb.Append(drillUpSessionId);
            sb.Append("\">drill up</a>] ");
        }

        private void ApplicationOnError(object sender, EventArgs eventArgs)
        {
            // stop and save profiling results on error

            using (ProfilingSession.Current.Step("Stop on Error")) { }

            ProfilingSession.Stop();
        }
    }
}
