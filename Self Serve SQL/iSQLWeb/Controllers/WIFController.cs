using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using iSql.Commons;

namespace iSQLWeb.Controllers {

    /// <summary>
    /// This controller is introduced mainly for providing an easy way to test and manipulate ADFS/WIF related information.
    /// NOTE: for now, it is not sub class of basecontroller, becdause auth layer was done in that class and we want an 
    /// independent way of testing.
    /// </summary>
    public class WIFController : Controller {
        //
        // GET: /WIF/

        public ActionResult Index() {

            if (Conf.ADFSEnabled) {
                // it should be Microsoft.IdentityModel.Claims.ClaimsIdentity but we can just use the generic interface here.
                //var id = this.User.Identity as System.Security.Claims.ClaimsIdentity;
                var id = this.User.Identity as Microsoft.IdentityModel.Claims.ClaimsIdentity;

                if (id == null) {
                    throw new NullReferenceException("Claim should not be null.");
                }
                ViewBag.Claims = id.Claims;

                return View();
            } else {
                //TODO: may need to redirect go some generic page.
                return Content("ADFS is not enabled.");
            }
        }

    }
}
