using iSql.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSQLWeb.Controllers
{
    public class UserAccessRequestController : BaseController
    {
        //
        // GET: /UserAccessRequest/
        List<SelectListItem> lst;
        public ActionResult Index()
        {
            return RedirectToAction("Create");
        }

        public ActionResult Create()
        {
            var user = Request.QueryString["req"];
            if (!String.IsNullOrWhiteSpace(user))
            {
                ViewBag.UserId = user;
            }
            
            lst = new List<SelectListItem>();
            foreach (var prj in db.Projects)
                lst.Add(new SelectListItem { Value = prj.ProjectId.ToString(), Text = prj.Name, Selected = false });

            ViewBag.ProjectList = lst;
            return View();
        }

        [HttpPost]
        public ActionResult Create(UserAccessRequest access, List<SelectListItem> projects)
        {
            if (ModelState.IsValid)
            {
                //db.Accesses.Add(access);
                //db.SaveChanges();

                //var mode = Request.Params["mode"];
                //if (mode == "user")
                //{
                //    return Redirect("/Access/User/" + access.UserId + "/");
                //}
                //else if (mode == "project ")
                //{
                //    return Redirect("/Access/Project/" + access.ProjectId + "/");
                //}
                return RedirectToAction("Index");
            }

            ViewBag.projects = (from p in db.Projects select p).ToList();
            return View(access);
        }
    }
}
