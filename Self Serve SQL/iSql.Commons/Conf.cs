using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.IO;
using System.Diagnostics;

namespace iSql.Commons {

    /// <summary>
    /// The class serves two purposes: first of all, it can be the central place for all configuraiton entries, and in charge of loading and reloading configuraiton files and convert them to the right type;
    /// second goal is to provide easy way of unit testing if you want, all fields are intentionally made public (not even bother to abuse getter/setter since we don't have complicated logic associated with), 
    /// user can just set those properties in unit test very easily, whithout even worrying about all DI injection, etc, which is overengineering for many apps.
    /// </summary>
    public class Conf
    {

        static Conf()
        {
            Reload();
        }
        
        private static string commitScript = "commit.sql";
        private static int bgProcessOccrance;

        public static void Reload()
        {
            string _workingFolder = ConfigurationManager.AppSettings["WorkingFolder"];
            if (string.IsNullOrWhiteSpace(_workingFolder))
            {
                WorkingFolder = AppDomain.CurrentDomain.BaseDirectory + "working/";
            }
            else
            {
                WorkingFolder = _workingFolder;
            }

            //bgProcessOccrance = 5;
            //string _checkStatus = ConfigurationManager.AppSettings["CheckTicketStatusInMin"];
            //int timer;
            //bool res = int.TryParse(_checkStatus, out timer);
            //if (res)
            //{
            //    bgProcessOccrance = Convert.ToInt32(_checkStatus);
            //}

            if (!File.Exists(WorkingFolder + commitScript))
                File.WriteAllText(WorkingFolder + commitScript, "commit;");

            CommitScript = WorkingFolder + commitScript;

            bool _allowLanding = false;
            bool.TryParse(ConfigurationManager.AppSettings["AllowLandingPage"], out _allowLanding);
            AllowLandingPage = _allowLanding;

            int _maximumLogFileSize = int.MaxValue;
            int.TryParse(ConfigurationManager.AppSettings["MaximumLogFileSize"], out _maximumLogFileSize);
            MaximumLogFileSize = _maximumLogFileSize;

            // load ADFS claim keys, and assume they are there even when ADFS disabled.
            ClaimGroup = ConfigurationManager.AppSettings["AdfsClaimGroup"];
            ClaimEnterpriseID = ConfigurationManager.AppSettings["AdfsClaimEnterpriseId"];

            // read email configuration 
            MailFrom = ConfigurationManager.AppSettings["MailFrom"];

            string emailTemplateFolder = AppDomain.CurrentDomain.BaseDirectory + "Templates/Email/";
            string templateFileName = ConfigurationManager.AppSettings["MailBodyTemplateFile"];
            if (!templateFileName.Contains('\\'))
            {
                // no path include, go with default folder
                templateFileName = emailTemplateFolder + '\\' + templateFileName;
            }
            using (StreamReader sr = new StreamReader(templateFileName))
            {
                MailBodyTemplate = sr.ReadToEnd();
            }

            MailBannerFile = ConfigurationManager.AppSettings["MailBannerFile"];
            if (!MailBannerFile.Contains('\\'))
            {
                MailBannerFile = emailTemplateFolder + '\\' + MailBannerFile;
            }

            EmailDomain = ConfigurationManager.AppSettings["EmailDomain"];
            if (!EmailDomain.Contains('@'))
            {
                EmailDomain = "@" + EmailDomain;
            }
            
            SMTPEnabled = bool.Parse(ConfigurationManager.AppSettings["SMTPEnabled"]);

            RootUrl = ConfigurationManager.AppSettings["RootUrl"];

            // load message related messages 
            MQ.Enabled = bool.Parse(ConfigurationManager.AppSettings["MQ_Enabled"]);
            MQ.HostName = ConfigurationManager.AppSettings["MQ_HostName"];
            MQ.DbaMessageTTL = int.Parse(ConfigurationManager.AppSettings["MQ_DBA_Message_TTL"]);

            MQ.DbaNotificationQueue = ConfigurationManager.AppSettings["MQ_DBA_Notification_Queue"];

            MQ.DbaNewRequestQueue = ConfigurationManager.AppSettings["MQ_DBA_New_Request"];
            MQ.DbaDecisionQueue = ConfigurationManager.AppSettings["MQ_DBA_Decision_Queue"];

            EnableCIOSpecificContent = bool.Parse(ConfigurationManager.AppSettings["EnableCIOSpecificContent"]);

            FooterVersionInfo = ConfigurationManager.AppSettings["FooterVersionInfo"];
        }

        #region general
        public static string WorkingFolder;

        public static bool AllowLandingPage;

        public static int MaximumLogFileSize;

        public static string CommitScript;

        public static string OracleServiceAccountName
        {
            get
            {
                return "dbcrdeploy";
            }
        }

        public static string OracleServiceAccountPassword
        {
            get
            {
                return "dbcrdeploy";
            }
        }
        #endregion

        #region ADFS related
        // Per discussion with Erin, whenever the landing page is NOT allowed, we assume we are NOT in local dev environment and assume ADFS is enabled.
        public static bool ADFSEnabled
        {
            get
            {
                return !AllowLandingPage;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        // We noticed ADFS team may configure different system's claim types under different schema keys.
        // And hence we have to make it dyanmically loaded from web.config.
        //-----------------------------------------------------------------------------------------------------

        // The claim type key with group information, in current staging system, it is"http://schemas.xmlsoap.org/claims/Group"
        public static string ClaimGroup;

        // Enterprise ID claim type key, currently is set to "http://schemas.xmlsoap.org/claims/Accenture.EnterpriseID"
        public static string ClaimEnterpriseID;

        #endregion

        #region Mail

        //NOTE: for now, we only have DBA mailing support requirement, so one group should be sufficient. 
        //      We can always extend it when multiple mailing groups are needed.

        public static string MailFrom;
        public static string MailBodyTemplate;
        public static string MailBannerFile;

        public static bool SMTPEnabled;

        public static string RootUrl;
        public static string EmailDomain;

        #endregion

        #region MQ

        public static MQConfig MQ = new MQConfig();

        #endregion

        #region CIO specific

        // need to switch on and off certain CIO specific content, before hand over to Oracle team
        public static bool EnableCIOSpecificContent;


        #endregion

        #region UI customization

        public static string FooterVersionInfo;

        #endregion
        
    }        
    public class MQConfig
    {
        public bool Enabled;
        public string HostName; 
        public string DbaNotificationQueue;
        public string DbaNewRequestQueue;
        public string DbaDecisionQueue;
        public int DbaMessageTTL; 
    }
}