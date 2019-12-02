using System;
using System.Data.Entity;
using System.Linq;

namespace WebScanner.DAL
{
    public class UrlScanDBContext : DbContext
    {
        public UrlScanDBContext() : base("name=UrlScanDBContext")
        {

        }

        public virtual DbSet<UrlScan> UrlScanEntities { get; set; }
    }
}