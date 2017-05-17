using iSql.Commons;
using iSql.Commons.Models;
using iSql.Shell;
using iSql.Util;
using iSQLWeb.Controllers;
using log4net;
using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace iSQLWeb
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected static ILog Log;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // initilaize db 
            //Database.SetInitializer<EntDbContext>(new DropCreateDatabaseAlways<EntDbContext>());
            DbInitializerManager.SetInitializer();

            // make sure log4net is loaded
            var logFile = HttpContext.Current.Server.MapPath("~/Logs/sss-testing.log");
            log4net.GlobalContext.Properties["LogName"] = logFile;

            log4net.Config.XmlConfigurator.Configure(new FileInfo(HttpContext.Current.Server.MapPath("~/log4net.xml")));
            Log = LogManager.GetLogger(this.GetType());
            Log.Info("applicaiton started");

            //Log.Info("start message queue receiver for dba notificaiton queue.");
            //MessageQueueHub.KickOffMessageProcessing();
            //Log.Info("queue receiver started.");
            //TODO: Uncomment when implemented completly
            StartCheckingBackgroundProcessStatus();
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;
            Response.Clear();
            Server.ClearError();
            var routeData = new RouteData();
            routeData.Values["controller"] = "Errors";
            routeData.Values["action"] = "General";
            routeData.Values["exception"] = exception;
            Response.StatusCode = 500;
            if (httpException != null)
            {
                Response.StatusCode = httpException.GetHttpCode();
                switch (Response.StatusCode)
                {
                    case 401:
                        routeData.Values["action"] = "Unauthorized";
                        break;
                    case 404:
                        routeData.Values["action"] = "Http404";
                        break;
                    default:
                        try
                        {
                            if (httpException.InnerException != null)
                                Log.Error("UserId [" + HttpContext.Current.User.Identity.Name
                                                     + "] Message- " + httpException.Message + "  "
                                                     + Convert.ToString(httpException.InnerException.Message));
                            else
                                Log.Error("UserId [" + HttpContext.Current.User.Identity.Name
                                                     + "] Message - " + httpException.Message);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(DateTime.Now + " Error: " + ex.Message);
                        }
                        break;
                }
            }
            // Avoid IIS7 getting in the middle
            Response.TrySkipIisCustomErrors = true;
            IController errorsController = new ErrorController();
            HttpContextWrapper wrapper = new HttpContextWrapper(Context);
            var rc = new RequestContext(wrapper, routeData);
            errorsController.Execute(rc);
        }

        /// <summary>
        /// This routine creates a background worker and starts it working on another thread to 
        /// periodically poll SQL server and refresh this applications cached data as needed.
        /// </summary>
        public static void StartCheckingBackgroundProcessStatus()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(CheckAndUpdateStatus);
            worker.WorkerReportsProgress = false;
            worker.WorkerSupportsCancellation = true;

            worker.RunWorkerCompleted +=
                   new RunWorkerCompletedEventHandler(SqlPollingWorkCompleted);
            worker.RunWorkerAsync(HttpContext.Current); //Pass HttpContext to background worker

            //Add this BackgroundWorker object instance to the application variables 
            //so it can be cleared when the Application_End event fires.
            HttpContext.Current.Application["SqlPollingBackgroundWorker"] = worker;
        }

        private static void SqlPollingWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Do nothing
        }

        private static void CheckAndUpdateStatus(object sender, DoWorkEventArgs e)
        {
            //This loop will run as long as the web application is running. When the web
            //application stops, this process will be stopped.
            int bgProcessOccrance = 5;
            string _checkStatus = ConfigurationManager.AppSettings["CheckTicketStatusInMin"];
            int timer;
            bool res = int.TryParse(_checkStatus, out timer);
            if (res)
            {
                bgProcessOccrance = Convert.ToInt32(_checkStatus);
            }
            while (true)
            {
                //Sleep for some period of time
                System.Threading.Thread.Sleep(bgProcessOccrance * 60 * 1000);
                //TODO: Check the database for changes and if they
                //exist refresh the data stored in the cache.
                Executor.CheckRunningProcessStatus((HttpContext)e.Argument);
            }
        }
    }
}