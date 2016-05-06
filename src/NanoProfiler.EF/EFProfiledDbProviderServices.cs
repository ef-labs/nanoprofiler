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
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Reflection;
using EF.Diagnostics.Profiling.Data;

namespace EF.Diagnostics.Profiling.EF
{
    /// <summary>
    /// Wrapper for a database provider factory to enable profiling
    /// </summary>
    public class EFProfiledDbProviderServices : DbProviderServices
    {
        private readonly DbProviderServices _services;

        public EFProfiledDbProviderServices(DbProviderServices services)
        {
            _services = services;
        }

        public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
        {
            return _services.CreateCommandDefinition(prototype);
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return _services.GetProviderManifest(manifestToken);
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            return _services.GetProviderManifestToken(connection);
        }

        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            var cmdDef = _services.CreateCommandDefinition(providerManifest, commandTree);
            var cmd = cmdDef.CreateCommand();
            return CreateCommandDefinition(new ProfiledDbCommand(cmd, () =>
                {
                    var profilingSession = ProfilingSession.Current;
                    if (profilingSession == null) return null;

                    return new DbProfiler(profilingSession.Profiler);
                })
                {
                    Connection = cmd.Connection
                });
        }

        protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            return _services.CreateDatabaseScript(providerManifestToken, storeItemCollection);
        }

        public override object GetService(Type type, object key)
        {
            return _services.GetService(type, key);
        }

        public override IEnumerable<object> GetServices(Type type, object key)
        {
            return _services.GetServices(type, key);
        }

        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            // if this is available in _services, use it
            var setDbParameterValueMethod = _services.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.Name.Equals("SetDbParameterValue"));
            if (setDbParameterValueMethod != null)
            {
                setDbParameterValueMethod.Invoke(_services, new[] { parameter, parameterType, value });
                return;
            }

            // this should never need to be called, but just in case, get the Provider Value
            if (value is DbGeography)
            {
                value = ((DbGeography)value).ProviderValue;
            }
            base.SetDbParameterValue(parameter, parameterType, value);
        }

        protected override DbCommand CloneDbCommand(DbCommand fromDbCommand)
        {
            if (fromDbCommand == null) return null;

            var cloneable = fromDbCommand as ICloneable;
            if (cloneable != null) return cloneable.Clone() as DbCommand;

            throw new NotSupportedException(fromDbCommand.GetType() + " does not implement ICloneable!");
        }

        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            return _services.DatabaseExists(connection, commandTimeout, storeItemCollection);
        }

        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, Lazy<StoreItemCollection> storeItemCollection)
        {
            return _services.DatabaseExists(connection, commandTimeout, storeItemCollection);
        }

        protected override DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string manifestToken)
        {
            return _services.GetSpatialDataReader(fromReader, manifestToken);
        }

        protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            _services.CreateDatabase(connection, commandTimeout, storeItemCollection);
        }

        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            _services.DeleteDatabase(connection, commandTimeout, storeItemCollection);
        }

        protected override DbSpatialServices DbGetSpatialServices(string manifestToken)
        {
            return _services.GetSpatialServices(manifestToken);
        }

        public override void RegisterInfoMessageHandler(DbConnection connection, Action<string> handler)
        {
            _services.RegisterInfoMessageHandler(connection, handler);
        }
    }
}
