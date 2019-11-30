using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScanner.DAL
{
    public interface IRepository<T> where T : class
    {
        bool AddIfNotExists(T item);
        void Update(T item);
        List<T> GetAll(ScanStatus status);
        List<T> GetAll();
        void RemoveAll();
    }
}
