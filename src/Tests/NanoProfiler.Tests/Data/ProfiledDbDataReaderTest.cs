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
using System.Data.Common;
using EF.Diagnostics.Profiling.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EF.Diagnostics.Profiling.Tests.Data
{
    [TestClass]
    public class ProfiledDbDataReaderTest
    {
        [TestMethod]
        public void TestProfiledDataReaderWrapper_Equals()
        {
            var mockReader = new Mock<IDataReader>();
            var mockDbProfiler = new Mock<IDbProfiler>();
            var target = new ProfiledDbDataReader(mockReader.Object, mockDbProfiler.Object);

            Assert.IsTrue(target.Equals(target));
            Assert.IsTrue(target.Equals(mockReader.Object));
            Assert.AreEqual(target.GetHashCode(), mockReader.Object.GetHashCode());
            Assert.IsFalse(target.Equals(null));
            Assert.IsTrue(target == target);
            Assert.IsFalse(target == (ProfiledDbDataReader)null);
            Assert.IsFalse((ProfiledDbDataReader)null == target);
            Assert.IsFalse(target == (IDataReader)null);
            Assert.IsFalse((IDataReader)null == target);
            Assert.IsTrue(target == mockReader.Object);
            Assert.IsTrue(mockReader.Object == target);
            Assert.IsFalse(target != target);
            Assert.IsFalse(target != mockReader.Object);
            Assert.IsFalse(mockReader.Object != target);
            Assert.IsTrue(target != (ProfiledDbDataReader)null);
            Assert.IsTrue((ProfiledDbDataReader)null != target);
            Assert.IsTrue(target != (IDataReader)null);
            Assert.IsTrue((IDataReader)null != target);

            var target2 = new ProfiledDbDataReader(mockReader.Object, mockDbProfiler.Object);
            Assert.IsTrue(target.Equals(target2));
            Assert.IsTrue(target == target2);
            Assert.IsFalse(target != target2);

            var mockReader2 = new Mock<IDataReader>();
            var target3 = new ProfiledDbDataReader(mockReader2.Object, mockDbProfiler.Object);
            Assert.IsFalse(target == target3);
            Assert.IsTrue(target != target3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfiledDataReaderWrapper_ctor_WithInvalidReader()
        {
            var mockSqlProfiler = new Mock<IDbProfiler>();
            new ProfiledDbDataReader(null, mockSqlProfiler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestProfiledDataReaderWrapper_ctor_WithInvalidSqlProfiler()
        {
            var mockReader = new Mock<IDataReader>();
            new ProfiledDbDataReader(mockReader.Object, null);
        }

        [TestMethod]
        public void TestProfiledDataReaderWrapper()
        {
            var mockSqlProfiler = new Mock<IDbProfiler>();

            bool closed = false;
            bool readerFinished = false;
            int depth = 2;
            int fieldCount = 3;
            int ordinal = 4;
            bool boolValue = true;
            byte byteValue = 5;
            long dataOffset = 6;
            byte[] bytesBuffer = new byte[1];
            int bufferOffset = 7;
            int length = 8;
            long bytesValue = 9;
            char charValue = (char)10;
            long charsValue = 11;
            char[] charsBuffer = new char[1];
            string dataTypeName = "test";
            DateTime dateTimeValue = DateTime.Now;
            decimal decimalValue = 12;
            double doubleValue = 13;
            Type fieldType = typeof(string);
            float floatValue = 14;
            Guid guidValue = Guid.NewGuid();
            short int16Value = 15;
            int int32Value = 16;
            long int64Value = 17;
            string name = "test";
            DataTable dataTable = new DataTable();
            string stringValue = "test";
            object value = new object();
            object[] values = new object[1];
            int valuesValue = 18;
            bool isClosed = true;
            bool isDBNull = true;
            bool nextResult = true;
            bool readValue = true;
            int recordsAffected = 20;
            object thisByName = new object();
            object thisByOrdinal = new object();
            bool hasRows = true;

            var mockReader1 = new Mock<IDataReader>();
            mockReader1.Setup(reader => reader.Close()).Callback(() => closed = true);
            mockReader1.Setup(reader => reader.Depth).Returns(depth);
            mockReader1.Setup(reader => reader.FieldCount).Returns(fieldCount);
            mockReader1.Setup(reader => reader.GetBoolean(ordinal)).Returns(boolValue);
            mockReader1.Setup(reader => reader.GetByte(ordinal)).Returns(byteValue);
            mockReader1.Setup(reader => reader.GetBytes(ordinal, dataOffset, bytesBuffer, bufferOffset, length)).Returns(bytesValue);
            mockReader1.Setup(reader => reader.GetChar(ordinal)).Returns(charValue);
            mockReader1.Setup(reader => reader.GetChars(ordinal, dataOffset, charsBuffer, bufferOffset, length)).Returns(charsValue);
            mockReader1.Setup(reader => reader.GetDataTypeName(ordinal)).Returns(dataTypeName);
            mockReader1.Setup(reader => reader.GetDateTime(ordinal)).Returns(dateTimeValue);
            mockReader1.Setup(reader => reader.GetDecimal(ordinal)).Returns(decimalValue);
            mockReader1.Setup(reader => reader.GetDouble(ordinal)).Returns(doubleValue);
            mockReader1.Setup(reader => reader.GetFieldType(ordinal)).Returns(fieldType);
            mockReader1.Setup(reader => reader.GetFloat(ordinal)).Returns(floatValue);
            mockReader1.Setup(reader => reader.GetGuid(ordinal)).Returns(guidValue);
            mockReader1.Setup(reader => reader.GetInt16(ordinal)).Returns(int16Value);
            mockReader1.Setup(reader => reader.GetInt32(ordinal)).Returns(int32Value);
            mockReader1.Setup(reader => reader.GetInt64(ordinal)).Returns(int64Value);
            mockReader1.Setup(reader => reader.GetName(ordinal)).Returns(name);
            mockReader1.Setup(reader => reader.GetOrdinal(name)).Returns(ordinal);
            mockReader1.Setup(reader => reader.GetSchemaTable()).Returns(dataTable);
            mockReader1.Setup(reader => reader.GetString(ordinal)).Returns(stringValue);
            mockReader1.Setup(reader => reader.GetValue(ordinal)).Returns(value);
            mockReader1.Setup(reader => reader.GetValues(values)).Returns(valuesValue);
            mockReader1.Setup(reader => reader.IsClosed).Returns(isClosed);
            mockReader1.Setup(reader => reader.IsDBNull(ordinal)).Returns(isDBNull);
            mockReader1.Setup(reader => reader.NextResult()).Returns(nextResult);
            mockReader1.Setup(reader => reader.Read()).Returns(readValue);
            mockReader1.Setup(reader => reader.RecordsAffected).Returns(recordsAffected);
            mockReader1.Setup(reader => reader[name]).Returns(thisByName);
            mockReader1.Setup(reader => reader[ordinal]).Returns(thisByOrdinal);
            var mockReader2 = new Mock<DbDataReader>();
            var dataRecordEnumerator = new IDataRecord[0].GetEnumerator();
            mockReader2.Setup(reader => reader.GetEnumerator()).Returns(dataRecordEnumerator);
            mockReader2.Setup(reader => reader.HasRows).Returns(hasRows);
            var target1 = new ProfiledDbDataReader(mockReader1.Object, mockSqlProfiler.Object);
            var target2 = new ProfiledDbDataReader(mockReader2.Object, mockSqlProfiler.Object);
            mockSqlProfiler.Setup(sqlProfiler => sqlProfiler.DataReaderFinished(It.IsAny<IDataReader>()))
                .Callback<IDataReader>(a =>
                {
                    Assert.AreEqual(target1, a);
                    readerFinished = true;
                });

            // Close()
            closed = false;
            readerFinished = false;
            target1.Close();
            Assert.IsTrue(closed);
            Assert.IsTrue(readerFinished);

            Assert.AreEqual(depth, target1.Depth);
            Assert.AreEqual(fieldCount, target1.FieldCount);
            Assert.AreEqual(boolValue, target1.GetBoolean(ordinal));
            Assert.AreEqual(byteValue, target1.GetByte(ordinal));
            Assert.AreEqual(bytesValue, target1.GetBytes(ordinal, dataOffset, bytesBuffer, bufferOffset, length));
            Assert.AreEqual(charValue, target1.GetChar(ordinal));
            Assert.AreEqual(charsValue, target1.GetChars(ordinal, dataOffset, charsBuffer, bufferOffset, length));
            Assert.AreEqual(dataTypeName, target1.GetDataTypeName(ordinal));
            Assert.AreEqual(dateTimeValue, target1.GetDateTime(ordinal));
            Assert.AreEqual(decimalValue, target1.GetDecimal(ordinal));
            Assert.AreEqual(doubleValue, target1.GetDouble(ordinal));
            Assert.AreEqual(fieldType, target1.GetFieldType(ordinal));
            Assert.AreEqual(floatValue, target1.GetFloat(ordinal));
            Assert.AreEqual(guidValue, target1.GetGuid(ordinal));
            Assert.AreEqual(int16Value, target1.GetInt16(ordinal));
            Assert.AreEqual(int32Value, target1.GetInt32(ordinal));
            Assert.AreEqual(int64Value, target1.GetInt64(ordinal));
            Assert.AreEqual(name, target1.GetName(ordinal));
            Assert.AreEqual(ordinal, target1.GetOrdinal(name));
            Assert.AreEqual(dataTable, target1.GetSchemaTable());
            Assert.AreEqual(stringValue, target1.GetString(ordinal));
            Assert.AreEqual(value, target1.GetValue(ordinal));
            Assert.AreEqual(valuesValue, target1.GetValues(values));
            Assert.AreEqual(isClosed, target1.IsClosed);
            Assert.AreEqual(isDBNull, target1.IsDBNull(ordinal));
            Assert.AreEqual(nextResult, target1.NextResult());
            Assert.AreEqual(readValue, target1.Read());
            Assert.AreEqual(recordsAffected, target1.RecordsAffected);
            Assert.AreEqual(thisByName, target1[name]);
            Assert.AreEqual(thisByOrdinal, target1[ordinal]);

            // GetEnumerator
            Assert.AreEqual(typeof(DbEnumerator), target1.GetEnumerator().GetType());
            Assert.AreEqual(dataRecordEnumerator, target2.GetEnumerator());

            // HasRows
            Assert.IsTrue(target1.HasRows);
            Assert.IsTrue(target2.HasRows);
        }

    }
}
