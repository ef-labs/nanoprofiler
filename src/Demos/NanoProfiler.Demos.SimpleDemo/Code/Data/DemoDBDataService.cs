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

namespace NanoProfiler.Demos.SimpleDemo.Code.Data
{
    public interface IDemoDBDataService
    {
        List<DemoData> LoadActiveDemoData();

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

        public DataSet LoadActiveDemoDataWithDataAdapter()
        {
            using (ProfilingSession.Current.Step("Data.LoadActiveDemoDataWithDataAdapter"))
            {
                using (var conn = new SqlConnection(@"Server=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\SimpleDemoDB.mdf;Database=SimpleDemoDB;Trusted_Connection=Yes;"))
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

        private IDbConnection GetConnection()
        {
            var conn = new SqlConnection(@"Server=(LocalDb)\v11.0;AttachDbFilename=|DataDirectory|\SimpleDemoDB.mdf;Database=SimpleDemoDB;Trusted_Connection=Yes;");

            if (ProfilingSession.Current == null)
            {
                return conn;
            }

            var dbProfiler = new DbProfiler(ProfilingSession.Current.Profiler);
            return new ProfiledDbConnection(conn, dbProfiler);
        }
    }
}
