using System;
using System.Collections.Generic;
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
        private static IUrlScanner scanner;
        private IRepository<UrlScan> repository;

        public ScanController(IRepository<UrlScan> repository)
        {
            this.repository = repository;
        }      

        [HttpPost]
        public ActionResult Scan(StartScan model)
        {
            if(scanner != null)
                scanner.Stop();

            scanner = new UrlScanner(
                url: model.StartUrl,
                word: model.SearchText,
                maxCountUrls: model.MaxCountUrls,
                maxCountThreads: model.MaxCountThreads);

            scanner.Start();

            return View(repository.GetAll().OrderByDescending(x => x.DateStart));
        }

        [HttpGet]
        public ActionResult Update()
        {        
            return View("Scan", repository.GetAll().OrderByDescending(x => x.DateStart));
        }

        [HttpGet]
        public ActionResult Stop()
        {
            scanner.Stop();

            return View("Scan", repository.GetAll().OrderByDescending(x => x.DateStart));
        }

        [HttpGet]
        public ActionResult Pause()
        {
            scanner.Pause();

            return View("Scan", repository.GetAll().OrderByDescending(x => x.DateStart));
        }

        [HttpGet]
        public ActionResult Resume()
        {
            scanner.Resume();

            return View("Scan", repository.GetAll().OrderByDescending(x => x.DateStart));
        }
    }
}