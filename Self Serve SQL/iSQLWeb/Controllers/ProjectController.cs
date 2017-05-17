using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iSql.Commons.Models;

namespace iSQLWeb.Controllers {
    [Authorize(Roles = "Admin")]
    public class ProjectController : BaseController {
        
        const string OracleDB = "Oracle";
        const string SqlServer = "SQL Server";

        private EntDbContext db = new EntDbContext();

        public ViewResult Index() {
            return View(db.Projects.OrderBy(project => project.Name).ToList());
        }

        /*
          public ViewResult Details(int id)
          {
              Project project = db.Projects.Find(id);
              return View(project);
          }
        */

        public ActionResult Create() {
            ViewBag.dbType = new List<string> { OracleDB, SqlServer };
            return View();
        }

        [HttpPost]
        public ActionResult Create(Project project) {
            ViewBag.dbType = new List<string> { OracleDB, SqlServer };
            if (ModelState.IsValid) {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        public ActionResult Edit(int id) {
            Project project = db.Projects.Find(id);
            ViewBag.dbType = new List<string> { OracleDB, SqlServer };
            return View(project);
        }

        [HttpPost]
        public ActionResult Edit(Project project) {
            if (ModelState.IsValid) {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        /* 
        public ActionResult Delete(int id)
        {
            Project project = db.Projects.Find(id);
            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        */

        protected override void Dispose(bool disposing) {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}