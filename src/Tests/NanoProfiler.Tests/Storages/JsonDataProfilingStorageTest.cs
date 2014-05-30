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
using System.Data;

using EF.Diagnostics.Profiling.Data;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timing;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net.Resolvers;

namespace EF.Diagnostics.Profiling.Tests.Storages
{
    [TestClass]
    public class JsonDataProfilingStorageTest
    {
        [TestMethod]
        public void TestJsonProfilingStorage()
        {
            slf4net.LoggerFactory.SetFactoryResolver(new SimpleFactoryResolver(item => Console.WriteLine(item.Message))); 
            
            var target = new JsonProfilingStorage(ProfilingStorageBase.Inline);

            // save empty result should not throw exception
            target.SaveResult(null);

            var name = "test";
            var stepName = "step1";
            var profiler = new Profiler(name, target, new [] { "test", "test2" }) as IProfiler;
            var mockParameter = new Mock<IDataParameter>();
            mockParameter.Setup(p => p.ParameterName).Returns("p1");
            mockParameter.Setup(p => p.Value).Returns(1);
            mockParameter.Setup(p => p.Direction).Returns(ParameterDirection.Input);
            var mockParameterDBNull = new Mock<IDbDataParameter>();
            mockParameterDBNull.Setup(p => p.DbType).Returns(DbType.Binary);
            mockParameterDBNull.Setup(p => p.Value).Returns(DBNull.Value);
            mockParameterDBNull.Setup(p => p.Direction).Returns(ParameterDirection.Input);
            var mockParameterBinary = new Mock<IDataParameter>();
            mockParameterBinary.Setup(p => p.DbType).Returns(DbType.Binary);
            mockParameterBinary.Setup(p => p.Value).Returns(new byte[100]);
            mockParameterBinary.Setup(p => p.Direction).Returns(ParameterDirection.Input);
            var mockParameterBinaryTooBig = new Mock<IDbDataParameter>();
            mockParameterBinaryTooBig.Setup(p => p.DbType).Returns(DbType.Binary);
            mockParameterBinaryTooBig.Setup(p => p.Value).Returns(new byte[0x200 + 1]);
            mockParameterBinaryTooBig.Setup(p => p.Direction).Returns(ParameterDirection.Input);
            var mockParameterDateTime = new Mock<IDbDataParameter>();
            mockParameterDateTime.Setup(p => p.DbType).Returns(DbType.DateTime);
            mockParameterDateTime.Setup(p => p.Value).Returns(DateTime.Now);
            mockParameterDateTime.Setup(p => p.Direction).Returns(ParameterDirection.Input);
            var mockParameterEnum = new Mock<IDbDataParameter>();
            mockParameterEnum.Setup(p => p.DbType).Returns(DbType.Int32);
            mockParameterEnum.Setup(p => p.Value).Returns(DbType.Boolean);
            mockParameterEnum.Setup(p => p.Direction).Returns(ParameterDirection.Input);
            var mockParameterXml = new Mock<IDbDataParameter>();
            mockParameterXml.Setup(p => p.DbType).Returns(DbType.Xml);
            mockParameterXml.Setup(p => p.Value).Returns("<xml />");
            mockParameterXml.Setup(p => p.Direction).Returns(ParameterDirection.Input);
            var mockParameterCollection = new Mock<IDataParameterCollection>();
            mockParameterCollection.Setup(collections => collections.GetEnumerator()).Returns(new IDataParameter[] { mockParameter.Object, mockParameterDBNull.Object, mockParameterBinary.Object, mockParameterBinaryTooBig.Object, mockParameterDateTime.Object, mockParameterEnum.Object, mockParameterXml.Object }.GetEnumerator());
            mockParameterCollection.Setup(collection => collection.Count).Returns(1);
            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(cmd => cmd.CommandText).Returns("test sql");
            mockCommand.Setup(cmd => cmd.Parameters).Returns(mockParameterCollection.Object);
            var dbTiming = new DbTiming(
                profiler, DbExecuteType.Reader, mockCommand.Object);
            using (profiler.Step(stepName, null, null))
            {
                profiler.AddCustomTiming(dbTiming);
                profiler.AddCustomTiming(new CustomTiming(profiler, "custom", "custom"));
            }
            profiler.Stop();

            // save normal result should not throw exception
            target.SaveResult(profiler);
            slf4net.LoggerFactory.Reset();
        }
    }
}
