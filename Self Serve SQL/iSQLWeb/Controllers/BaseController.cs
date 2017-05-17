using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using iSql.Commons;
using iSql.Commons.Models;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Collections;
using System.Diagnostics;
using iSql.Shell;

namespace iSQLWeb.Controllers
{
    public class BaseController : Controller
    {
        // shared logger, for now just share the root logger...
        protected static ILog Logger = log4net.LogManager.GetLogger(typeof(BaseController));

        // log4net dynamic properties in different threads may not work well
        //        protected static ILog TLogger = log4net.LogManager.GetLogger("ticket");

        #region auth related

        // Now is ADFS ready. To avoid ambiguity, use full qualified AuthorizationContext here, otherwise it will have conflict with identity namespace.
        protected override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            // NOTE: ADFS needs to be handled specially, meanwhile, we still want to keep the old Windows based authentication for now, 
            //       to make local testing easier. Now it is possible to switch back and force between different auth layers. 

            if (Conf.ADFSEnabled)
            {
                HandleAdfsAuth(filterContext);
            }
            else
            {
                // refactory old auth code to dedicated method
                HandleWindowsAuth(filterContext);
            }
            base.OnAuthorization(filterContext);
        }

        private void HandleAdfsAuth(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (Session["user"] != null)
            {
                //check db to see if identity is there or not  
                filterContext.HttpContext.User = (AccPrincipal)Session["user"];
            }
            else
            {
                // when request reached this far, it should already passed authenticaiton federation login step (assume http modules are properly configured and ordered)
                // and we need to extract the right user informaiton here. 

                // after decompile the Accenture.Web.Security.WifExtension.dll mentioned in ADFS team's white paper, it seems it is 
                // not much more than simple wrapper layer. In certain classes, they do provide some claim wrapping functions, but 
                // it does not handle the group information (non standard fields anyway), and in real world, that field itself may not 
                // be properly set (as the back and forth iternation between our team and ADFS team), so let's just stick to the basics 
                // of MS interfaces, and skip extra layers for now.

                // well, unfortunatley, ADFS team's configuration template refer to old WIF/ADFS while they recommend 4.5, it will NOT 
                // provide the right System.Security.Claims.ClaimsIdentity even under .NET 4.5, but provide old Microsoft.IdentityModel.Claims.ClaimsIdenity.

                // var id = this.User.Identity as ClaimsIdentity;
                var id = this.User.Identity as Microsoft.IdentityModel.Claims.ClaimsIdentity;

                // in theory, id should not be null once passed ADFS layer
                if (id == null)
                {
                    throw new NullReferenceException("Claim identity should not be null when ADFS integration is turned on.");
                }

                //NOTE: we can wrap it into existing AccPrincipal, or just use raw ClaimsIdentity as is. But the problem is that, existing code 
                //      had quite some bindings to old code, such as the Role based authentication attribute;  and if we switch to WIF code, then 
                //      another problem we have face is versioning, ADFS team's documenation and configuration section are out dated, and it will
                //      cause not only namespace conflict, but also certain types missing and different properties names, in case in the future 
                //      we upgraded to higher version WIF and .NET runtime, we may face similar thing again.

                var groups = new List<string>();
                var accessInfoDict = new Dictionary<int, ADGroupAccessInfo>();
                
                //NOTE: group claims need valid Enterprise ID infomation, which is passed via accenture customized claim. In most cases, it should
                //      show up in early part of the claim and being parsed first; but if for whatever reason, the one who process ADFS ITG makes 
                //      mistake in their console, it may show up in lower part of the claims, and we may be screwed.  We have no control on how 
                //      claims is parsed and ordered in the claim collection, it is MS' code. To avoid that,  we have to extract EID first, so even
                //      valid EID is not defined at the end of the claim SAML xml body by human "mistak", we still have it processed first, and group
                //      checking logic won't be impacted. 
                var eid = (from c in id.Claims where c.ClaimType == Conf.ClaimEnterpriseID select c.Value).FirstOrDefault<string>();
                if (string.IsNullOrWhiteSpace(eid))
                {
                    throw new ArgumentException("ADFS did not provide proper Enterprise ID in security claim.");
                }

                foreach (var claim in id.Claims)
                {
                    // in WIF 4.5, we can use claim.Type, but now ADFS team's sample doc does NOT support that, go with the obsoleted way now.
                    var type = claim.ClaimType;

                    // NOTE: 2013-10, since now we moved the ClaimGroup to Conf and made it configurable, it is no longer a constant and hence we cannot use switch statements here.
                    // have to modify it to pure if statement...
                    if (type == Conf.ClaimGroup)
                    {
                        //groups.Add(claim.Value);
                        var info = ADGroupAccessInfo.FromClaim(claim.Value);

                        // need to retrieve the project id based on the project key name, which is supposed to be uniq
                        //NOTE: per the latest discussion w/ Erin, they will re-use AIR id for group of project, so it is no longer unique.... 
                        //var projId = (from p in db.Projects where p.AdGroupKeyName == info.KeyName select p.ProjectId).FirstOrDefault<int>();
                        var projIds = (from p in db.Projects where p.AdGroupKeyName == info.KeyName select p.ProjectId).ToList<int>();

                        if (!projIds.Any())
                        {
                            // this should NOT occur, it means that AD groups were setup but corresponding project key name is not configured
                            // or it is updated but not matching each other, etc. For now we just ignore that.
                            Logger.Error("No project found with key name: " + info.KeyName);

                            // even it is emtpy projects, we may want to assign it to the info instead of let it be null
                            info.ProjectIds = projIds;
                            continue;
                        }
                        else
                        {
                            info.ProjectIds = projIds;
                        }

                        // defensive code here, it is common that people make mistake to have same users in both approver and requester group, 
                        // in such case, we only need to add Approver access.
                        // ADGroupAccessInfo existInfo = null; 

                        // NOTE: now need to create an entry for all the projects in the project id collections, since it is no longer uniq.
                        //       As a result, all projects share the same AIR id/project name in the project table will be updated at the same time, 
                        //       and there's no way for user be one project's approverl and be another project's requester at the same time.

                        // if such information exist and it is approver, then do nothing, otherwise, replace it
                        foreach (int projectId in projIds)
                        {
                            if (!accessInfoDict.ContainsKey(projectId)
                                || !accessInfoDict[projectId].IsApprover)
                            {
                                accessInfoDict[projectId] = info;
                            }
                        }//end projects loop 

                    }// end if, changed from; switch statement, no need to break, otherwise it affects outer loop 
                }

                if (string.IsNullOrWhiteSpace(eid))
                {
                    // double check, this should never happen, if so, then ADFS claims is not working properly and EID was missing.
                    throw new ArgumentException("ADFS claims do not provide Enterprise ID, or the claim type is changed recently, contact admin.");
                }

                // load user object from db
                var userObj = db.Users.Find(eid);

                // chances are, when we go for ADFS and open app to globl users, they may not in the system at all, we will create them on the fly now.  
                if (userObj == null)
                {
                    // create user with EID and make sure they are activated.
                    // 2013-10: per the latest discussion, just set system role to "Common User" instead of empty string, no longer
                    // need to distinguish between users accounts automaticated created by system after ADFS enabled and manually created one.
                    var newUser = new User() { UserId = eid, SystemRole = "Common User", IsActive = true };
                    db.Users.Add(newUser);
                    db.SaveChanges();
                    userObj = newUser;
                }

                // now let's handle user access, we will Update/Create/Delete user access based on claim on the fly, but only for the firt request.

                // I don't want start from user object here, just simple LINQ query here, and in this case, I don't have to worry about collection iteration and removing at same time, and possible round trips to db. 
                var accesses = (from a in db.Accesses where a.UserId == eid select a).ToList();

                // remove all project access that is not defined in ADFS claim, and direclty remove it from db context level
                foreach (var access in accesses)
                {
                    if (!accessInfoDict.ContainsKey(access.ProjectId))
                    {
                        db.Accesses.Remove(access);
                    }
                }

                // now loop through all Claims to add or update access information, for example, user now may be promoted from requester to approver
                foreach (var prjId in accessInfoDict.Keys)
                {
                    var claimAccess = accessInfoDict[prjId];
                    //NOTE: per the latest discussion, we have to check all project list 
                    //foreach (int prjId in claimAccess.ProjectIds) {

                    // check if current user's access list already contains such project
                    //var accessRecord = (from a in accesses where a.ProjectId == claimAccess.ProjectId select a).FirstOrDefault();
                    var accessRecord = (from a in accesses where a.ProjectId == prjId select a).FirstOrDefault();
                    if (accessRecord != null)
                    {
                        // check and see if we need update, this seems redudant, but we don't want to assign value if it is same as old one,
                        // to avoid set dirty flag and trigger unnecessary database round trip.
                        if (accessRecord.Role != claimAccess.Role)
                        {
                            accessRecord.Role = claimAccess.Role;
                        }
                    }
                    else
                    {
                        // let's create the new access record now
                        var newAccessRec = new Access() { ProjectId = prjId, Role = claimAccess.Role, UserId = eid };
                        db.Accesses.Add(newAccessRec);
                        db.SaveChanges();
                    }
                }

                // now just save db changes once and allow EM to fire as few db calls back as it can.
                db.SaveChanges();

                var idenity = new AccIdentity() { IsAuthenticated = true, Name = userObj.UserId, Role = userObj.SystemRole };
                var principal = new AccPrincipal(idenity);
                filterContext.HttpContext.User = principal;
                Session["user"] = principal;

            } // ADFS logic end
        }

        static string URL_DBA_NOTIFICATION = "/DbaNotification/Index";

        private void HandleWindowsAuth(System.Web.Mvc.AuthorizationContext filterContext)
        {
            // original Windows auth code, go with our customized AccPrinciapl. There may be some overlapped logic, but at this point, we don't have time to clean up.

            if (Session["user"] != null)
            {
                //check db to see if identity is there or not  
                filterContext.HttpContext.User = (AccPrincipal)Session["user"];
            }
            else if (this.HttpContext.User != null)
            {
                // NOTE: after adding support for speiclal controllers such as DBANotificationController, we need to enabled anonymous auth just for them. So we have to 
                //       build additional check for anobymous users but just for those special URL. And please keep in mind that those controller should NOT access any
                //       customzied property that is not avaible for anonymous users.
                if (!HttpContext.User.Identity.IsAuthenticated)
                {
                    if (Request.Url.AbsolutePath.Equals(URL_DBA_NOTIFICATION, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // we simply allow anonymous access for that speical URL
                        return;
                    }
                    else
                    {
                        Logger.Error("Unauthorized access [" + HttpContext.User.Identity.Name + "] tried to access this site.");
                        throw new HttpException(401, "Unauthorized access [" + HttpContext.User.Identity.Name + "], anonymouse is access is not allowed for this URL.");
                    }
                }

                var eid = ExtractEntId(this.HttpContext.User.Identity.Name);
                var userObj = db.Users.Find(eid);
                //NOTE: we have to check the active attribute for now
                if (userObj == null)
                {
                    //Log.Error("Invalid user try to access the applicaiton - " + eid);
                    //TODO: such exception won't be handled yet at this stage
                    Logger.Error("Windows authorizied user [" + HttpContext.User.Identity.Name + "] tried to access this site but has no applicaiton access rights.");
                    throw new HttpException(401, "Unauthorized access [" + HttpContext.User.Identity.Name + "]");
                }
                else if (!userObj.IsActive)
                {
                    Logger.Error("Windows authorizied user [" + HttpContext.User.Identity.Name + "] tried to access this site but has no long an active user in SSS db.");
                    throw new HttpException(401, "User [" + HttpContext.User.Identity.Name + "] is no longer an active user.");
                }
                var idenity = new AccIdentity() { IsAuthenticated = true, Name = userObj.UserId, Role = userObj.SystemRole };
                var principal = new AccPrincipal(idenity);
                filterContext.HttpContext.User = principal;
                Session["user"] = principal;
                Logger.Info("Login by [" + HttpContext.User.Identity.Name + "].");
            }
        }

        /// <summary>
        ///  Extrace enterprise ID for Windows Authenticaiton; as to ADFS, current accenture specific claims return Enterprise ID directly from a 
        ///  customzied claim type, so no need to extract and parse from standard "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" claim.
        /// </summary>
        /// <param name="fullUseNameWithDomain"></param>
        /// <returns></returns>
        public static string ExtractEntId(string fullUseNameWithDomain)
        {
            int i = fullUseNameWithDomain.LastIndexOf("\\");
            if (i >= 0 && i < fullUseNameWithDomain.Length - 1)
            {
                return fullUseNameWithDomain.Substring(i + 1);
            }
            return null;
        }

        #endregion

        #region rendering helper methods

        protected ContentResult RenderCsv(DataTable dt, string fileName)
        {
            Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + fileName + DateTime.Now.Ticks + ".csv\"");
            return Content(dt.ToCsv(), "application/vnd.ms-excel");
        }

        protected ContentResult RenderCsv(DataSet ds, string fileName)
        {
            Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + fileName + DateTime.Now.Ticks + ".csv\"");
            return Content(ds.ToCsv(), "application/vnd.ms-excel");
        }

        #endregion

        //public User User {
        //    get { return Session["user"] as User; }
        //    set { Session["user"] = value; }
        //}

        #region Data Context Wrapper
        //NOTE: in dev stage, just stick with EF for now, for performance consideration, we may switch to Linq2Sql or raw SP later on.
        protected EntDbContext db = new EntDbContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region logging support methods

        /// <summary>
        /// Only get the logger for specific ticket, and the log file will be created in the working folder, along side with target and rollback scripts.
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        public static ILog GetLoggerForTicket(int ticketId)
        {
            TicketLogger l = new TicketLogger("t-" + ticketId);
            //l.EffectiveLevel = Level.All;
            FileAppender appender = new FileAppender();
            appender.File = Conf.WorkingFolder + "ticket-" + ticketId + "/x.log";
            appender.LockingModel = new FileAppender.MinimalLock();

            appender.AppendToFile = true;

            l.Level = Level.All;

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            //patternLayout.ActivateOptions();
            appender.Layout = patternLayout;
            appender.ActivateOptions();

            l.Hierarchy = (Hierarchy)LogManager.GetRepository();

            l.AddAppender(appender);
            return new LogImpl(l);
        }

        public class TicketLogger : Logger
        {
            public TicketLogger(string name) : base(name) { }
        }

        #endregion

        #region Email related -- will move to new namespace soon

        public static List<string> _dbaEmails = new List<string>();
        public static List<string> DbaEmails
        {
            get
            {
                if (_dbaEmails.Count == 0)
                {
                    LoadAllDbaEmails();
                }
                return _dbaEmails;
            }
        }
        public static void LoadAllDbaEmails()
        {
            using (var db = new EntDbContext())
            {
                _dbaEmails = (from u in db.Users where u.SystemRole == "DBA" select u.UserId).ToList<string>();
            }
        }

        public static void AddDbaEmails(MailMessage msg)
        {
            foreach (var email in DbaEmails)
            {
                msg.To.Add(new MailAddress(email + "@accenture.com"));
            }
        }

        public static string GetUrlForTicket(int id)
        {
            // put special query string to let us know it is from email if we enalbe query string in IIS log
            return new StringBuilder().Append("<a href='").Append(Conf.RootUrl).Append("Ticket/Overview/").Append(id).Append("?ref=email' >").Append(id).Append("</a>").ToString();
        }

        #endregion
    }

    public class AccPrincipal : IPrincipal
    {
        public AccPrincipal(IIdentity identity)
        {
            _identity = identity;
        }

        private IIdentity _identity;

        #region Implementation of IPrincipal

        public bool IsInRole(string role)
        {
            return role.Equals((_identity as AccIdentity).Role);
        }

        public IIdentity Identity
        {
            get { return _identity; }

            //not part of the interface
            //TOOD: may consider make it protected internal later on, but now just make it public in case of unit testing 
            set { _identity = value; }
        }

        #endregion
    }

    public class AccIdentity : IIdentity
    {
        private string _name;
        private bool _authenticated;

        #region Implementation of IIdentity

        public string Name
        {
            get { return _name; }

            // not part of the interface 
            set { _name = value; }
        }

        public string AuthenticationType
        {
            get { return "CustomizedAuthentication"; }
        }

        public bool IsAuthenticated
        {
            get { return _authenticated; }

            // not part of the interface
            set { _authenticated = value; }
        }

        #endregion

        // for now, just assume single roles will be assigned, we may modify it to "roles" if complicated roles management needed
        public string Role { get; set; }
    }

    public class ADGroupAccessInfo
    {

        // I originally go with Tuple, and then dynamic, but then thought it may just be easier to introduce a dedicated calss.
        // Too bad, that .NET does not support named tuple but you have to go with anonymous type... when you go with collections, it looks not nice.

        // And I don't want to tie everything back to Entity Framework models. 
        // NOTE: per latest discussion with Erin, AIR id will be used for a group of projects, so the project claim is no long unique, and we have to make a collection here.
        public List<int> ProjectIds { get; set; }
        public string ADGroupName { get; set; }
        public string Role { get; set; }
        // the project key name, which should be uniq
        public string KeyName { get; set; }

        public bool IsApprover
        {
            get
            {
                return String.Equals("approver", Role, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public override string ToString()
        {
            //TODO: may consier to make it a full JSON or YAML format and handle all excaping and corner cases, but now it is enough for debug purpose.
            return new StringBuilder().Append("{ Project Ids: [").Append(string.Join(",", ProjectIds)).Append("]")
                .Append(", ADGroupName:").Append(ADGroupName)
                .Append(", Role:").Append(Role)
                .Append(", KeyName:").Append(KeyName)
                .Append(" } ").ToString();
        }

        // NOTE: why not just use regular expression with grouping restriction like this one below? 
        //            ^access\\.selfservicesql\\.(?<project>\\w+)\.(?<role>(approver|requester))
        //       The reason is simple, people make mistakes. AD groups are created by ED team manually, 
        //       in case there's a typo, then the whole RegEx will be invalid. We may want to capture this first.
        //       It will be normalized anyway, so don't bother right now.
        const string pattern = "^access\\.selfservicesql\\.(?<project>\\w+)\\.(?<role>\\w+)";
        // make it case insensitive, humanbeings make mistakes sometimes when create AD group
        public static Regex GroupClaimRegEx = new Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public static ADGroupAccessInfo FromClaim(string claim)
        {
            // in case ADFS is not properly setup, or at least when you try with local STS and not have proper settings.
            if (String.IsNullOrWhiteSpace(claim))
            {
                throw new ArgumentException("ADFS passed invalid claim :" + claim);
            }
            var match = GroupClaimRegEx.Match(claim);
            if (match.Success)
            {
                var info = new ADGroupAccessInfo();
                info.KeyName = match.Groups["project"].Value;

                // NOTE: per the test with Erin, we found duplicated "approver" and "Approver" roles co-exist in database which is not good.
                //       The reasaon is that, exsiting code base already hardcoded roles "Approver" and "Requester" in the past, and it is 
                //       widely used in cshtml views, if we have to change that, it is too much work. Instead, we can normalize it here. 

                // Only "Approver" and "Requester" are valid group names for now
                string role = match.Groups["role"].Value;
                if (role.Equals("Approver", StringComparison.CurrentCultureIgnoreCase))
                {
                    info.Role = "Approver";
                }
                else if (role.Equals("Requester", StringComparison.CurrentCultureIgnoreCase))
                {
                    info.Role = "Requester";
                }
                else
                {
                    // for now just throw exception
                    throw new ArgumentException("ADFS passed claim: " + claim + "; but the role name is not either Approver or Requester, and hence not fully supported now.");
                }
                info.Role = role;
                return info;
            }
            return null;
        }
    } //end of class
}