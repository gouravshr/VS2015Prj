using iSql.Commons;
using iSql.Commons.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace iSql.Util
{
    public class EmailUtil {

        protected static ILog Logger = log4net.LogManager.GetLogger(typeof(EmailUtil));
        
        /// <summary>
        /// Method for sending email
        /// </summary>
        /// <param name="message"></param>
        /// <param name="attention"></param>
        /// <param name="detail"></param>
        /// <param name="action"></param>
        public static void SendMail(ref MailMessage message, string attention, string detail, string action) {
            // need to format the body first
            string body = String.Format(Conf.MailBodyTemplate, attention, detail, action);

            SmtpClient smtpClient = null;
            try
            {
                smtpClient = new SmtpClient();

                AlternateView altView = AlternateView.CreateAlternateViewFromString(body, Encoding.UTF8, MediaTypeNames.Text.Html);
                LinkedResource resource = new LinkedResource(Conf.MailBannerFile, MediaTypeNames.Image.Jpeg);

                // have to assign unique id
                resource.ContentId = Guid.NewGuid().ToString();
                altView.LinkedResources.Add(resource);

                //message = new MailMessage();
                message.AlternateViews.Add(altView);

                Attachment att = new Attachment(Conf.MailBannerFile);
                att.ContentDisposition.Inline = true;

                message.IsBodyHtml = true;
                message.Attachments.Add(att);

                smtpClient.Credentials = new NetworkCredential("gourav.shrivastava@gmail.com", "newdelhi");
                smtpClient.EnableSsl = true;

                // we want to async send... the object can be anything, we don't care callback at this point anyway.
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                // need to log it
                // in sync call, we should handle errors, but in async backtround thread condition, we cannot let exception thrown to ruin the whole thread, which may not 
                // be properly hanlded in app level and cause w3wp down, or even it is configured properly, we cannot let such exception to cause work states issue -- email
                // is just one notificaiton approach here, it is not essential part, even email failed, we will let workflow complete.
                Logger.Error("As error has occurred during sending email: " + ex.Message);
            }
            finally
            {
                if (smtpClient != null)
                {
                    smtpClient.Dispose();
                }

                if (message != null)
                {
                    message.Dispose();
                }
            }
        }

        public static void SendDbaValidaitonRequest(int ticketId, string requestorName)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(Conf.MailFrom);

            AddDbaEmails(message);
            // should just cc ticket owner 
            message.CC.Add(requestorName + "@accenture.com");
            //message.CC.Add(new MailAddress( User.Identity.Name + "@accenture.com" ));

            message.Subject = "DBA manual script validation request for ticket " + ticketId;
            message.Body = "Self Servic SQL user [" + requestorName + "] sent a manual script validation request for ticket " + GetUrlForTicket(ticketId) + ", please take some time to valiate it.";
            message.IsBodyHtml = true;

            string attention = "Self Service SQL DBA Team";

            StringBuilder sb = new StringBuilder();
            string detail = sb.Append("Self Service SQL user <b>[").Append(requestorName)
                              .Append("]</b> sent a manual script validation request for ticket </span>").Append(GetUrlForTicket(ticketId))
                              .ToString();

            string action = "Please validate the script by clicking on the link above and Approve / Reject the request";

            // do not really send it until the smtp testing environment is ready, but we still want to debug smtp address population... so just check at the last minute
            if (Conf.SMTPEnabled)
            {
                SendMail(ref message, attention, detail, action);
            }
        }

        public static void SendDbaDecisionEmail(string decision, int ticketId, string requestorName, string userName)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(Conf.MailFrom);

            AddDbaEmails(message);
            message.CC.Add(requestorName + "@accenture.com");
            //message.CC.Add(new MailAddress( User.Identity.Name + "@accenture.com" ));

            StringBuilder sb = new StringBuilder();
            string verdict = string.Equals("approve", decision, StringComparison.CurrentCultureIgnoreCase) ? "approved" : "rejected";
            message.Subject = sb.Append("DBA ").Append(decision).Append(" manual script validation request for ticket ").Append(ticketId).ToString();
            message.Body = "Ticket " + GetUrlForTicket(ticketId) + ", please take some time to valiate it.";
            message.IsBodyHtml = true;

            string attention = "Self Service SQL DBA Team";
            sb.Clear();
            string detail = sb.Append("Ticket ").Append(GetUrlForTicket(ticketId))
                              .Append(" has been ").Append(verdict).Append(".")
                              .ToString();
            string action = "None";
            // do not really send it until the smtp testing environment is ready, but we still want to debug smtp address population... so just check at the last minute
            if (Conf.SMTPEnabled)
            {
                SendMail(ref message, attention, detail, action);
            }
        }

        //TODO : Whenever a new ticket will be created, email send to project's supervisor(s) to approve the request
        public static void SendSupervisorValidateRequest(int ticketId, string requestorName)
        {
            //TODO : Implementation
        }

        //TODO : Project supervisor decision will be sent to requestor(should be sent to whole project's members those have approval rights in the project???)
        public static void SendSupervisorDecisionEmail(string decision, int ticketId, string requestorName, string userName)
        { 
        //TODO : Implementation
        }

        //TODO : Final status of ticket (Either it is succeeded or failed) will be sent to whole project members
        public static void SendWorkflowFinalStatusEmail(int ticketId)
        {
            //TODO : Implementation
        }

        //TODO : Target script execution result will be sent to owner of ticket as well as Project approvers (Either it is succeeded or failed)
        public static void SendScriptExecutionStatusEmail(int ticketId)
        {
            //TODO : Implementation
        }

        // ideally, we should keep DBA data fetchinig and other logic out... but for now, this is just a wrapper naemspace to group things together and make main flow code 
        // more readable, so... we keep them here but may refactory at later time.
        public static List<string> _dbaEmails = new List<string>(); 
        public static List<string> DbaEmails {
            get {
                if (_dbaEmails.Count == 0)
                {
                    LoadAllDbaEmails(); 
                }
                return _dbaEmails;
            }
        }

        public static void LoadAllDbaEmails() 
        {
            using(var db = new EntDbContext())
            {
                _dbaEmails = (from u in db.Users where u.SystemRole == "DBA" select string.IsNullOrEmpty(u.PreferredEmailId) ? u.UserId + Conf.EmailDomain : u.PreferredEmailId).ToList<string>();
            }
        }

        public static string GetEmail(string userId)
        {
            string emailId = string.Empty;
            using (var db = new EntDbContext())
            {
                emailId = (from u in db.Users where u.UserId == userId select string.IsNullOrEmpty(u.PreferredEmailId) ? u.UserId + Conf.EmailDomain : u.PreferredEmailId).FirstOrDefault();
            }
            return emailId;
        }

        public static void AddDbaEmails(MailMessage msg)
        {
            foreach (var email in DbaEmails)
                msg.To.Add(new MailAddress(email));
        }

        public static void AddApproverEmails(MailMessage msg, int ticketId)
        {
            List<string> approvers = null;
            using (var db = new EntDbContext())
            {
                approvers = (from acc in db.Accesses join tkt in db.Tickets on acc.ProjectId equals tkt.ProjectId 
                             where acc.Role == "Approver" && tkt.TicketId == ticketId
                             select string.IsNullOrEmpty(acc.User.PreferredEmailId) ? acc.User.UserId + Conf.EmailDomain : acc.User.PreferredEmailId).ToList<string>();
            }

            foreach (var email in approvers)
                msg.To.Add(new MailAddress(email));
        }

        public static string GetUrlForTicket(int id) {
            // put special query string to let us know it is from email if we enalbe query string in IIS log
            return new StringBuilder().Append("<a href='").Append( Conf.RootUrl).Append("Ticket/Overview/").Append(id).Append("?ref=email' >").Append(id).Append("</a>").ToString();
        }
    }
}