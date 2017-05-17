using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iSql.Commons;
using log4net;

namespace iSQLWeb.Controllers
{
    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Landing");
            }

            /*
            //ViewBag.user = HttpContext.User; 
            var myTickets = from t in db.Tickets where t.UserId == HttpContext.User.Identity.Name orderby t.CreatedAt descending select t ;

            ViewBag.tickets = myTickets;

            // retrieve tickets tha wait for current user's approval and he is NOT the requester. 

            //NOTE: we can chain LINQ expressions but unfortunately, it generates very NASTY and slsow query, so now I just query it first, to get teh "IN" statement generated instead of nested join.
            //TODO: 1. cache avaialble projects. 2. switch to Stored Procedure 
            var myProjIds = (from a in db.Accesses where a.UserId == HttpContext.User.Identity.Name select a.ProjectId).ToList();

            // for now, just get the last 30 days 
            var valideDate = DateTime.Now.Date.Subtract(new TimeSpan(30, 0, 0, 0));

            var followupTickets = from t in db.Tickets
                                  where t.UserId != HttpContext.User.Identity.Name && myProjIds.Contains(t.ProjectId)
                                        && t.CreatedAt >= valideDate
                                  orderby t.CreatedAt descending
                                  select t;
            */
            //NOTE: for performance consideration, we need to bypass the object model to just retrevie back what we want... we can go with either very complicated and nested 
            //Ent for Linq query, or db context's sql query or object context's query mehtod -- with has limitiation in EF4.1 and hard to retrieve record set back; or go 
            //deeper to Database level and work directly on the sql conneciton, but have to understand internal way of closing those connection if you don't want to dispose 
            //explicilty.......... or why not just simple old school ADO.NET stuff if we just want sotred proc solution? 

            // for now, just get the last 30 days 
            var valideDate = DateTime.Now.Date.Subtract(new TimeSpan(30, 0, 0, 0));

            SqlParameter[] parameters = new SqlParameter[] {
                                           new SqlParameter("UserId", HttpContext.User.Identity.Name),         
                                           new SqlParameter("SinceDate", valideDate),         
                                        };

            ViewBag.tickets = DaoUtil.ExecStoredProc("GetMyTickets", parameters);

            SqlParameter[] parameters2 = new SqlParameter[] {
                                           new SqlParameter("UserId", HttpContext.User.Identity.Name),         
                                           new SqlParameter("SinceDate", valideDate),         
                                        };
            ViewBag.followup = DaoUtil.ExecStoredProc("GetFollowUpTickets", parameters2);
            //ViewBag.followup = followupTickets;

            //ViewBag.WorkState = new SelectList(db.Workstates.Select(ws=>ws.Name).Distinct());

            //TODO: more robust message displaying system other than just simple appSettings configuration.
            string msg = ConfigurationManager.AppSettings["HomePageMessage"];
            if (!string.IsNullOrWhiteSpace(msg))
            {
                ViewBag.hasMessage = true;
                ViewBag.message = msg;
            }
            else
            {
                ViewBag.hasMessage = false;
            }
            return View();
        }

        public ActionResult Landing() {
            //NOTE: for produciton we don't allow landing page at all, you only need it for dev and testing environment 
            if (Conf.AllowLandingPage) {

                // hackish way ... it is used for dev and testing purpose.
                var users = from u in db.Users select u;
                ViewBag.userList = new SelectList(users, "UserId", "UserId");
                return View(users);
            } else {
                Response.StatusCode = 404;
                return Content("Not found");
            }
        }

        [HttpPost]
        public ActionResult Landing(string user = null) {
            if (!String.IsNullOrEmpty(user)) {
                var userObj = db.Users.Find(user); 
                //User = userObj;
                var idenity = new AccIdentity() {IsAuthenticated = true, Name = userObj.UserId, Role = userObj.SystemRole};
                var principal = new AccPrincipal(idenity);
                Session["user"] = principal;
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult Logout() {
            Session["user"] = null;
            return RedirectToAction("Index");
        }
    }
}