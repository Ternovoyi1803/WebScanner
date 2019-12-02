using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebScanner.BLL;
using WebScanner.MVC.Models;

namespace WebScanner.MVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(StartScan model)
        {
            if (!ModelState.IsValid)
                return View("Index");

            UrlScannerSource source = new UrlScannerSource()
            {
                Url = model.StartUrl,
                Text = model.SearchText,
                MaxCountThreads = model.MaxCountThreads,
                MaxCountUrls = model.MaxCountUrls
            };

            UrlScanner.GetUrlScanner(true).Setup(source);

            return RedirectToActionPermanent("Scan", "Scan");
        }
    }
}