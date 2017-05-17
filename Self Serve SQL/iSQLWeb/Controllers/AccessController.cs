using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using iSql.Commons.Models;

namespace iSQLWeb.Controllers {
    [Authorize(Roles = "Admin")]
    public class AccessController : BaseController {
        public ViewResult Index() {
            return View(db.Accesses.ToList());
        }

        public ViewResult Details(int id) {
            Access access = db.Accesses.Find(id);
            return View(access);
        }

        public ActionResult User(string id) {
            // NOTE: admin access rights by given user name     
            var accesses = (from a in db.Accesses where a.UserId == id select a).ToList();
            ViewBag.user = id;
            return View( accesses );
        }
   
        public ActionResult Project(int id) {
            // NOTE: admin access rights by given user name     
            var accesses = (from a in db.Accesses where a.ProjectId == id select a).ToList();
            ViewBag.project = id; 
            return View( accesses );
        }

        public ActionResult Create( string id = null) {

            var user = Request.QueryString["u"]; 
            if( ! String.IsNullOrWhiteSpace(user)) {
                ViewBag.userId = user; 
            }            
            
            //TODO: caching, no need to retreive the prj and user list every time.
            ViewBag.projects = (from p in db.Projects select p).ToList();
            ViewBag.users = (from u in db.Users select u).ToList();
            ViewBag.roles = new List<string> { "Approver", "Requester" };


            return View();
        }

        [HttpPost]
        public ActionResult Create(Access access) {

            // here's the old trick since early EF and MVC framework...
            // since EF 4.x, there's no need to maintain two different relationship from both models
            /*
            var projId = 0;
            if (int.TryParse(Request.Params["CategoryID"], out projId) && projId > 0) {
                var project = db.Projects.Find(projId);
                if (null == project) { 
                    //TODO: Error handling
                }
                access.Project = project;
            } else {
                //TODO: error handling.
            }
            */

            // but you still need validate the model, now associated entity is set already  
            //if ( TryValidateModel(access) &&  ModelState.IsValid)

            // since MVC3, the default binder has beeing improved, we don't have to play those tricks to support 
            // nested complex types.
            if (ModelState.IsValid) {
                db.Accesses.Add(access);
                db.SaveChanges();

                var mode = Request.Params["mode"]; 
                if(mode =="user") {
                    return Redirect("/Access/User/" + access.UserId + "/");
                } else if (mode == "project ") {
                    return Redirect("/Access/Project/" + access.ProjectId + "/");
                }
                return RedirectToAction("Index");
            }

            ViewBag.projects = (from p in db.Projects select p).ToList();
            return View(access);
        }

        public ActionResult Edit(int id) {
            Access access = db.Accesses.Find(id);

            var user = Request.QueryString["u"]; 
            if( ! String.IsNullOrWhiteSpace(user)) {
                ViewBag.user = user; 
            }            

            var project = Request.QueryString["p"]; 
            if( ! String.IsNullOrWhiteSpace(user)) {
                ViewBag.project = project; 
            }            

            ViewBag.roles = new List<string> { "Approver", "Requester" };

            return View(access);
        }

        [HttpPost]
        public ActionResult Edit(Access access) {
            if (ModelState.IsValid) {
                db.Entry(access).State = EntityState.Modified;
                db.SaveChanges();

                var mode = Request.Params["mode"]; 
                if(mode =="user") {
                    //special treat here
                    return Redirect("/Access/User/" + access.UserId + "/");
                } else if (mode == "project ") {
                    return Redirect("/Access/Project/" + access.ProjectId);
                }
                return RedirectToAction("Index");
            }

            //TODO: invalid user input and proper error handling, and persist user/project mode
            //ViewBag.roles = new List<string> { "Approver", "Requester" };
            return View(access);
        }

        public ActionResult Delete(int id) {
            Access access = db.Accesses.Find(id);

            return View(access);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id) {
            Access access = db.Accesses.Find(id);
            db.Accesses.Remove(access);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}