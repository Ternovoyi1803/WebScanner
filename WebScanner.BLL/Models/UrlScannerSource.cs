using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScanner.BLL
{
    public class UrlScannerSource
    {
        public string Url { get; set; }
        public string Text { get; set; }
        public int MaxCountUrls { get; set; }
        public int MaxCountThreads { get; set; }
    }
}
