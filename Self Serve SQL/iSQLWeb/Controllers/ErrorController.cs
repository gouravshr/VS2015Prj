using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSQLWeb.Controllers
{
    public class ErrorController : Controller {
        public ActionResult Unauthorized() {
            HttpException ex = RouteData.Values["exception"] as HttpException;
            if( ex == null ) {
                //NOTE: this should never happen, but we kept it here in case people just refresh the page 
                ViewBag.error = "";
            }else {
                ViewBag.error = ex.Message;
            }

            //NOTE: for some reason, when we dynamically invoke the acdtion, ViewEngine may get messed up, so here we manually pass the full path here.
            return View("~/Views/Error/Unauthorized.cshtml");
        }

        public ActionResult General() {
            // for general exception, it may not be httpexception at all, 
            // for example, in ADFS integration, it can be encrypt/decrypt related exceptions.
            Exception ex = RouteData.Values["exception"] as Exception ;
            
            return Content( ex == null ?  "General server side exception." : ex.Message + "\n" + ex.StackTrace );
        }
    }
}
