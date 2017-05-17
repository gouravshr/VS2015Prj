using iSql.Commons;
using System;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace iSQLWeb.Controllers
{
    [Authorize(Roles="DBA,Admin")] 
    public class DbaController : BaseController
    {
        public ActionResult Index() {

            // there are lots of pending work items for DBA, so just retrieve past 15 days data
            var valideDate = DateTime.Now.Date.Subtract(new TimeSpan(30, 0, 0, 0));

            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("SinceDate", valideDate),  };

            // get working queue items
            ViewBag.workingQueue = DaoUtil.ExecStoredProc("GetPendingDbaRequests", parameters);

            // get approved/rejected items
            // cannot reuse parameter otherwise Ado.net will throw exception
            parameters = new SqlParameter[] { new SqlParameter("SinceDate", valideDate),  };
            ViewBag.decisionHistory = DaoUtil.ExecStoredProc("GetDbaDecisions", parameters);

            return View();
        }
    }
}