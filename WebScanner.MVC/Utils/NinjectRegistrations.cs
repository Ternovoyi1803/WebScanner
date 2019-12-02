using Ninject.Modules;
using WebScanner.BLL;
using WebScanner.DAL;

namespace WebScanner.MVC
{
    // Dependency injection 
    public class NinjectRegistrations: NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository<UrlScan>>().To<UrlScanRepository>();
        }
    }
}