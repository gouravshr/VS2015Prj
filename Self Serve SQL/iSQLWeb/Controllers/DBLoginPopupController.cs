using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iSQLWeb.Controllers
{
    public class DBLoginPopupController : Controller
    {
        //
        // GET: /DBLoginPopup/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /DBLoginPopup/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /DBLoginPopup/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /DBLoginPopup/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /DBLoginPopup/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /DBLoginPopup/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /DBLoginPopup/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /DBLoginPopup/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
