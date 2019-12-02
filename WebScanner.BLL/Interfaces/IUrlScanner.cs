using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScanner.BLL
{
    public interface IUrlScanner
    {
        void Setup(UrlScannerSource source);

        void Start();
        void Stop();
        void Pause();
        void Resume();

        int UrlsCounter { get; }
        int MaxCountUrls { get; }
    }
}
