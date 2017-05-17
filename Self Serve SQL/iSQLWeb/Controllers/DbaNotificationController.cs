using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using iSql.Commons;
using iSql.Util;

namespace iSQLWeb.Controllers
{
    /// <summary>
    /// This is also a special controller that deliver public messages to DBA notfier program. No sensitive data, just simple notification, 
    /// so we can exclude this controller from complicated ADFS auth process. 
    /// 
    /// It can grab the latest message from the message center and only need to deliever the latest if updated.
    /// </summary>
    public class DbaNotificationController : Controller
    {

        public ActionResult Index()
        {

            DateTime lastAccessTime = DateTime.MinValue;
            DateTime.TryParse(Request.QueryString["since"], out lastAccessTime);
           
            
            // 10 seconds old message 
            if (( MessageQueueHub.BroadcaseMessageUpdateTime - lastAccessTime ).TotalSeconds > 60 && !String.IsNullOrWhiteSpace(MessageQueueHub.BroadcastMessageToDba) ) {
               return Content( MessageQueueHub.BroadcastMessageToDba ); 
            }
           
            // otherwise just return not modified status, which won't be interpreted by the messenger -- it only process 200 codes.
            return new HttpStatusCodeResult( 304, "Not modified."); 
        }
    }
}