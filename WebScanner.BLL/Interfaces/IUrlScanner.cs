using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScanner.BLL
{
    public interface IUrlScanner
    {
        Task Start();
        Task Stop();
        Task Pause();
        Task Resume();

        int UrlsCounter { get; }
        int MaxCountUrls { get; }
    }
}
