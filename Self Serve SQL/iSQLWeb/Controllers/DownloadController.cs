using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using iSql.Commons;

namespace iSQLWeb.Controllers {

    /// <summary>
    /// Downloading controller to dynamically deliver resources based on user roles, and access rights, with full control on file content and size delivered. 
    /// NOTE: 1. Physical resource folder's access needs to be restriced in IIS configurations.
    ///       2. And for now, we just simply deliver the raw content in bytes without worry about encoding.
    /// </summary>
    public class DownloadController : BaseController {

        #region sql scripts downloading
        protected ActionResult GetSqlFile(int ticketId, bool isRollback = false) {
            //NOTE: user must have access to download file 
            string accessRole = StateMachine.HasAccessToTicket(ticketId, User.Identity.Name);
            if (String.IsNullOrEmpty(accessRole)) {
                Response.StatusCode = 403;
                return Content("No access.");
            }

            string sqlFile = Conf.WorkingFolder + "ticket-" + ticketId + (isRollback ? "/rollback.sql" : "/target.sql");
            string downloadFileName = "ticket-" + ticketId + (isRollback ? "-rollback-" : "-target-");
            using (StreamReader reader = new StreamReader(sqlFile)) {
                Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + downloadFileName + DateTime.Now.Ticks + ".sql\"");
                return Content(reader.ReadToEnd(), "application/octet-stream");
            }
        }

        public ActionResult TargetSql(int id) {
            return GetSqlFile(id);
        }

        public ActionResult RollbackSql(int id) {
            return GetSqlFile(id, isRollback: true);
        }

        #endregion

        #region truncate log file to put size limit

        public ActionResult LogFile(int id) {
            string accessRole = StateMachine.HasAccessToTicket(id, User.Identity.Name);
            if (String.IsNullOrEmpty(accessRole)) {
                Response.StatusCode = 403;
                return Content("No access.");
            }

            string logFile = Request.Params["log"];
            string logFilePath = Conf.WorkingFolder + "ticket-" + id + "/" + logFile;

            if (string.IsNullOrWhiteSpace(logFile) 
                || !logFile.ToLower().EndsWith(".log")     // if user try to access other resource that is NOT ended with .log, it will fail
                || !System.IO.File.Exists(logFilePath)) {
                Response.StatusCode = 404;
                return Content("File not exist.");
            }
            
            // else, let's just grab 1M log file and send back
            // NOTE: encoding issues may be an issue, but user may get truncated file anyway.
            // EDR 2012-02-27:  Updated to dynamically determine size of the buffer to download.
            using (StreamReader reader = new StreamReader(logFilePath)) {
                Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + logFile );                
                char[] buffer = new char[Conf.MaximumLogFileSize];                
                //TODO:  Get actual count of charactes read and only pass back those,  currently passes back a file size equal to the buffer.
                int bytesRead = reader.Read(buffer, 0, Conf.MaximumLogFileSize);
                byte[] bytes = Encoding.UTF8.GetBytes(buffer, 0, bytesRead);
                return File(bytes,  "application/octet-stream");                                
            }
            
        }

        #endregion

    }
}
