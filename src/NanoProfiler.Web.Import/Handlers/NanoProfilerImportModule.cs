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
using System.Net;
using System.Web;
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling.Web.Import.Handlers
{
    /// <summary>
    /// A module supports import and export profiling sessions.
    /// </summary>
    public sealed class NanoProfilerImportModule : IHttpModule
    {
        private const string ViewUrl = "/nanoprofiler/view";
        private const string Import = "import";
        private const string Export = "export";
        private const string QueryString = "QUERY_STRING";

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        public void Init(HttpApplication application)
        {
            application.BeginRequest += ApplicationOnBeginRequest;
        }

        #endregion

        #region Private Methods

        private void ApplicationOnBeginRequest(object sender, EventArgs eventArgs)
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return;
            }

            var path = context.Request.Path.TrimEnd('/');

            if (path.EndsWith(ViewUrl, StringComparison.OrdinalIgnoreCase))
            {
                var import = context.Request.QueryString[Import];
                if (Uri.IsWellFormedUriString(import, UriKind.Absolute))
                {
                    ImportSessionsFromUrl(import);
                }

                if (context.Request.ServerVariables[QueryString] == Export)
                {
                    context.Response.ContentType = "application/json";

                    context.Response.Write(ImportHelper.SerializeSessions(ProfilingSession.CircularBuffer));
                    context.Response.End();
                }
            }
        }

        private void ImportSessionsFromUrl(string importUrl)
        {
            IEnumerable<ITimingSession> sessions = null;

            var request = WebRequest.Create(importUrl);
            using (var response = request.GetResponse() as HttpWebResponse)
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = stream.ReadToEnd();
                    sessions = ImportHelper.DeserializeSessions(content);
                }
            }

            if (sessions == null)
            {
                return;
            }

            if (ProfilingSession.CircularBuffer == null)
            {
                return;
            }

            var existingIds = ProfilingSession.CircularBuffer.Select(session => session.Id).ToList();
            foreach (var session in sessions)
            {
                if (!existingIds.Contains(session.Id))
                {
                    ProfilingSession.CircularBuffer.Add(session);
                }
            }
        }

        #endregion
    }
}
