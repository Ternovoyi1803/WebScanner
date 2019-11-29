using System;
using System.Data.Entity;
using System.Linq;

namespace WebScanner.DAL
{
    public class UrlScanDBContext : DbContext
    {
        public UrlScanDBContext() : base("name=UrlScanDBContext")
        {
            //Database.SetInitializer(new DropCreateDatabaseAlways<DataModel>());
        }

        public virtual DbSet<UrlScan> UrlScanEntities { get; set; }
    }
}