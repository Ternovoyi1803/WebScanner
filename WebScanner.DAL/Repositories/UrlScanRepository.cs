using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScanner.DAL
{
    //TODO Lock review and optimize
    public class UrlScanRepository : IDisposable, IRepository<UrlScan>
    {
        private UrlScanDBContext db;
        private static object locker = new object();
        private bool disposedValue = false;

        public UrlScanRepository()
        {
            db = new UrlScanDBContext();
        }

        public bool AddIfNotExists(UrlScan url)
        {
            lock (locker)
            {
                if (Contains(url.Url))
                    return false;

                db.UrlScanEntities.Add(url);
                db.SaveChanges();

                return true;
            }
        }

        public void Update(UrlScan url)
        {
            lock (locker)
            {
                var targetUrl = db
                    .UrlScanEntities
                    .Where(x => x.Url == url.Url)
                    .SingleOrDefault() ?? throw new NullReferenceException();

                targetUrl = url;
                db.SaveChanges();
            }
        }

        public List<UrlScan> Read(ScanStatus status)
        {
            lock (locker)
            {
                return db
                    .UrlScanEntities
                    .Where(x => x.ScanStatus == status)
                    .ToList();
            }
        }

        public void RemoveAll()
        {
            lock (locker)
            {
                db.UrlScanEntities.RemoveRange(db.UrlScanEntities);
                db.SaveChanges();
            }
        }

        private bool Contains(string url)
        {
            return db
                .UrlScanEntities
                .Where(x => x.Url == url)
                .SingleOrDefault() != null ? true : false;
        }

        #region Dispose pattern      

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                db.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                db.Dispose();
                GC.SuppressFinalize(this);
                disposedValue = true;
            }
        }

        ~UrlScanRepository()
        {
            Dispose();
        }

        #endregion
    }
}
