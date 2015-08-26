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
using EF.Diagnostics.Profiling.Timing;
using ServiceStack.Text;

namespace EF.Diagnostics.Profiling.Web.Extensions.Handlers
{
    internal static class ImportProfilingResultsHelper
    {
        public static List<IProfiler> Parse(string jsonArrayString)
        {
            var profilers = JsonSerializer.DeserializeFromString<List<SerializableProfiler>>(jsonArrayString);
            return profilers.Select(profiler => profiler as IProfiler).ToList();
        }

        #region Nested Classes

        private abstract class SerializableTimingBase : ITiming
        {
            public long DurationMilliseconds { get; set; }

            public string ExecuteType { get; set; }

            public Guid Id { get; set; }

            public string MachineName { get; set; }

            public string Name { get; set; }

            public Guid? ParentId { get; set; }

            public long StartMilliseconds { get; set; }

            public DateTime Started { get; set; }

            public HashSet<string> Tags { get; set; }

            public string Type { get; set; }

            #region ITiming Members

            long ITiming.DurationMilliseconds
            {
                get { return DurationMilliseconds; }
            }

            string ITiming.ExecuteType
            {
                get
                {
                    return ExecuteType;
                }
                set
                {
                    ExecuteType = value;
                }
            }

            Guid ITiming.Id
            {
                get { return Id; }
            }

            string ITiming.MachineName
            {
                get { return MachineName; }
            }

            string ITiming.Name
            {
                get
                {
                    return Name;
                }
                set
                {
                    Name = value;
                }
            }

            Guid? ITiming.ParentId
            {
                get { return ParentId; }
            }

            long ITiming.StartMilliseconds
            {
                get { return StartMilliseconds; }
            }

            DateTime ITiming.Started
            {
                get { return Started; }
            }

            TagCollection ITiming.Tags
            {
                get
                {
                    return Tags == null ? null : new TagCollection(Tags);
                }
                set
                {
                    Tags = value;
                }
            }

            string ITiming.Type
            {
                get { return Type; }
            }

            #endregion
        }

        private class SerializableStepTiming : SerializableTimingBase
        {

        }

        private class SerializableCustomTiming : SerializableTimingBase
        {
            public int? InputSize { get; set; }

            public string InputData { get; set; }

            public int OutputSize { get; set; }

            public long? OutputStartMilliseconds { get; set; }
        }

        private class SerializableProfiler : SerializableTimingBase, IProfiler
        {
            public string Client { get; set; }

            public List<SerializableCustomTiming> CustomTimings { get; set; }

            public string LocalAddress { get; set; }

            public List<SerializableStepTiming> StepTimings { get; set; }

            #region IProfiler Members

            void IProfiler.AddCustomTiming(CustomTiming customTiming)
            {
                throw new NotImplementedException();
            }

            void IProfiler.AddStepTiming(StepTiming stepTiming)
            {
                throw new NotImplementedException();
            }

            string IProfiler.Client
            {
                get
                {
                    return Client;
                }
                set
                {
                    Client = value;
                }
            }

            private List<CustomTiming> _customTimings;

            IEnumerable<CustomTiming> IProfiler.CustomTimings
            {
                get
                {
                    if (_customTimings == null)
                    {
                        _customTimings = new List<CustomTiming>();

                        if (CustomTimings != null)
                        {
                            for (var i = 0; i < CustomTimings.Count; ++i)
                            {
                                _customTimings.Add(CreateCustomTiming(CustomTimings[i]));
                            }
                        }
                    }

                    return _customTimings;
                }
            }

            private CustomTiming CreateCustomTiming(SerializableCustomTiming sourceCustomTiming)
            {
                // ensure parentId
                ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = sourceCustomTiming.ParentId;

                var customTiming = new CustomTiming(this, sourceCustomTiming.Type, sourceCustomTiming.Name)
                {
                    DurationMilliseconds = sourceCustomTiming.DurationMilliseconds,
                    ExecuteType = sourceCustomTiming.ExecuteType,
                    StartMilliseconds = sourceCustomTiming.StartMilliseconds,
                    Tags = sourceCustomTiming.Tags == null ? null : new TagCollection(sourceCustomTiming.Tags),
                    InputData = sourceCustomTiming.InputData,
                    InputSize = sourceCustomTiming.InputSize,
                    OutputSize = sourceCustomTiming.OutputSize,
                    OutputStartMilliseconds = sourceCustomTiming.OutputStartMilliseconds
                };

                ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = null;

                return customTiming;
            }

            IDisposable IProfiler.Ignore()
            {
                throw new NotImplementedException();
            }

            string IProfiler.LocalAddress
            {
                get
                {
                    return LocalAddress;
                }
                set
                {
                    LocalAddress = value;
                }
            }

            IProfilingStep IProfiler.Step(string name, IEnumerable<string> tags, string executeType)
            {
                throw new NotImplementedException();
            }

            private List<StepTiming> _stepTimings;

            IEnumerable<StepTiming> IProfiler.StepTimings
            {
                get
                {
                    if (_stepTimings == null)
                    {
                        _stepTimings = new List<StepTiming>();

                        if (StepTimings != null)
                        {
                            var sortedStepTimings = SortStepTimingsByHiearachy(StepTimings);
                            for (var i = 0; i < sortedStepTimings.Count; ++i)
                            {
                                _stepTimings.Add(CreateStepTiming(sortedStepTimings[i]));
                            }
                        }
                    }

                    return _stepTimings;
                }
            }

            private List<SerializableStepTiming> SortStepTimingsByHiearachy(List<SerializableStepTiming> stepTimings)
            {
                var sortedList = new List<SerializableStepTiming>();

                if (stepTimings == null || stepTimings.Count == 0)
                {
                    return sortedList;
                }

                AddSelfAndChildrenStepTimings(sortedList, stepTimings[0], stepTimings);

                return sortedList;
            }

            private void AddSelfAndChildrenStepTimings(
                List<SerializableStepTiming> sortedList, SerializableStepTiming stepTiming, List<SerializableStepTiming> stepTimings)
            {
                sortedList.Add(stepTiming);

                for (var i = 0; i < stepTimings.Count; ++i)
                {
                    if (stepTimings[i].ParentId == stepTiming.Id)
                    {
                        AddSelfAndChildrenStepTimings(sortedList, stepTimings[i], stepTimings);
                    }
                }
            }

            private StepTiming CreateStepTiming(SerializableStepTiming sourceStepTiming)
            {
                // ensure parentId
                ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = sourceStepTiming.ParentId;

                var stepTiming = new StepTiming(this, sourceStepTiming.Name)
                {
                    DurationMilliseconds = sourceStepTiming.DurationMilliseconds,
                    ExecuteType = sourceStepTiming.ExecuteType,
                    StartMilliseconds = sourceStepTiming.StartMilliseconds,
                    Tags = sourceStepTiming.Tags == null ? null : new TagCollection(sourceStepTiming.Tags)
                };

                // fix Id
                if (CustomTimings != null)
                {
                    for (var i = 0; i < CustomTimings.Count; ++i)
                    {
                        if (CustomTimings[i].ParentId == sourceStepTiming.Id)
                        {
                            CustomTimings[i].ParentId = stepTiming.Id;
                        }
                    }
                }
                for (var i = 0; i < StepTimings.Count; ++i)
                {
                    if (StepTimings[i].ParentId == sourceStepTiming.Id)
                    {
                        StepTimings[i].ParentId = stepTiming.Id;
                    }
                }
                sourceStepTiming.Id = stepTiming.Id;

                ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = null;

                return stepTiming;
            }

            void IProfiler.Stop(bool discardResults)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion
    }
}
