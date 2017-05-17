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
    public class UserController : BaseController {


        public ViewResult Index() {
            //TODO: now just display ALL users, in the future we need:
            //      1. Add at least paging options
            //      2. Add search features so we can quickly find a user by EID

            //NOTE: please note enteprrise id contains "." and caused problem after ADFS integration.
            //      It is solved in the view level.
            return View(db.Users.ToList());
        }


/* 
        public ViewResult Details(string id) {
            User user = db.Users.Find(id);
            return View(user);
        }
*/

        public ActionResult Create() {
            return View();
        }


        [HttpPost]
        public ActionResult Create(User user) {
            if (ModelState.IsValid) {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }


        public ActionResult Edit(string id) {
            //NOTE: after proper tweaking, id with special dot can be automatically parsed by framework again.
            //      No need to do encrypt/decrypt, delete all those code here already in this branch.
            User user = db.Users.Find(id);
            return View(user);
        }


        [HttpPost]
        public ActionResult Edit(User user) {
            if (ModelState.IsValid) {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        /* 
                NOTE: no longer need to delete users -- we may keep it here to identify which ticket being created by who, we can just set the active flag to diallow access
                public ActionResult Delete(string id)
                {
                    User user = db.Users.Find(id);
                    return View(user);
                }

                [HttpPost, ActionName("Delete")]
                public ActionResult DeleteConfirmed(string id)
                {            
                    User user = db.Users.Find(id);
                    db.Users.Remove(user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
        */
    }
}