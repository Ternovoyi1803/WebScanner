using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebScanner.MVC.Models
{
    public class StartScan
    {
        public string StartUrl { get; set; }
        public int MaxCountThreads { get; set; }
        public string SearchText { get; set; }
        public int MaxCountUrls { get; set; }
    }
}