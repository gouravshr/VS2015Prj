using iSql.Commons;
using iSql.Commons.Models;
using iSql.Shell;
using iSql.Util;
using iSql.Validator;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace iSQLWeb.Controllers {
    [Authorize]
    public class TicketController : BaseController {

        public ActionResult NoAccess() {
            return View();
        }

        public ActionResult Overview(int id) {
            object a = HttpContext.Application["d"];
            Ticket t = db.Tickets.Find(id);
            if (null == t) {
                //TODO: error handling
            }
            
            // NOTE: check if user has access
            string accessRole = StateMachine.HasAccessToTicket(id, User.Identity.Name);
            if (String.IsNullOrEmpty(accessRole)) {
                return RedirectToAction("NoAccess");
            }
            
            //Executor.GetScriptExecutionStatus(HttpContext, User.Identity.Name, id);
            ViewBag.accessRole = accessRole;

            // NOTE: only approver can take user actions, and owner of request can NOT approve his/her own requests.
            bool isOwner = (t.UserId == HttpContext.User.Identity.Name);
            ViewBag.isOwner = isOwner;
            ViewBag.isApprover = (accessRole == "Approver");
            ViewBag.isValidApprover = ViewBag.isApprover && !isOwner;

            //NOTE: 
            //  Per latest requests, now we need to support manual DBA validation; as a result, we need to pass additonal parameter to let render engine 
            //  know if user has DBA role or not. Only DBAs have rights to see the region of approval/reject validation flow, other users can only see 
            //  general messaging showing it is waiting for actions. 
            //  And as of now, DBA is a new SystemRole that is not project related. 

            bool isDBA = User.IsInRole("DBA");
            ViewBag.isDBA = isDBA;

            ViewBag.project = t.Project;
            ViewBag.states = from w in db.Workstates where w.TicketId == t.TicketId select w;

            // get comments... 
            ViewBag.comments = from c in db.Comments where c.TicketId == t.TicketId select c;

            return View(t);
        }

        public ActionResult AddComment( int id ) {
             Ticket t = db.Tickets.Find(id);
            if (null == t) {
                //TODO: error handling
            }

            // NOTE: check if user has access
            string accessRole = StateMachine.HasAccessToTicket(id, User.Identity.Name);
            if (String.IsNullOrEmpty(accessRole)) {
                return RedirectToAction("NoAccess");
            }

            string text =  Request.Params["text"];
            if( String.IsNullOrWhiteSpace( text )) {
                return Content("Comment text cannot be blank.");
            }

            Comment comment = new Comment() { TicketId = id, Text = text.Trim(), ByUser = User.Identity.Name, CreatedAt = DateTime.Now };
            db.Comments.Add(comment);
            db.SaveChanges();

            //return  Content("Comments added");
            return Content("{\"ok\":true, \"id\":" + comment.CommentId + "}");
        }

        // Ticket creation and script upload request
        public ActionResult Upload() {
            // only retrieve that project that user has access to
            var projects = ( from a in db.Accesses
                           where a.UserId == HttpContext.User.Identity.Name
                           select a.Project ).ToList() ;

            //NOTE: we have to check if this user really has access to ANY project, if NOT, then we have to redirect him to contact the admin to gain access. It is possible that user are created by accesses v not be assigned yet.
            if( projects.Count() < 1 ) {
                return View( "NoProjectAccess");
            }

            ViewBag.ProjectId = new SelectList(projects, "ProjectId", "Name");
            //ViewBag.ProjectId = new SelectList(db.Projects, "ProjectId", "Name");

            return View();
        }

        [HttpPost]
        public ActionResult Upload(Ticket ticket) {
            //target script must not be empty.
            bool validTargetScript = Request.Files["targetScript"].ContentLength > 0;

            //NOTE: for now, we allow upload empty rollback script, but user must upload the script even it is blank 
            bool validRollbackScript = !String.IsNullOrWhiteSpace(Request.Files["rollbackScript"].FileName);

            if (!(validTargetScript && validRollbackScript))
            {
                //NOTE: super bad that Microsoft still haven't fix the validaiton summary issue, you can ONLY use String.Empty to add customized 
                //      error message here without introducing your own customized validator class, which is a lot of efforts
                ModelState.AddModelError(string.Empty, "Both target and rollback scripts are required.");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    // populate other fields
                    ticket.UserId = HttpContext.User.Identity.Name;
                    ticket.CreatedAt = DateTime.Now;

                    db.Tickets.Add(ticket);
                    db.SaveChanges();

                    //NOTE: think about when you scale out and deploy this applicaiton to different nodes, how can you rely on the local folders to hold those information? 
                    //    1. You can use SAN, and everything becomes very simple then. 
                    //    2. Use sticky session or other customized routing way to make sure you always visit to the same server.
                    //    3. BLOB solution, hope we never use this one... bulky, slow and hard to access.

                    // create ticket working folder
                    string targetFolder = Conf.WorkingFolder + "ticket-" + ticket.TicketId;
                    if (Directory.Exists(targetFolder))
                    {
                        //TODO: This should NEVER occurs! Cry out and notify users.
                        //            throw new InvalidOperationException("Folder exist for newly created ticket [" + ticket.TicketId + "], which is incorrect.");
                    }
                    else
                    {
                        Directory.CreateDirectory(targetFolder);
                        //TODO: handle IO exception
                    }

                    //TODO: refactory and distract code to separated method
                    HttpPostedFileBase scriptFile = Request.Files["targetScript"];
                    var originalScriptName = Path.GetFileName(scriptFile.FileName);
                    scriptFile.SaveAs(Path.Combine(targetFolder, "target.sql"));

                    HttpPostedFileBase rollbackScriptFile = Request.Files["rollbackScript"];
                    var originalRollbackName = Path.GetFileName(rollbackScriptFile.FileName);
                    rollbackScriptFile.SaveAs(Path.Combine(targetFolder, "rollback.sql"));

                    // refactory the code to state machine
                    var logMessage = "Target script [original name: " + originalScriptName + "] and rollback script [original name: " + originalRollbackName + "]  are uploaded by [" + HttpContext.User.Identity.Name + "] and renamed.";
                    if (!StateMachine.AddFileUploadedState(ticket.TicketId, logMessage))
                    {
                        //TODO: redirect to error page and inform admin on the state saving issue 
                        throw new Exception("Failed to update the file uploaded state.");
                    }

                    ILog tlog = GetLoggerForTicket(ticket.TicketId);
                    tlog.Info(DateTime.Now + " [ticket: " + ticket.TicketId + "]" + logMessage);

                    logMessage = "Wait for project supervisor's approval.";
                    //ViewBag.Message = "Files have been successfully uploaded";
                    //Require supervisor approval, New workstate will be added here                   
                    if (!StateMachine.AddSupervisorWaitForDecision(ticket.TicketId, logMessage))
                    {
                        //TODO: redirect to error page and inform admin on the state saving issue 
                        throw new Exception("Failed to update wait for supervisor's decision state.");
                    }

                    //TODO: Auto script validation will be moved into another function, and called after getting approval of script
                    //SqlScriptValidation(ticket.TicketId);

                    //return RedirectToAction("Index");  
                    return Redirect("/Ticket/Overview/" + ticket.TicketId);
                }
            }

            var projects = from a in db.Accesses
                           where a.UserId == HttpContext.User.Identity.Name
                           select a.Project;

            ViewBag.ProjectId = new SelectList(projects, "ProjectId", "Name", ticket.ProjectId);
            //ViewBag.ProjectId = new SelectList(db.Projects, "ProjectId", "Name", ticket.ProjectId);
            return View(ticket);
        }

        public ActionResult TicketAbort(int id, string commentStr = null)
        {
            //NOTE: access control is done via attributes above, and it is actually the SystemRole in db, which is independent of each project's roles.

            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            ILog tlog = GetLoggerForTicket(id);
            if (state.Code != FlowState.TicketWaitingForApproval.Code)
            {
                // someone else must must already handled this, just give some warning message back
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to reject the request..");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }

            // Comment is optional, need to insert to db if it is not blank.
            if (!String.IsNullOrWhiteSpace(commentStr))
            {
                Comment comment = new Comment() { TicketId = id, Text = commentStr.Trim(), ByUser = User.Identity.Name, CreatedAt = DateTime.Now };
                db.Comments.Add(comment);
                db.SaveChanges();
            }

            string logMessage = "[" + User.Identity.Name + "] rejected the request, and workflow is automatically terminated now.";
            // We terminate the whole flow like it was before.
            if (!StateMachine.AddSupervisorRejectFailedState(id, logMessage, User.Identity.Name))
            {
                throw new Exception("Failed to reject the request by project supervisor.");
            }
            tlog.Info(DateTime.Now + " [ticket: " + id + "]" + logMessage);

            //NOTE: now we need automatically swich to staging push status.
            logMessage = "Terminate workflow after rejecting the request from project supervisor.";
            if (!StateMachine.AddTerminateOnErrorState(id, logMessage))
            {
                throw new Exception("Fail to terminate workflow after rejecting the request from project supervisor.");
            }

            //TODO: Move the code into EmailUtil by creating a new function
            // needs to send a short message for DBA messenger as well
            if (Conf.MQ.Enabled)
            {
                MessageQueueHub.SendDbaDecisionMessage("Ticket " + t.TicketId + " for [" + t.Project.Name + "] project is rejected by " + User.Identity.Name);
            }

            if (ComeFromDbaCenter())
            {
                return RefreshDbaPage();
            }

            return RefreshOverviewPage(id);
        }

        public ActionResult TicketApprove(int id, string commentStr = null)
        {
            //Check the current state
            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            ILog tlog = GetLoggerForTicket(id);
            if (state.Code != FlowState.TicketWaitingForApproval.Code)
            {
                // someone else must must already handled this, just give some warning message back
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to approve the request..");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }

            // Comment is optional, need to insert to db if it is not blank.
            if (!String.IsNullOrWhiteSpace(commentStr))
            {
                Comment comment = new Comment() { TicketId = id, Text = commentStr.Trim(), ByUser = User.Identity.Name, CreatedAt = DateTime.Now };
                db.Comments.Add(comment);
                db.SaveChanges();
            }

            //Save the state 
            string logMessage = "[" + User.Identity.Name + "] has been approved the request.";
            // We terminate the whole flow like it was before.
            if (!StateMachine.AddSupervisorApprovalSucceedState(id, logMessage, User.Identity.Name))
            {
                throw new Exception("Failed to approved the request by project supervisor.");
            }
            tlog.Info(DateTime.Now + " [ticket: " + id + "]" + logMessage);

            //Move it for script validation
            SqlScriptValidation(id);
            return RefreshOverviewPage(id);
        }

        public ActionResult ScriptExecutionStatus(int id)
        {
            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            if (state.Code != FlowState.ProductExecutionStarted.Code)
            {
                return RefreshOverviewPage(id);
            }
            var app = (HttpApplication)HttpContext.GetService(typeof(HttpApplication));
            int result = Executor.GetScriptExecutionStatus(app.Context, User.Identity.Name, id,isRecursive:false);
            if (result == 0)
                return WarningMessage("Execution is still going! please wait for some time, then refresh the page or click on 'Get Execution Status' button to get the latest status.");
            else if (result == 2)
            {
                return WarningMessage("Lost the background execution process due to some inexpectable error, so the ticket has been aborted, please raise a new ticket.");
            }
            return RefreshOverviewPage(id);
        }

        public void SqlScriptValidation(int ticketId)
        {
            //now automatically kick off scanner
            string targetFolder = Conf.WorkingFolder + "ticket-" + ticketId;
            var targetScriptErrors = new List<string>();
            var rollbackScriptErrors = new List<string>();
            bool targetScriptPassed = ValidateScript(targetFolder + "/target.sql", targetScriptErrors);
            bool rollbackScriptPassed = ValidateScript(targetFolder + "/rollback.sql", rollbackScriptErrors);

            string logMessage = string.Empty;
            if (targetScriptPassed && rollbackScriptPassed)
            {
                logMessage = "SQL script auto validation passed.";
                if (!StateMachine.AddValidationSucceedState(ticketId, logMessage))
                {
                    //TODO: redirect to error page and inform admin on the state saving issue 
                    throw new Exception("Failed updating file uploaded state.");
                }

                // TODO: send notificaiton email, etc, to registered approvers.

                //StateMachine.AddStageWaitForPushState(ticket.TicketId, "Wait for approvers' authorization.");
                StateMachine.AddProductWaitForPush(ticketId, "Wait for approvers' authorization.");
            }
            else
            {
                logMessage = "SQL script auto validation failed due to prohibited SQL is detected,  -- [target.sql:" +
                             targetScriptPassed + ", rollback.sql:" + rollbackScriptPassed + " ].";

                if (targetScriptErrors.Count > 0)
                {
                    logMessage += "  Target Script Error Message:  " + targetScriptErrors[0];
                }

                if (rollbackScriptErrors.Count > 0)
                {
                    logMessage += "  Rollback Script Error Message:  " + rollbackScriptErrors[0];
                }

                //NOTE: this should be failed state, not succeed state.
                if (!StateMachine.AddValidationFailedState(ticketId, logMessage))
                {
                    //TODO: redirect to error page and inform admin on the state saving issue 
                    throw new Exception("Failed updating validation failure state.");
                }

                // Now We have to allow manual overwrite operation from DBA team, so that DBA team can 
                //      take advantage of human intelligence to review scripts that failed on auto validators. 
                //      -- introduce new states of manual validation
                //      -- show choices to users, and let them decide either go with flow termination and resubmit after correction, 
                //         or go with DBA manual validation flow.
                //      -- termination flow needs to be moved to another action handler.
                logMessage = "Wait for user's decision on starting DBA manual validation process or not.";
                if (!StateMachine.AddDBAOverwriteWaitForDecisionState(ticketId, logMessage))
                {
                    //TODO: redirect to error page and inform admin on the state saving issue 
                    throw new Exception("Failed updating wait for DBA overwrite decision state.");
                }

            } // end of failed scripts on validation 
        }

        public ActionResult KickOffDbaManualValidation(int id) {
            //TODO: should admin or DBA have rights to kick off such request? That's a business team's question. 
            // This is very special use case that, any valid user, either requestor or approver, should be able to request kicking off a manual validation by DBA.
            string access = StateMachine.HasAccessToTicket(id, User.Identity.Name);
            if (string.IsNullOrWhiteSpace(access)) {
                return RedirectToAction("NoAccess");
            }

            // However, we still needs to check if someone else alredy made such change.
            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            if (state.Code != FlowState.DBAOverwriteWaitForDecision.Code) {
                // status already being updated by another user.
                ILog tlog = GetLoggerForTicket(id);
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to kick off DBA manual validation process.");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }

            //TODO: send emails here. 
            //      -- Should we do it in async way as well? 
            //      -- Make body and subject customizable. 
            //      -- Error handling... it is non-fatal features, should we move on or what?

            // need some testing on alt view... rollback to old code for now
            EmailUtil.SendDbaValidaitonRequest(id, User.Identity.Name);
            //using (SmtpClient smtpClient = new SmtpClient()) {
            //    MailMessage message = new MailMessage();
            //    message.From = new MailAddress(Conf.MailFrom);

            //    AddDbaEmails(message); 
            //    // should just cc ticket owner 
            //    message.CC.Add(t.UserId + "@calheers.ca.gov");
            //    message.Bcc.Add("gourav.shrivastava@accenture.com");
            //    //message.CC.Add(new MailAddress( User.Identity.Name + "@accenture.com" ));

            //    message.IsBodyHtml = true;
            //    message.Subject = "DBA manual script validation request for ticket " + id;
            //    message.Body = "<!DOCTYPE html><html><head></head><body>Self Servic SQL user [" + User.Identity.Name + "] sent a manual script validation request for ticket " + GetUrlForTicket(id) + ", please take some time to valiate it.</body></html>";
            //    smtpClient.Credentials = new NetworkCredential("gourav.shrivastava@gmail.com", "newdelhi");
            //    smtpClient.EnableSsl = true;
            //    // do not really send it until the smtp testing environment is ready, but we still want to debug smtp address population... so just check at the last minute
            //    if (Conf.SMTPEnabled) {
            //        try {
            //            smtpClient.Send(message);
            //        } catch ( Exception ex ) {
            //            //log it
            //            Logger.Error(ex.Message);
            //            return WarningMessage("SMTP exception: " + ex.Message + "\nPlease refresh the page.");
            //        } finally {
            //            smtpClient.Dispose();
            //        }
            //    }
            //}
           
            // needs to send a short message for DBA messenger as well
            if (Conf.MQ.Enabled) {
                MessageQueueHub.SendNewDbaValidationRequestMessage("Ticket " + t.TicketId + " for [" + t.Project.Name + "] project requested manual DBA validation process.");
            }

            string logMessage = "DBA manual validation workflow is kicked off, please wait for DBA team's quick response.";
            if (!StateMachine.AddDBAOverwriteRequestedState(id, logMessage)) {
                // should be redirected to error page incase of updating failure.
                throw new Exception("Fail to update DBA Overwrite Requested status.");
            }

            // need to refresh the page to let user see updated states. 
            return RefreshOverviewPage(id);
        }

        public ActionResult NoDbaValidationAndTerminateFlow(int id) {
            // This is very special use case that, any valid user, either requestor or approver, should be able to request kicking off a manual validation by DBA.
            string access = StateMachine.HasAccessToTicket(id, User.Identity.Name);
            if (string.IsNullOrWhiteSpace(access)) {
                return RedirectToAction("NoAccess");
            }

            //check if others already made decision and moved on, if so, inform user to refresh page manually to get latest status. 
            //TODO: maybe some auto timer refresh
            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            if (state.Code != FlowState.DBAOverwriteWaitForDecision.Code) {
                // status already being updated by another user.
                ILog tlog = GetLoggerForTicket(id);
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to bypass DBA manual validation process and terminate the workflow.");
                return WarningMessage("Workflow state has been changed by others while we are waiting for your decision, please refresh the page to get latest status.");
            }

            string logMessage = "User aborted DBA manual validation process and decided to terminate the workflow.";
            // We terminate the whole flow like it was before.
            if (!StateMachine.AddTerminateOnErrorState(id, logMessage)) {
                // should be redirected to error page incase of updating failure.
                throw new Exception("Fail to update terminate on error status when user choose not to start DBA manual validation process -- current Workflow should be treated as TERMINATED.");
            }

            // return json string to inform user. 
            return RefreshOverviewPage(id);
        }

        #region DBA manual validation decision 

        [Authorize(Roles = "DBA,Admin")]
        public ActionResult DbaApproveSqlScript(int id)
        {
            //NOTE: access control is done via attributes above, and it is actually the SystemRole in db, which is independent of each project's roles.

            // Comment is optional, need to insert to db if it is not blank.
            string commentStr = Request.Params["comment"];
            if (!String.IsNullOrWhiteSpace(commentStr))
            {
                Comment comment = new Comment() { TicketId = id, Text = commentStr.Trim(), ByUser = User.Identity.Name, CreatedAt = DateTime.Now };
                db.Comments.Add(comment);
                db.SaveChanges();
            }

            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            ILog tlog = GetLoggerForTicket(id);
            if (state.Code != FlowState.DBAOverwriteRequested.Code)
            {
                // someone else must must already handled this, just give some warning message back
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to approve manual DBA overwrite request..");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }

            string logMessage = "DBA [" + User.Identity.Name + "] approved the request of manual script checking, and overwrite the auto valiation result. Now the scripts are ready for execution.";
            // We terminate the whole flow like it was before.
            if (!StateMachine.AddDBAOverwriteApprovedState(id, logMessage))
            {
                // should be redirected to error page incase of updating failure.
                throw new Exception("Fail to update DBA overwrite approved status.");
            }
            tlog.Info(DateTime.Now + " [ticket: " + id + "] " + logMessage);

            //NOTE: now we need automatically swich to staging push status.
            logMessage = "Resume workflow, and waiting for user's decision to execute the target script.";
            //if(!StateMachine.AddStageWaitForPushState(id, logMessage)){
            //    throw new Exception("Fail to update wait for staging push status after DBA approved scripts.");
            //}
            if (!StateMachine.AddProductWaitForPush(id, logMessage))
            {
                throw new Exception("Fail to update wait for production push status after DBA approved scripts.");
            }
            // EmailUtil.SendDbaDecisionEmail("approve", id,  requestorName: t.CreatedBy.UserId, userName: User.Identity.Name);

            using (SmtpClient smtpClient = new SmtpClient())
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress(Conf.MailFrom);

                AddDbaEmails(message);
                // should just cc ticket owner 
                message.CC.Add(t.UserId + "@accenture.com");

                message.Subject = "DBA validated and approved ticket " + id;
                message.Body = "DBA [" + User.Identity.Name + "] approved validation request for ticket " + id + ", please continue your workflow.";

                smtpClient.EnableSsl = false;

                // do not really send it until the smtp testing environment is ready.
                if (Conf.SMTPEnabled)
                {
                    try
                    {
                        smtpClient.Send(message);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                }
            }

            // needs to send a short message for DBA messenger as well
            if (Conf.MQ.Enabled)
            {
                MessageQueueHub.SendDbaDecisionMessage("Ticket " + t.TicketId + " for [" + t.Project.Name + "] project is approved by " + User.Identity.Name);
            }

            if (ComeFromDbaCenter())
            {
                return RefreshDbaPage();
            }

            return RefreshOverviewPage(id);
        }

        [Authorize(Roles="DBA,Admin")] 
        public ActionResult DbaRejectSqlScript(int id) {

            //NOTE: access control is done via attributes above, and it is actually the SystemRole in db, which is independent of each project's roles.
            // Comment is optional, need to insert to db if it is not blank.
            string commentStr = Request.Params["comment"];
            if (!String.IsNullOrWhiteSpace(commentStr)) {
                Comment comment = new Comment() { TicketId = id, Text = commentStr.Trim(), ByUser = User.Identity.Name, CreatedAt = DateTime.Now };
                db.Comments.Add(comment);
                db.SaveChanges();
            }

            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            ILog tlog = GetLoggerForTicket(id);
            if (state.Code != FlowState.DBAOverwriteRequested.Code) {
                // someone else must must already handled this, just give some warning message back
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to reject manual DBA overwrite request..");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }

            string logMessage = "DBA [" + User.Identity.Name + "] rejected the request of manual script validation, and workflow is automatically terminated now.";
            // We terminate the whole flow like it was before.
            if (!StateMachine.AddDBAOverwriteRejectedState(id, logMessage)) {
                // should be redirected to error page incase of updating failure.
                throw new Exception("Fail to update DBA overwrite rejected status.");
            }

            tlog.Info(DateTime.Now + " [ticket: " + id + "] " + logMessage);
            
            //NOTE: now we need automatically swich to staging/prod push status.
            logMessage = "Terminate workflow after DBA reject manual validation request.";
            if(!StateMachine.AddTerminateOnErrorState(id, logMessage)){
                throw new Exception("Fail to terminate workflow after DBA rejected manual valiation.");
            }
            
            //TODO: Move the code into EmailUtil class 
            //EmailUtil.SendDbaDecisionEmail("reject", id,  requestorName: t.CreatedBy.UserId, userName: User.Identity.Name);
            /*
            using (SmtpClient smtpClient = new SmtpClient()) {

                MailMessage message = new MailMessage();
                message.From = new MailAddress(Conf.MailFrom);

                AddDbaEmails(message); 
                // should just cc ticket owner 
                message.CC.Add(t.UserId + "@accenture.com");

                message.Subject = "DBA validated but rejected ticket " + id;
                message.Body = "DBA [" + User.Identity.Name + "] rejected validation request for ticket " + id + ", please check the web site for addtional comments.";

                smtpClient.EnableSsl = false;

                // do not really send it until the smtp testing environment is ready.
                if (Conf.SMTPEnabled) {
                    try
                    {
                        smtpClient.Send(message);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                }
            }
            */
            // needs to send a short message for DBA messenger as well
            if (Conf.MQ.Enabled) {
                MessageQueueHub.SendDbaDecisionMessage("Ticket " + t.TicketId + " for [" + t.Project.Name + "] project is rejected by " + User.Identity.Name );
            }

            if (ComeFromDbaCenter()) {
                return RefreshDbaPage();
            }

            return RefreshOverviewPage(id);
        }

        #endregion 

        [HttpPost]
        public ActionResult PushToStaging(int id, string dbUserName = null, string dbPassword = null) {
            //TODO: validate the user is 1) approver 2) not the original owner of this request 3) does have access to the ticket -- i.e., not trying to hack to get access to other project environment   
            bool isApprover = "Approver".Equals(StateMachine.HasAccessToTicket(id, User.Identity.Name));
            bool isOwner = StateMachine.IsCreator(id, User.Identity.Name);
            if (!isApprover || isOwner) {
                return RedirectToAction("NoAccess");
            }

            //TODO: assert work flow must be in the push to staging user action stage
            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            if (state.Code != FlowState.StageWaitForPush.Code) {
                // someone else must must already handled this, just give some warning message back
                ILog tlog = GetLoggerForTicket(id);
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to push script to the staging environment.");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }

            //var lockInfo = GetLock(ticketId: id, serverCategory: "Staging", stepDescreption: "pushing to staging");
            //if( lockInfo.HasError ) {
            //    return WarningMessage(lockInfo.LogMessage);
            //}
            var app = (HttpApplication)HttpContext.GetService(typeof(HttpApplication));
            Executor.AsyncPushToStage(HttpContext.User.Identity.Name, app.Context, id, dbUserName, dbPassword);
            //NOTE: don't even bother go with .Json() to return a JsonResult, go with fast and raw  stuff content, if you know how, :)
            return RefreshOverviewPage(id);
        }

        public ActionResult PushToProd(int id, string dbUserName = null, string dbPassword = null)
        {
            //TODO: validate the user is 1) approver 2) not the original owner of this request 3) does have access to the ticket -- i.e., not trying to hack to get access to other project environment   
            bool isApprover = "Approver".Equals(StateMachine.HasAccessToTicket(id, User.Identity.Name));
            bool isOwner = StateMachine.IsCreator(id, User.Identity.Name);
            if (!isApprover && !isOwner)
            {
                return RedirectToAction("NoAccess");
            }
            //TODO: assert work flow must be in the push to staging user action stage
            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            ILog tlog = null;
            if (state.Code != FlowState.ProductWaitForPush.Code)
            {
                // someone else must must already handled this, just give some warning message back
                tlog = GetLoggerForTicket(id);
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to push this script to the staging environment.");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }
            try
            {
                if (Executor.ValidateDBLogin(HttpContext.User.Identity.Name, id, dbUserName, dbPassword))
                {
                    var lockInfo = GetLock(ticketId: id, serverCategory: "Production", stepDescreption: "pushing to production");
                    if (lockInfo.HasError)
                        return WarningMessage(lockInfo.LogMessage);
                    var app = (HttpApplication)HttpContext.GetService(typeof(HttpApplication));
                    Executor.AsyncPushToProd(HttpContext.User.Identity.Name, app.Context, id, Conf.OracleServiceAccountName, Conf.OracleServiceAccountPassword, dbUserName);
                }
                else
                    return WarningMessage("Login failed, due to incorrect Username or Password");
            }
            catch (Exception ex)
            {
                tlog = GetLoggerForTicket(id);
                tlog.Error(DateTime.Now + " Error [" + User.Identity.Name + "]" +  ex.Message);
            }
                return RefreshOverviewPage(id);
        }

        #region staging execution validation
        public ActionResult ConfirmStagingExecution(int id) {
            bool isApprover = "Approver".Equals(StateMachine.HasAccessToTicket(id, User.Identity.Name));
            bool isOwner = StateMachine.IsCreator(id, User.Identity.Name);
            if (!isApprover && !isOwner) {
                return RedirectToAction("NoAccess");
            }

            //StateMachine.AddStagingExecutionValidationiConfirmedState(id, "Staging execution results confirmed by: " + HttpContext.User.Identity.Name);
            StateMachine.AddProductWaitForPush(id, "Waiting for approver's permisson to push to production");
            //return Redirect("/Ticket/Overview/" + id);
            return RefreshOverviewPage(id);
        }

        public ActionResult RejectStagingExecution(int id, string dbUserName = null, string dbPassword = null) {
            bool isApprover = "Approver".Equals(StateMachine.HasAccessToTicket(id, User.Identity.Name));
            bool isOwner = StateMachine.IsCreator(id, User.Identity.Name);
            if (!isApprover && !isOwner) {
                return RedirectToAction("NoAccess");
            }

            //NOTE: also need to grab lock
            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            if (state.Code != FlowState.StageExecutionValidationWaiting.Code) {
                // someone else must must already handled this, just give some warning message back
                ILog tlog = GetLoggerForTicket(id);
                tlog.Info(DateTime.Now  + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to reject staging execution results.");
                return WarningMessage("Workflow state has been changed by others, current status is: " + state.Name + ",  please refresh the page to get latest status.");
            }

            var lockInfo = GetLock(ticketId: id, serverCategory: "Staging", stepDescreption: "User rejected staging execution results, acquire lock here to rollback staging.");
            if( lockInfo.HasError ) {
                return WarningMessage(lockInfo.LogMessage);
            }

            // only update state before we really kick off rollback 
            //StateMachine.AddStagingExecutionValidationiRejectedState(id, "Staging execution results rejected by: " + HttpContext.User.Identity.Name);

            //NOTE: now flow termiantion is handled in the callback method, in an async way, and the lock should be released there.
            var app = (HttpApplication)HttpContext.GetService(typeof(HttpApplication));
            Executor.AsyncStagingRollbackAction(app.Context, HttpContext.User.Identity.Name,  id, RollbackReason.OnProdRejection,  
                                                callback:Executor.TerminateWorkFlowOnErrorCallBack, 
                                                state: new Dictionary<string, object> {
                                                                {"TicketId", id }, 
                                                                {"LogMessage", "workflow terminated on error."} , 
                                                                {"ReleaseLock", true}, 
                                                                {"UnlockReason", "User rejected Stage execution results, release lock after Stage rollback attmpt."}
                                               }, dbUserName: dbUserName, dbPassword:dbPassword);

            return RefreshOverviewPage(id);
        }
        #endregion

        #region production execution validation

        public ActionResult ConfirmProdExecution(int id, string dbUserName = null, string dbPassword = null)
        {
            bool isApprover = "Approver".Equals(StateMachine.HasAccessToTicket(id, User.Identity.Name));
            bool isOwner = StateMachine.IsCreator(id, User.Identity.Name);
            ILog tlog = null;
            if (!isApprover && !isOwner)
            {
                return RedirectToAction("NoAccess");
            }

            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            if (state.Code != FlowState.ProductExeuctionValidationWaiting.Code)
            {
                // someone else must must already handled this, just give some warning message back
                tlog = GetLoggerForTicket(id);
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to commit the target script into the Production environment.");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }

            //TODO:Do I need to validate user's login again for commit the changes?
            tlog = GetLoggerForTicket(id);
            var app = (HttpApplication)HttpContext.GetService(typeof(HttpApplication));
            if (Executor.CommitChanges(app.Context, id, dbUserName, dbPassword))
            {
                tlog.Info(DateTime.Now + " [ticket: " + id + "] Prod execution result confirmed by: [" + HttpContext.User.Identity.Name + "]");
                StateMachine.AddProdExecutionValidationConfirmedState(id, "Production execution results confirmed by: [" + HttpContext.User.Identity.Name +"]");
                tlog.Info(DateTime.Now + " [ticket: " + id + "] workflow has been completed successfully");
                StateMachine.AddTerminateNormallyState(id, "Workflow terminated successfully.");
            }
            else
            {
                tlog.Info(DateTime.Now + " [ticket: " + id + "] Workflow terminated due to error");
                StateMachine.AddTerminateOnErrorState(id, "Workflow terminated due to error.");
            }
            return RefreshOverviewPage(id);
        }

        public ActionResult RejectProdExecution(int id, string dbUserName = null, string dbPassword = null)
        {
            bool isApprover = "Approver".Equals(StateMachine.HasAccessToTicket(id, User.Identity.Name));
            bool isOwner = StateMachine.IsCreator(id, User.Identity.Name);
            if (!isApprover && !isOwner)
            {
                return RedirectToAction("NoAccess");
            }

            //NOTE: also need to grab lock
            var t = db.Tickets.Find(id);
            var state = t.WorkStates.Last();
            if (state.Code != FlowState.ProductExeuctionValidationWaiting.Code)
            {
                // someone else must must already handled this, just give some warning message back
                ILog tlog = GetLoggerForTicket(id);
                tlog.Info(DateTime.Now + " Workflow latest status has beeen changed when user [" + User.Identity.Name + "] tried to push the rollback script into the Production environment.");
                return WarningMessage("Workflow state has been changed by others, please refresh the page to get latest status.");
            }

            if (Executor.ValidateDBLogin(HttpContext.User.Identity.Name, id, dbUserName, dbPassword))
            {
                ILog tlog = GetLoggerForTicket(id);
                tlog.Info(DateTime.Now + " User account has been validate for : [" + User.Identity.Name + "]");

                var lockInfo = GetLock(ticketId: id, serverCategory: "Production", stepDescreption: "User rejected production execution results, acquire processing lock here to rollback both production and staging.");
                if (lockInfo.HasError)
                {
                    return WarningMessage(lockInfo.LogMessage);
                }
                var app = (HttpApplication)HttpContext.GetService(typeof(HttpApplication));
                //NOTE: since tasks are kicked off in aync way, it is really hard to control which one terminated first, for now we just kick off prod rollback script first
                Executor.AsyncProdAndStagingRollbackAction(app.Context, HttpContext.User.Identity.Name, id, RollbackReason.OnProdRejection,
                                                            dbUserName: Conf.OracleServiceAccountName, dbPassword: Conf.OracleServiceAccountPassword,
                                                            callback: Executor.TerminateWorkFlowOnErrorCallBack,
                                                            state: new Dictionary<string, object> {
                                                                {"TicketId", id }, 
                                                                {"LogMessage", "Workflow terminated on error."} , 
                                                                {"ReleaseLock", true}, 
                                                                {"UnlockReason", "User rejected production execution results, release lock after production and staging rollback attmpt."}
                                                         });
                // update status after locks being granted
                StateMachine.AddProdExecutionValidationRejectedState(id, "Production execution results rejected by: " + HttpContext.User.Identity.Name);
                tlog.Info(DateTime.Now + " Production execution results rejected by: [" + User.Identity.Name + "]");
            }
            else
            {
                return WarningMessage("Login failed, due to incorrect user name or password");
            }
            return RefreshOverviewPage(id);
        }
        #endregion

        #region user abort
        public ActionResult UserAbort(int id) {
            StateMachine.AddUserAbortState(id, "Request is aborted by user, workflow is brutally terminated with no rollback.");
            return RefreshOverviewPage(id);
        }

        #endregion

        #region helper methods

        private bool ComeFromDbaCenter() {
            return Request.UrlReferrer != null
                && (Request.UrlReferrer.LocalPath.Equals("/DBA", StringComparison.CurrentCultureIgnoreCase)
                || Request.UrlReferrer.LocalPath.Equals("/DBA/", StringComparison.CurrentCultureIgnoreCase));
        }

        private JavaScriptResult RefreshOverviewPage(int id) {
            //NOTE: to avoid the infamous IE caching issue.
            return JavaScript("{\"redirect\":\"/Ticket/Overview/" + id + "?t=" + DateTime.Now.Ticks + "\"  }");
        }

        private JavaScriptResult RefreshDbaPage( ) {
            return JavaScript("{\"redirect\":\"/DBA" + "?t=" + DateTime.Now.Ticks + "\"  }");
        }

        private JavaScriptResult WarningMessage(string msg) {
            return JavaScript("{\"warning\":\"" + msg + "\"  }");
        }

        protected bool ValidateScript(string filePath, List<string> errors) {
            using (var f = new StreamReader(filePath)) {
                var sqlStr = f.ReadToEnd();
                return SqlValidation.Validate(sqlStr, errors);
            }
        }

        private static string LockingTextLogMessage(DataRow dt) {
            //NOTE: you need to double \\ to escape the JSON string, otherwise, you will see client side JavaScript error.
            StringBuilder sb = new StringBuilder();
            sb.Append("Project Name:").Append(dt["ProjectName"]).Append("\\n");
            sb.Append("Stage:").Append(dt["ServerCategory"]).Append("\\n");
            sb.Append("Ticket ID:").Append(dt["TicketId"]).Append("\\n");
            sb.Append("Locked By:").Append(dt["LockedBy"]).Append("\\n");
            sb.Append("Locked At:").Append(dt["LockedAt"]).Append("\\n");
            sb.Append("Lock Reaons:").Append(dt["LockReason"]).Append("\\n");
            return sb.ToString();
        }

        #endregion

        #region locking check logic 

        protected  internal class LockingInfo {
            public bool HasError { get; set; }
            public int ErrorCode { get; set; }
            public int ErrorMsg { get; set;  }
            public string LogMessage { get; set;  }
            public string ExtraInfo { get; set; }
        }

        private LockingInfo GetLock( int ticketId, string serverCategory, string stepDescreption) {

            var info = new LockingInfo();
            var tlog = GetLoggerForTicket(ticketId);

            using (var dt = DaoUtil.GetLock(ticketId, serverCategory, User.Identity.Name, stepDescreption)) {
                // no matter what, dt should not be null and should only contains ONE row
                if (dt == null) {
                    info.LogMessage = new StringBuilder()
                                          .Append("Project could not be locked.  Unable to execute script.  [")
                                          .Append( ticketId).Append("] at step [").Append(stepDescreption).Append("] by user [")
                                          .Append( User.Identity.Name ).Append("], please contact admin immediately.")
                                          .ToString();
                    info.HasError = true; 
                    tlog.Fatal(info.LogMessage);
                } else  if (dt.Rows.Count != 1) {
                    info.LogMessage = new StringBuilder()
                                          .Append("Multiple locks were found for the project.  [")
                                          .Append( ticketId).Append("] at step [").Append(stepDescreption).Append("] by user [")
                                          .Append( User.Identity.Name).Append("], please contact admin immediately.")
                                          .ToString();
                    info.ExtraInfo = dt.ToXml();
                    info.HasError = true;
                    tlog.Fatal(info.LogMessage);
                } else if (dt.Rows[0]["ErrorMsg"] != DBNull.Value || dt.Rows[0]["ErrorCode"] != DBNull.Value) {
                    // check if error code or error message is null or not
                    info.LogMessage = new StringBuilder()
                                          .Append("Only ONE script is allowed to execute against this project, please try later.")
                                          .Append(" \\n\\n =====================================\\n Process currently running:\\n" )
                                          .Append(LockingTextLogMessage(dt.Rows[0]))
                                          .ToString();
                    info.ExtraInfo = dt.ToXml();
                    info.HasError = true;

                    // this is common case, not real error, just use info logging level
                    tlog.Info(DateTime.Now + " " + info.LogMessage);
                }
            }
            return info; 
        }
        #endregion 
    }
}