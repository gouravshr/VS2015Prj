using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSQLWeb.Controllers {
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController {
        public ActionResult Index() {
            return View();
        }
    }
}
