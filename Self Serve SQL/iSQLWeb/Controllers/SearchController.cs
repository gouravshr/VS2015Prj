using iSql.Commons;
using System;
using System.Web.Mvc;

namespace iSQLWeb.Controllers
{
    public class SearchController : BaseController
    {
        public ActionResult Index()
        {
            string q = Request.Params["q"];
            //for now, just try ticket id search 

            if (!String.IsNullOrWhiteSpace(q))
            {
                int id = -1;
                if (int.TryParse(q, out id))
                {
                    var ticket = db.Tickets.Find(id);

                    if (ticket == null)
                        ViewBag.message = "Invalid ticket reference id [" + id + "].";
                    else
                    {
                        string accessRole = StateMachine.HasAccessToTicket(id, User.Identity.Name);

                        // per latest request, Admin & DBA can view all tickets, but they may not kick off the workflow except some speical steps such as validation
                        bool isPreviledgeUser = User.IsInRole("Admin") || User.IsInRole("DBA");
                        if (String.IsNullOrEmpty(accessRole) && !isPreviledgeUser)
                            ViewBag.message = "Ticket with reference id [" + id +
                                              "] found, but you do not have access to it.";
                        else
                            return Redirect("/Ticket/Overview/" + id);
                    }
                }
            }
            return View();
        }
    }
}