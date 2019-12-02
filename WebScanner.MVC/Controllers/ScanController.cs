using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebScanner.BLL;
using WebScanner.DAL;
using WebScanner.MVC.Models;

namespace WebScanner.MVC.Controllers
{
    public class ScanController : Controller
    {
        private IUrlScanner scanner;
        private IRepository<UrlScan> repository;

        public ScanController(IRepository<UrlScan> repository)
        {
            this.repository = repository;
            scanner = UrlScanner.GetUrlScanner();
        }

        public ActionResult Scan()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Scan(string status)
        {
            return View("Scan", (object)status);
        }   

        [HttpPost]
        public ActionResult Update()
        {        
            return View("Scan");
        }

        [HttpGet]
        public ActionResult Start()
        {
            scanner.Start();

            return View("Scan");
        }

        [HttpGet]
        public ActionResult Stop()
        {
            scanner.Stop();

            return View("Scan");
        }

        [HttpGet]
        public ActionResult Pause()
        {
            scanner.Pause();

            return View("Scan");
        }

        [HttpGet]
        public ActionResult Resume()
        {
            scanner.Resume();

            return View("Scan");
        }

        [HttpGet]
        public int GetHandlingUrlsCounter()
        {
            return (100 * scanner.UrlsCounter) / scanner.MaxCountUrls;
        }

        public ActionResult ScanData(string status)
        {
            if(status == null || status == "All")
                return PartialView(repository.GetAll().OrderByDescending(x => x.DateStart));
                 
            ScanStatus scanStatus = ScanStatus.None;
            switch (status)
            {
                case "Loading":
                    {
                        scanStatus = ScanStatus.Loading;
                        break;
                    }
                case "Found":
                    {
                        scanStatus = ScanStatus.Found;
                        break;
                    }
                case "NotFound":
                    {
                        scanStatus = ScanStatus.NotFound;
                        break;
                    }
                case "Failure":
                    {
                        scanStatus = ScanStatus.Failure;
                        break;
                    }
            }

            return PartialView(repository.GetAll(scanStatus).OrderByDescending(x => x.DateStart));
        }     
    }
}