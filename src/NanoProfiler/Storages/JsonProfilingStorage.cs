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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

using EF.Diagnostics.Profiling.Timing;

using slf4net;

namespace EF.Diagnostics.Profiling.Storages
{
    /// <summary>
    /// A <see cref="IProfilingStorage"/> implementation which persists profiling results as json via slf4net.
    /// </summary>
    public class JsonProfilingStorage : ProfilingStorageBase
    {
        private readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() => LoggerFactory.GetLogger(typeof(JsonProfilingStorage)));

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="JsonProfilingStorage"/>.
        /// </summary>
        /// <param name="maxQueueLength">The max queue length of the internal worker queue.</param>
        public JsonProfilingStorage(int maxQueueLength = 1000)
            : base(maxQueueLength)
        {
        }

        #endregion

        #region JsonProfilingStorage Members

        /// <summary>
        /// Serialize data to JSON string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual string Serialize(object data)
        {
            if (data == null)
            {
                return null;
            }

            var serializer = new DataContractJsonSerializer(data.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, data);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }        

        /// <summary>
        /// Logs the timing data via slf4net logger.
        /// </summary>
        /// <param name="timingData">The timing to be serialized to JSON and be logged.</param>
        /// <param name="aggregations">The aggregations to be merged to timing data.</param>
        protected void LogTimingData(TimingDataBase timingData
            , params KeyValuePair<string, TimingAggregation>[] aggregations)
        {
            if (!_logger.Value.IsInfoEnabled)
            {
                return;
            }

            var json = Serialize(timingData);

            // merge aggregations to JSON of timing data
            if (aggregations != null && aggregations.Length > 0)
            {
                var sb = new StringBuilder(json.Substring(0, json.Length - 1));

                foreach (var aggr in aggregations)
                {
                    var aggrData = new AggregationData();
                    aggrData.Count = aggr.Value.Count;
                    aggrData.Duration = aggr.Value.TotalDurationMilliseconds;

                    var jsonAggr = Serialize(aggrData);
                    var aggrType = aggr.Key.ToLowerInvariant();
                    sb.Append(",");
                    sb.Append(
                        jsonAggr.Substring(1, jsonAggr.Length - 2)
                        .Replace("count", aggrType + "Count")
                        .Replace("duration", aggrType + "Duration"));
                }

                sb.Append("}");
                json = sb.ToString();
            }

            _logger.Value.Info(json);
        }

        /// <summary>
        /// Saves the result of an <see cref="IProfiler"/>.
        /// </summary>
        /// <param name="profiler"></param>
        protected override void SaveProfiler(IProfiler profiler)
        {
            if (!_logger.Value.IsInfoEnabled)
            {
                return;
            }

            if (profiler == null)
            {
                return;
            }

            SaveSessionTiming(profiler);

            // Save each step timings
            if (profiler.StepTimings != null && profiler.StepTimings.Any())
            {
                foreach (var stepTiming in profiler.StepTimings)
                {
                    SaveStepTiming(profiler, stepTiming);
                }
            }

            // Save each custom timings
            if (profiler.CustomTimings != null && profiler.CustomTimings.Any())
            {
                foreach (var customTiming in profiler.CustomTimings)
                {
                    SaveCustomTiming(profiler, customTiming);
                }
            }
        }

        /// <summary>
        /// Saves a session timing.
        /// </summary>
        /// <param name="profiler">The profiler.</param>
        protected virtual void SaveSessionTiming(IProfiler profiler)
        {
            if (profiler == null)
            {
                return;
            }

            try
            {
                var sessionData = new SessionTimingData();
                SetBasicTimingProperties(profiler, sessionData);

                sessionData.SessionId = profiler.Id;
                sessionData.Client = profiler.Client;
                sessionData.LocalAddress = profiler.LocalAddress;

                var aggrs = GetCustomTimingAggregationsByType(profiler.CustomTimings);
                LogTimingData(sessionData, aggrs);
            }
            catch (Exception ex)
            {
                _logger.Value.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Saves a step timing.
        /// </summary>
        /// <param name="profiler">The profiler.</param>
        /// <param name="stepTiming">The step timing.</param>
        protected virtual void SaveStepTiming(IProfiler profiler, StepTiming stepTiming)
        {
            if (profiler == null || stepTiming == null)
            {
                return;
            }

            try
            {
                var stepData = new StepTimingData();
                SetBasicTimingProperties(stepTiming, stepData);

                stepData.SessionId = profiler.Id;

                LogTimingData(stepData);
            }
            catch (Exception ex)
            {
                _logger.Value.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Saves a custom timing.
        /// </summary>
        /// <param name="profiler">The profiler.</param>
        /// <param name="customTiming">The custom timing.</param>
        protected virtual void SaveCustomTiming(IProfiler profiler, CustomTiming customTiming)
        {
            if (profiler == null || customTiming == null)
            {
                return;
            }

            try
            {
                var customData = new CustomTimingData();
                SetBasicTimingProperties(customTiming, customData);

                customData.SessionId = profiler.Id;
                customData.InputSize = customTiming.InputSize;
                customData.InputData = customTiming.InputData;
                customData.OutputSize = customTiming.OutputSize;
                customData.OutputStartMilliseconds = customTiming.OutputStartMilliseconds;

                LogTimingData(customData);
            }
            catch (Exception ex)
            {
                _logger.Value.Error(ex, ex.Message);
            }
        }

        /// <summary>
        /// Gets the aggregation of specified <see cref="CustomTiming"/>s by type.
        /// </summary>
        /// <param name="customTimings">The <see cref="CustomTiming"/>s.</param>
        /// <returns>Returns the aggregation of specified <see cref="CustomTiming"/>s by type.</returns>
        protected static KeyValuePair<string, TimingAggregation>[] GetCustomTimingAggregationsByType(
            IEnumerable<CustomTiming> customTimings)
        {
            if (customTimings == null)
            {
                return null;
            }

            var results = new Dictionary<string, TimingAggregation>();

            foreach (var customTiming in customTimings)
            {
                TimingAggregation aggr;
                if (!results.TryGetValue(customTiming.Type, out aggr))
                {
                    aggr = new TimingAggregation();
                    results[customTiming.Type] = aggr;
                }
                ++aggr.Count;
                aggr.TotalDurationMilliseconds += customTiming.DurationMilliseconds;
            }

            return results.ToArray();
        }

        #endregion

        #region Private Methods

        private static void SetBasicTimingProperties(ITiming timing, TimingDataBase timingData)
        {
            if (timing == null || timingData == null)
            {
                return;
            }

            timingData.DurationMilliseconds = timing.DurationMilliseconds;
            timingData.ExecuteType = timing.ExecuteType;
            timingData.Id = timing.Id;
            timingData.MachineName = timing.MachineName;
            timingData.Name = timing.Name;
            timingData.ParentId = timing.ParentId;
            timingData.StartMilliseconds = timing.StartMilliseconds;
            timingData.Started = ToSortableDateTimeString(timing.Started);
            if (timing.Tags != null && timing.Tags.Any())
            {
                timingData.Tags = new List<string>(timing.Tags);
            }
            timingData.Type = timing.Type;
        }

        private static string ToSortableDateTimeString(DateTime dt)
        {
            return dt.ToString("s");
        }

        #endregion

        #region Nested Classes

        [DataContract]
        private class AggregationData
        {
            [DataMember(Name = "count")]
            public int Count { get; set; }

            [DataMember(Name = "duration")]
            public long Duration { get; set; }
        }

        #endregion
    }
}
