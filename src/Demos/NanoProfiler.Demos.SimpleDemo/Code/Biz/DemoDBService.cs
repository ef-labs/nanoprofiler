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
using System.Linq;

using EF.Diagnostics.Profiling;

using NanoProfiler.Demos.SimpleDemo.Code.Data;
using NanoProfiler.Demos.SimpleDemo.Code.Models;
using System.Threading.Tasks;

namespace NanoProfiler.Demos.SimpleDemo.Code.Biz
{
    public interface IDemoDBService
    {
        List<DemoData> LoadActiveDemoData();
        Task<List<DemoData>> LoadActiveDemoDataAsync();
        int LoadActiveDemoDataCount();
        Task<int> LoadActiveDemoDataCountAsync();
        List<DemoData> LoadActiveDemoData2();
        List<DemoData> LoadActiveDemoData3();
        Task TestSaveDemoDataAsync();
    }

    public class DemoDBService : IDemoDBService
    {
        private IDemoDBDataService _dataService;

        public DemoDBService(IDemoDBDataService dataService)
        {
            _dataService = dataService;
        }

        public List<DemoData> LoadActiveDemoData()
        {
            using (ProfilingSession.Current.Step("Biz.LoadActiveDemoData"))
            {
                //demos load data with data adapter
                _dataService.LoadActiveDemoDataWithDataAdapter();

                return _dataService.LoadActiveDemoData();
            }
        }

        public async Task<List<DemoData>> LoadActiveDemoDataAsync()
        {
            using (ProfilingSession.Current.Step("Biz.LoadActiveDemoDataAsync"))
            {
                //demos load data with data adapter
                _dataService.LoadActiveDemoDataWithDataAdapter();

                return await _dataService.LoadActiveDemoDataAsync();
            }
        }

        public int LoadActiveDemoDataCount()
        {
            using (ProfilingSession.Current.Step("Biz.LoadActiveDemoDataCount"))
            {
                return _dataService.LoadActiveDemoDataCount();
            }
        }

        public async Task<int> LoadActiveDemoDataCountAsync()
        {
            using (ProfilingSession.Current.Step("Biz.LoadActiveDemoDataCountAsync"))
            {
                return await _dataService.LoadActiveDemoDataCountAsync();
            }
        }

        public List<DemoData> LoadActiveDemoData2()
        {

            using (ProfilingSession.Current.Step("Biz.LoadActiveDemoData2"))
            {
                var query = from item in DemoDBDataContext.Get().GetTable<Table>()
                            where item.IsActive
                            select item;

                return query.Select(item => new DemoData { Id = item.Id, Name = item.Name }).ToList();
            }
        }

        public List<DemoData> LoadActiveDemoData3()
        {
            using (ProfilingSession.Current.Step("Biz.LoadActiveDemoData3"))
            {
                using (var dbContext = new DemoEFDbContext())
                {
                    var query = dbContext.DemoDatas.Where(item => item.IsActive);

                    return query.Select(item => new DemoData {Id = item.Id, Name = item.Name}).ToList();
                }
            }
        }

        public async Task TestSaveDemoDataAsync()
        {
            using (ProfilingSession.Current.Step("Biz.TestSaveDemoDataAsync"))
            {
                using (var dbContext = new DemoEFDbContext())
                {
                    var newItem = dbContext.DemoDatas.Create();
                    newItem.IsActive = true;
                    newItem.Name = "new";
                    dbContext.DemoDatas.Add(newItem);
                    await dbContext.SaveChangesAsync();

                    var items = dbContext.DemoDatas.Where(i => i.IsActive).OrderByDescending(e => e.Id).ToList();
                    dbContext.DemoDatas.Remove(items.First());
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
