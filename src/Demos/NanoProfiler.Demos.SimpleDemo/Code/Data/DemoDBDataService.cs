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

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using EF.Diagnostics.Profiling;
using EF.Diagnostics.Profiling.Data;

using NanoProfiler.Demos.SimpleDemo.Code.Models;
using NanoProfiler.Demos.SimpleDemo.Unity;
using System.Threading.Tasks;
using System.Data.Common;

namespace NanoProfiler.Demos.SimpleDemo.Code.Data
{
    public interface IDemoDBDataService
    {
        [ProfiledMethod]
        List<DemoData> LoadActiveDemoData();

        [ProfiledMethod]
        Task<List<DemoData>> LoadActiveDemoDataAsync();

        [ProfiledMethod]
        int LoadActiveDemoDataCount();

        [ProfiledMethod]
        Task<int> LoadActiveDemoDataCountAsync();

        [ProfiledMethod]
        DataSet LoadActiveDemoDataWithDataAdapter();
    }

    public class DemoDBDataService : IDemoDBDataService
    {
        public List<DemoData> LoadActiveDemoData()
        {
            using (ProfilingSession.Current.Step("Data.LoadActiveDemoData"))
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "select Id, Name from [Table] where IsActive = @IsActive";
                        cmd.Parameters.Add(new SqlParameter("@IsActive", 1));

                        using (var reader = cmd.ExecuteReader())
                        {
                            var results = new List<DemoData>();
                            while (reader.Read())
                            {
                                results.Add(new DemoData { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                            }
                            return results;
                        }
                    }
                }
            }
        }

        public async Task<List<DemoData>> LoadActiveDemoDataAsync()
        {
            using (ProfilingSession.Current.Step("Data.LoadActiveDemoDataAsync"))
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "WAITFOR DELAY '00:00:01'; select Id, Name from [Table] where IsActive = @IsActive";
                        cmd.Parameters.Add(new SqlParameter("@IsActive", 1));

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var results = new List<DemoData>();
                            while (await reader.ReadAsync())
                            {
                                results.Add(new DemoData { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                            }
                            return results;
                        }
                    }
                }
            }
        }

        public int LoadActiveDemoDataCount()
        {
            using (ProfilingSession.Current.Step("Data.LoadActiveDemoDataCount"))
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "select COUNT(*) from [Table] where IsActive = @IsActive";
                        cmd.Parameters.Add(new SqlParameter("@IsActive", 1));
                        return (int)cmd.ExecuteScalar();
                    }
                }
            }
        }

        public async Task<int> LoadActiveDemoDataCountAsync()
        {
            using (ProfilingSession.Current.Step("Data.LoadActiveDemoDataCountAsync"))
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "WAITFOR DELAY '00:00:01'; select COUNT(*) from [Table] where IsActive = @IsActive";
                        cmd.Parameters.Add(new SqlParameter("@IsActive", 1));
                        return await cmd.ExecuteScalarAsync().ContinueWith(r => (int)r.Result);
                    }
                }
            }
        }

        public DataSet LoadActiveDemoDataWithDataAdapter()
        {
            using (ProfilingSession.Current.Step("Data.LoadActiveDemoDataWithDataAdapter"))
            {
                using (var conn = new SqlConnection(@"Server=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\SimpleDemoDB.mdf;Database=SimpleDemoDB;Trusted_Connection=Yes;"))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "select Id, Name from [Table] where IsActive = @IsActive";
                        cmd.Parameters.Add(new SqlParameter("@IsActive", 1));

                        var dataAdapter = new SqlDataAdapter(cmd);
                        var ds = new DataSet("SimpleDemoDB");

                        // the simplest way to enable DB profiling is to hook the DbConnection like in the method above.
                        // But if for any reason, you could not hook profiling at the DbConnection,
                        // you could also hook it inline at DataAdapter, DataReader or DbCommand.
                        if (ProfilingSession.Current != null)
                        {
                            var dbProfiler = new DbProfiler(ProfilingSession.Current.Profiler);
                            var profiledDataAdapter = new ProfiledDbDataAdapter(dataAdapter, dbProfiler);
                            profiledDataAdapter.Fill(ds);
                        }
                        else
                        {
                            dataAdapter.Fill(ds);
                        }
                    }
                }
            }

            return null;
        }

        private DbConnection GetConnection()
        {
            var conn = new SqlConnection(@"Server=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\SimpleDemoDB.mdf;Database=SimpleDemoDB;Trusted_Connection=Yes;");

            if (ProfilingSession.Current == null)
            {
                return conn;
            }

            var dbProfiler = new DbProfiler(ProfilingSession.Current.Profiler);
            return new ProfiledDbConnection(conn, dbProfiler);
        }
    }
}
