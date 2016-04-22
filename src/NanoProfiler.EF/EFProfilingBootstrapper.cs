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

using System.Collections.Concurrent;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SqlClient;

namespace EF.Diagnostics.Profiling.EF
{
    /// <summary>
    /// Bootsrap the profiling feature for Entity Framework 6.
    /// </summary>
    public static class EFProfilingBootstrapper
    {
        private static readonly ConcurrentDictionary<DbProviderServices, DbProviderServices> ProviderCache = new ConcurrentDictionary<DbProviderServices, DbProviderServices>();

        /// <summary>
        /// Registers the WrapProviderService method with the Entity Framework 6 DbConfiguration as a replacement service for DbProviderServices.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                DbConfiguration.Loaded += (_, a) => a.ReplaceService<DbProviderServices>(
                    (services, o) => WrapProviderService(services));
            }
            catch (SqlException ex)
            {
                // Try to prevent tripping this harmless Exception when initializing the DB
                // Issue in EF6 upgraded from EF5 on first db call in debug mode: http://entityframework.codeplex.com/workitem/594
                if (!ex.Message.Contains("Invalid column name 'ContextKey'"))
                {
                    throw;
                }
            }
        }

        private static DbProviderServices WrapProviderService(DbProviderServices services)
        {
            if (ProviderCache.ContainsKey(services))
            {
                return ProviderCache[services];
            }

            var instance = new EFProfiledDbProviderServices(services);
            ProviderCache.TryAdd(services, instance);

            return instance;
        }
    }
}
