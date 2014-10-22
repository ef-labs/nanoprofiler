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
using System.Transactions;
using EF.Diagnostics.Profiling.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using IsolationLevel = System.Data.IsolationLevel;

namespace EF.Diagnostics.Profiling.Tests.Data
{
    [TestClass]
    public class ProfiledDbConnectionTest
    {
        [TestMethod]
        public void TestProfiledDbConnectionWithIDbConnection()
        {
            var mockConnection = new Mock<IDbConnection>();
            var mockDbProfiler = new Mock<IDbProfiler>();

            var target = new ProfiledDbConnection(mockConnection.Object, mockDbProfiler.Object);

            // test BeginDbTransaction()
            var beginTransCalled = false;
            var isoLevel = IsolationLevel.Chaos;
            var mockTransaction = new Mock<IDbTransaction>();
            mockTransaction.Setup(t => t.IsolationLevel).Returns(isoLevel);
            mockConnection.Setup(c => c.BeginTransaction(isoLevel)).Callback<IsolationLevel>(a => beginTransCalled = true).Returns(mockTransaction.Object);
            var transaction = target.BeginTransaction(isoLevel);
            Assert.AreNotEqual(mockTransaction.Object, transaction);
            Assert.AreEqual(isoLevel, transaction.IsolationLevel);
            Assert.IsTrue(beginTransCalled);

            // test ChangeDatabase()
            var dbName = "test db";
            var changeDatabaseCalled = false;
            mockConnection.Setup(c => c.ChangeDatabase(dbName)).Callback<string>(a => changeDatabaseCalled = true);
            target.ChangeDatabase(dbName);
            Assert.IsTrue(changeDatabaseCalled);

            // test Close()
            var closeCalled = false;
            mockConnection.Setup(c => c.Close()).Callback(() => closeCalled = true);
            target.Close();
            Assert.IsTrue(closeCalled);

            // test ConnectionString
            var connStr1 = "test 1;";
            var connStr2 = "test 2";
            var connectionStringSet = false;
            mockConnection.Setup(c => c.ConnectionString).Returns(connStr1);
            mockConnection.SetupSet(c => c.ConnectionString = It.IsAny<string>()).Callback<string>(a =>
            {
                Assert.AreEqual(connStr2, a);
                connectionStringSet = true;
            });
            Assert.AreEqual(connStr1, target.ConnectionString);
            target.ConnectionString = connStr2;
            Assert.IsTrue(connectionStringSet);

            // test CreateDbCommand()
            var createDbCommandCalled = false;
            var mockCommand = new Mock<IDbCommand>();
            var sql = "test sql";
            mockCommand.Setup(c => c.CommandText).Returns(sql);
            mockConnection.Setup(c => c.CreateCommand()).Callback(() => createDbCommandCalled = true).Returns(mockCommand.Object);
            var command = target.CreateCommand();
            Assert.AreNotEqual(mockCommand.Object, command);
            Assert.AreEqual(sql, command.CommandText);
            Assert.IsTrue(createDbCommandCalled);

            // test Database
            mockConnection.Setup(c => c.Database).Returns(dbName);
            Assert.AreEqual(dbName, target.Database);

            // test Open()
            var openCalled = false;
            mockConnection.Setup(c => c.Open()).Callback(() => openCalled = true);
            target.Open();
            Assert.IsTrue(openCalled);

            // test State
            var connState = ConnectionState.Executing;
            mockConnection.Setup(c => c.State).Returns(connState);
            Assert.AreEqual(connState, target.State);

            // test ConnectionTimeout
            var timeout = 1;
            mockConnection.Setup(c => c.ConnectionTimeout).Returns(timeout);
            Assert.AreEqual(timeout, target.ConnectionTimeout);

        }

        [TestMethod]
        public void TestProfiledDbConnectionWithProfiledDbConnection()
        {
            var mockConnection = new Mock<DbConnection>();
            var mockDbProfiler = new Mock<IDbProfiler>();

            var target = new ProfiledDbConnection(mockConnection.Object, mockDbProfiler.Object);

            // test BeginDbTransaction()
            var beginTransCalled = false;
            var isoLevel = IsolationLevel.Chaos;
            var mockTransaction = new Mock<IDbTransaction>();
            var profiledTransaction = new ProfiledDbTransaction(mockTransaction.Object, mockDbProfiler.Object);
            mockConnection.Protected().Setup<DbTransaction>("BeginDbTransaction", isoLevel).Callback<IsolationLevel>(a => beginTransCalled = true).Returns(profiledTransaction);
            var transaction = target.BeginTransaction(isoLevel);
            Assert.AreEqual(profiledTransaction, transaction);
            Assert.IsTrue(beginTransCalled);

            // test CreateDbCommand()
            var createDbCommandCalled = false;
            var mockCommand = new Mock<IDbCommand>();
            var profiledCommand = new ProfiledDbCommand(mockCommand.Object, mockDbProfiler.Object);
            mockConnection.Protected().Setup<DbCommand>("CreateDbCommand").Callback(() => createDbCommandCalled = true).Returns(profiledCommand);
            var command = target.CreateCommand();
            Assert.AreEqual(profiledCommand, command);
            Assert.IsTrue(createDbCommandCalled);

            // test DataSource
            var dataSource = "test";
            mockConnection.Setup(c => c.DataSource).Returns(dataSource);
            Assert.AreEqual(dataSource, target.DataSource);

            // test ServerVersion
            var serverVersion = "test";
            mockConnection.Setup(c => c.ServerVersion).Returns(serverVersion);
            Assert.AreEqual(serverVersion, target.ServerVersion);

            // test Dispose()
            var disposeCalled = false;
            mockConnection.Protected().Setup("Dispose", true).Callback<bool>(a => disposeCalled = true);
            mockConnection.Setup(c => c.State).Returns(ConnectionState.Executing);
            target.Dispose();
            Assert.IsTrue(disposeCalled);
        }
    }
}
