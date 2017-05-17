using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using iSql.Commons;
using iSql.Commons.Models;

namespace iSQLWeb.Controllers
{
    [Authorize]
    public class ScriptUploadController : BaseController
    {
        //temporarily hardcode it 
        //public static string UPLOAD_DIR = Conf.WorkingFolder + "/ticket-001";

        public ActionResult Index( int? id ) {

            ViewBag.ProjectId = new SelectList(db.Projects, "ProjectId", "Name");

            if (Request.HttpMethod == "POST") {
                if (Request.Files.Count != 2) {
                    throw new Exception("Both target script and rollback script needs to be uploaded.");
                } else {
                    // the folder should now be created in ticket creation staging already 
                    var UPLOAD_DIR = Conf.WorkingFolder + "/ticket-" + id;
                    //TODO: refactory and distract code to separated method
                    HttpPostedFileBase scriptFile = Request.Files["targetScript"];
                    var originalScriptName = Path.GetFileName(scriptFile.FileName);
                    scriptFile.SaveAs(Path.Combine(UPLOAD_DIR, "target.sql"));

                    HttpPostedFileBase rollbackScriptFile = Request.Files["rollbackScript"];
                    var originalRollbackName = Path.GetFileName(rollbackScriptFile.FileName);
                    rollbackScriptFile.SaveAs(Path.Combine(UPLOAD_DIR, "rollback.sql"));

                    ViewBag.Message = "Files have been successfully uploaded";

                }
            } else {
                // for GET request 
                
            }
            return View();
        }


    }
}
