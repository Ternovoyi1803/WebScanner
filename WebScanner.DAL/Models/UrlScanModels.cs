using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScanner.DAL
{
    public class UrlScan
    {
        public UrlScan() { }
        public UrlScan(string url)
        {
            Url = url;
        }
        public UrlScan(string url, string urlParent)
        {
            Url = url;
            UrlParent = urlParent;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(2048)]
        public string Url { get; set; }
        public ScanStatus ScanStatus { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string FailureReason { get; set; }
        [MaxLength(2048)]
        public string UrlParent { get; set; }
    }

    public enum ScanStatus
    {
        None,
        Loading,
        Found,
        NotFound,
        Failure
    }
}
