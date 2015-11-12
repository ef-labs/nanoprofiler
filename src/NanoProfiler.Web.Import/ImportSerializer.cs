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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EF.Diagnostics.Profiling.Web.Import
{
    /// <summary>
    /// Serializer for importing profiling results.
    /// </summary>
    public static class ImportSerializer
    {
        /// <summary>
        /// Serialize a list of sessions.
        /// </summary>
        /// <param name="sessions"></param>
        /// <returns></returns>
        public static string SerializeSessions(IEnumerable<ITimingSession> sessions)
        {
            if (sessions == null) return "[]";

            var json = JsonConvert.SerializeObject(sessions.ToArray(), new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver  = new CamelCasePropertyNamesContractResolver()
                });
            return json;
        }

        /// <summary>
        /// Deserialize a list of sessions.
        /// </summary>
        /// <param name="jsonArrayString"></param>
        /// <returns></returns>
        public static IEnumerable<ITimingSession> DeserializeSessions(string jsonArrayString)
        {
            var sessions = JsonConvert.DeserializeObject<TimingSession[]>(jsonArrayString, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>(
                    new JsonConverter[]
                        {
                            new ConcurrentQueueDeserializationConverter(), 
                            new TimingListDeserializationConverter(),
                            new TimingDeserializationConverter(), 
                        })
            });

            return sessions;
        }

        #region Nested Classes

        private class TimingListDeserializationConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(IEnumerable<ITiming>);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return serializer.Deserialize<Timing[]>(reader);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        private class TimingDeserializationConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ITiming);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return serializer.Deserialize<Timing>(reader);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        private class ConcurrentQueueDeserializationConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ConcurrentQueue<>);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var objType = objectType.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(objType);
                var list = serializer.Deserialize(reader, listType);
                var bagType = typeof(ConcurrentQueue<>).MakeGenericType(objType);
                var instance = Activator.CreateInstance(bagType, list);
                return instance;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
