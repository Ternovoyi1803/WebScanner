﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScanner.Models
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
