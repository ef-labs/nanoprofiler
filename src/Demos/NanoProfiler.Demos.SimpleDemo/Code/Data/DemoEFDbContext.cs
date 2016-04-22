using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace NanoProfiler.Demos.SimpleDemo.Code.Data
{
    public class DemoEFDbContext : DbContext
    {
        public DbSet<DemoEFDbContext.Table> DemoDatas { get; set; }

        public DemoEFDbContext() : base("DemoDBConnectionString")
        {
        }

        [Table("Table")]
        public class Table
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
        }
    }
}