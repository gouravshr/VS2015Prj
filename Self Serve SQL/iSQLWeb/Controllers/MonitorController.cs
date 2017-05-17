using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iSql.Commons;
using iSql.Commons.Models;
using iSql.Shell;

namespace iSQLWeb.Controllers
{
    public class MonitorController : BaseController
    {
        [Authorize(Roles = "Admin")]
        public ActionResult Locks()
        {
            ViewBag.locks = DaoUtil.GetAllLocks();
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Remove(int id)
        {
            ViewBag.locks = DaoUtil.GetSingleLock(id);
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Remove(int ticketId, string comments)
        {
            string reason = "Lock manually released by " + User.Identity.Name + ".  Comments:  " + comments;
            DaoUtil.ReleaseLock(ticketId, reason);
            string logMsg = "[" + User.Identity.Name + "] has aborted the execution";
            StateMachine.AddUserAbortState(ticketId,logMsg);
            var app = (HttpApplication)HttpContext.GetService(typeof(HttpApplication));
            ManageProcesses.RemoveStoredProcess(ManageProcesses.GetProcessByTicket(ticketId), app.Context);
            ManageProcesses.UpdateTicketForProcess(ticketId, null);
            if (!String.IsNullOrWhiteSpace(comments))
            {
                Comment comment = new Comment() { TicketId = ticketId, Text = comments, ByUser = User.Identity.Name, CreatedAt = DateTime.Now };
                db.Comments.Add(comment);
                db.SaveChanges();
            }
            return RedirectToAction("Locks");
        }
    }
}
