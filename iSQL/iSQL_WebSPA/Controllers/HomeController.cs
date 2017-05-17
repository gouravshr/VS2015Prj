using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSQL_WebSPA.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult User()
        {
            ViewBag.Message = "User page.";

            return View();
        }

        public ActionResult Report()
        {
            ViewBag.Message = "Report page.";

            return View();
        }

        public ActionResult Ticket()
        {
            ViewBag.Message = "Ticket page.";

            return View();
        }

        public ActionResult Project()
        {
            ViewBag.Message = "Project page.";

            return View();
        }
    }
}