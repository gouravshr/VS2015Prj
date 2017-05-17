using iSql.Commons;
using iSql.Commons.Models;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web;

namespace iSql.Shell
{
    public class Executor
    {
        protected static ILog Logger = log4net.LogManager.GetLogger(typeof(Executor));

        //TODO: make it configurable 
        public static string SqlCmdFullPath = "sqlcmd";
        public static string SqlPlusFullPath = "sqlplus.exe";
        //"c:/Program Files/Microsoft SQL Server/100/Tools/Binn/sqlcmd

        public static void AsyncExecuteSqlCmd(string args, string workingDir = null)
        {
            try
            {
                var paraArray = new object[] { args, workingDir };
                Thread backgroundThread = new Thread(new ParameterizedThreadStart(ExecuteSqlCmd));
                backgroundThread.IsBackground = true;
                backgroundThread.Priority = ThreadPriority.Normal;
                backgroundThread.Start(paraArray);
            }
            catch (ThreadStartException ex)
            {
                //TODO:
            }
            catch (ThreadAbortException ex)
            {
                //TODO:
            }
            catch (Exception ex)
            {
                //TODO:
            }
        }

        /// <summary>
        /// This mehtod is created to act as a wrapper to utilize  ParameterizedThreadStart feature in .NET 2.x.  
        /// </summary>
        /// <param name="parametersAsObjectArrary"></param>
        public static void ExecuteSqlCmd(object parametersAsObjectArrary)
        {
            //NOTE: this is NOT a type-safe way of invoking aysnc calls. To make it real type-safe, wrapper invoking class needs to be defined...to much work for now.
            if (parametersAsObjectArrary.GetType() != typeof(Array))
            {
                throw new ArgumentException("Wrong argument, except object arrays.");
            }

            var args = (object[])parametersAsObjectArrary;
            string cmd = (string)args[0];
            string workDir = args[1] as string;

            ExecuteSqlCmd(cmd, workDir);
        }

        //TO execute script for SQL Server via using SQLCMD
        public static Process ExecuteSqlCmd(string arguments, string workingDir = null)
        {
            ProcessStartInfo pinfo = new ProcessStartInfo();
            // for development purpose we may want to see the window for nowd
            pinfo.CreateNoWindow = true;
            pinfo.UseShellExecute = true;
            pinfo.WindowStyle = ProcessWindowStyle.Normal;
            pinfo.FileName = SqlCmdFullPath;

            if (null != workingDir) { pinfo.WorkingDirectory = workingDir; }

            pinfo.Arguments = arguments;

            Console.Out.WriteLine("working dir:" + pinfo.WorkingDirectory);

            Process p = null;
            // unfortunately, when process exit and disposed, you cannot access execution informaiton -- thoughts: should I not dispose it... leaking risk though?
            p = Process.Start(pinfo);
            p.WaitForExit();
            return p;
        }

        //This method basically for Oracle/sqlplus
        public static Process ExecuteSqlPlus(string arguments, string targetScript, string logFilePath, string workingDir = null, bool isScriptFile = true, bool isWaitProcess = false, string clientId = null)
        {
            ProcessStartInfo pinfo = new ProcessStartInfo();
            // for development purpose we may want to see the window for nowd
            pinfo.CreateNoWindow = true;
            pinfo.UseShellExecute = false;
            pinfo.WindowStyle = ProcessWindowStyle.Normal;
            pinfo.FileName = SqlPlusFullPath;
            if (null != workingDir) { pinfo.WorkingDirectory = workingDir; }
            pinfo.Arguments = arguments;

            Console.Out.WriteLine("working dir:" + pinfo.WorkingDirectory);

            pinfo.RedirectStandardInput = true;
            pinfo.RedirectStandardOutput = true;
            pinfo.RedirectStandardError = true;

            Process p = new Process();
            p.StartInfo = pinfo;
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.StandardInput.WriteLine("set time on");

            if (isScriptFile)
            {
                if (clientId != null)
                    p.StandardInput.WriteLine("exec dbms_session.set_identifier('" + clientId + "');");

                p.StandardInput.WriteLine("spool " + logFilePath);
                p.StandardInput.WriteLine("@" + pinfo.WorkingDirectory + "//" + targetScript);
                p.StandardInput.WriteLine("spool off");
            }
            else
                p.StandardInput.WriteLine(targetScript);

            // unfortunately, when process exit and disposed, we cannot access execution informaiton in case of wait process for commit/rollback result
            if (!isWaitProcess)
            {
                p.StandardInput.WriteLine("exit");
                p.WaitForExit();
            }
            return p;
        }

        //This method basically for Oracle/sqlplus
        public static Process ExecuteSqlPlus(Process p, string targetScript, string logFilePath = null, string workingDir = null)
        {
            if (null != workingDir) { p.StartInfo.WorkingDirectory = workingDir; }
            if (logFilePath != null)
            {
                p.StandardInput.WriteLine("spool " + logFilePath);
                p.StandardInput.WriteLine("@" + p.StartInfo.WorkingDirectory + "//" + targetScript);
                p.StandardInput.WriteLine("spool off");
            }
            else
            {
                p.StandardInput.WriteLine(targetScript);
            }
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();
            return p;
        }

        public static Process ExecuteTargetScript(int ticketId, string logFile, string extraArgs, string dbType = "sql server", bool isWaitProcess = false, string clientId=null)
        {
            //TODO:  handle various execution exception that may not caused by script itself, but by OS, such as insufficient memory, DNS issue, etc; needs more details on workflow impact  
            if (dbType.ToLower() == "sql server")
                return Executor.ExecuteSqlCmd(extraArgs + " -b -i target.sql  -o " + logFile, workingDir: Conf.WorkingFolder + "/ticket-" + ticketId);
            else
                return Executor.ExecuteSqlPlus(extraArgs, "target.sql", logFile, workingDir: Conf.WorkingFolder + "/ticket-" + ticketId, isWaitProcess: isWaitProcess, clientId:clientId);
        }

        public static void AsyncExecuteTargetScript(int ticketId, string logFile, string extraArgs, string dbType = "sql server")
        {
            Executor.AsyncExecuteSqlCmd(extraArgs + " -b -i target.sql  -o " + logFile, workingDir: Conf.WorkingFolder + "/ticket-" + ticketId);
        }

        public static Process ExecuteRollbackScript(int ticketId, string logFile, string extraArgs, string dbType = "sql server")
        {
            if (dbType.ToLower() == "sql server")
                return Executor.ExecuteSqlCmd(extraArgs + " -b -i rollback.sql  -o " + logFile, workingDir: Conf.WorkingFolder + "/ticket-" + ticketId);
            else
                return Executor.ExecuteSqlPlus(extraArgs, "rollback.sql", logFile, workingDir: Conf.WorkingFolder + "/ticket-" + ticketId);
        }

        #region engine

        public delegate bool TicketPusher(string currentUserName, HttpContext context, int ticketId, string dbUserName = null, string dbPassword = null, string clientId =null);

        public delegate bool TicketRollback(HttpContext context, string currentName, int ticketId, string reason, string dbUserName = null, string dbPassword = null);

        public static bool PushToStage(string currentUserName, HttpContext context, int id, string dbUserName = null, string dbPassword = null, string clientId = null)
        {
            //TODO :Disabling the staging functions
            /*
            ILog tlog = GetLoggerForTicket(id);

            Process p = null;
            try {
                string dbServer;
                string dbName;
                string dbType;
                string commandArgs = BuildExtraArgs(id, out dbServer, out dbName, out dbType, forProd: false, userName: dbUserName, password: dbPassword);

                ExecutionInitiateLog elog = new ExecutionInitiateLog { Description = "Pushed to Stage", AuthorizedBy = currentUserName, StartTime = DateTime.Now, DBServer = dbServer, DBName = dbName };

                StateMachine.AddPushToStagingStartedState(id, logMessage:elog.ToJsonString(), authorizedBy: currentUserName, dbServer: dbServer, dbName:dbName);

                var stagingTargeExeLog = "staging-target-execution-" + DateTime.Now.Ticks + ".log";

                // find the staging connection information
                // TODO: switch to stored proc or prepared statement to  make it more efficient
                tlog.Info("[ticket: " + id + "] start push to staging execution.");

                p = Executor.ExecuteTargetScript(id, stagingTargeExeLog, commandArgs, dbType);
                var pushtoStageJsonLog = GetProcessInfoJsonLog( currentUserName, id, stagingTargeExeLog, p, dbServer, dbName, "Push to staging.");

                tlog.Info("[ticket: " + id + "] " + pushtoStageJsonLog);

                bool succeed = (p.ExitCode == 0);

                if (succeed) {
                    tlog.Info("[ticket: " + id + "] pushing to staging succeeded.");
                    StateMachine.AddPushToStagingSucceedState(id, pushtoStageJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName);
                    // wati for validation
                    StateMachine.AddStagingExecutionValidationWaitingState(id, "wait for validation on staging execution.", authorizedBy: currentUserName);

                    // release the lock
                    DaoUtil.ReleaseLock(id,  "Successfully pushed script to staging, release lock.");

                } else {
                    tlog.Info("[ticket: " + id + "] pushing to staging failed.");
                    StateMachine.AddPushToStagingFailedState(id, pushtoStageJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName);
                    StateMachine.AddPushToStagingRollbackStartedState(id, "authorized by : " + currentUserName, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);

                    StagingRollbackAction(currentUserName, id, RollbackReason.OnPushToStage);

                    //no matter what, terminate current workflow!
                    StateMachine.AddTerminateOnErrorState(id, "Workflow terminated on error.");

                    // release lock
                    DaoUtil.ReleaseLock(id,  "Failed to push script to staging, unlock after staging rollback attempt.");
                }

            } catch (Exception e) {
                //TODO:  handle various execution exception that may not caused by script itself, but by OS, such as insufficient memory, DNS issue, etc; needs more details on workflow impact  
                tlog.Error("[ticket: " + id + "] " + e.Message);
                throw;
            } finally {
                if (p != null) { p.Dispose(); }
            }*/
            return true;
        }

        public static void AsyncPushToStage(string currentUserName, HttpContext context, int id, string dbUserName = null, string dbPassword = null, string clientId = null)
        {
            //NOTE: we just take the shortcut now to use .NET thread pool for faster coding... and yes we are aware of its limitations, and will adjust that if perfmon evidence shows pool size issues
            TicketPusher stagingPusher = new TicketPusher(PushToStage);
            stagingPusher.BeginInvoke(currentUserName, context, id, dbUserName, dbPassword, null, null, clientId);
        }

        public static bool ValidateDBLogin(string currentUserName, int id, string dbUserName = null, string dbPassword = null)
        {
            Process p = null;
            string LoginTestSQL = Conf.WorkingFolder + "/ticket-" + id + "/UserValidate.sql";
            ILog tlog = GetLoggerForTicket(id);
            var validateUserLog = "user-validation" + ".log";
            try
            {
                string dbServer;
                string dbName;
                string dbType;

                //Logged useraccount
                string commandArgs = BuildExtraArgs(id, out dbServer, out dbName, out dbType, forProd: true, userName: dbUserName, password: dbPassword);
                tlog.Info(DateTime.Now + " [ticket: " + id + "] Check DB login for [" + currentUserName + "]");
                //Service account
                if (!File.Exists(LoginTestSQL))
                    File.Create(LoginTestSQL);

                ExecutionInitiateLog elog = new ExecutionInitiateLog { Description = "Pushed to Production", AuthorizedBy = currentUserName, StartTime = DateTime.Now, DBServer = dbServer, DBName = dbName };

                p = Executor.ExecuteSqlPlus(commandArgs, "UserValidate.sql", validateUserLog, workingDir: Conf.WorkingFolder + "/ticket-" + id);
                bool succeed = (p.ExitCode == 0);

                if (succeed && ExecutionSucceded(validateUserLog, id) == 1)
                    return true;
                else
                {
                    tlog.Info(DateTime.Now + " [ticket: " + id + "] DB login failed for [" + currentUserName + "]");
                    return false;
                }
            }
            catch (Exception ex)
            {
                tlog.Info(DateTime.Now + " [ticket: " + id + "]" + ex.Message);
                return false;
            }
            finally
            {
                if (p != null) p.Dispose();
                try
                {
                    File.Delete(validateUserLog);
                }
                catch (Exception) { }
            }
        }

        public static bool PushToProd(string currentUserName, HttpContext context, int id, string dbUserName = null, string dbPassword = null, string clientId = null)
        {
            ILog tlog = GetLoggerForTicket(id);
            Process p = null;
            try
            {
                string dbServer;
                string dbName;
                string dbType;
                string commandArgs = BuildExtraArgs(id, out dbServer, out dbName, out dbType, forProd: true, userName: dbUserName, password: dbPassword);

                ExecutionInitiateLog elog = new ExecutionInitiateLog { Description = "Pushed to Production", AuthorizedBy = currentUserName, StartTime = DateTime.Now, DBServer = dbServer, DBName = dbName };
                StateMachine.AddPushToProductionStartedState(id, logMessage: elog.ToJsonString(), authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);

                var prodTargetExeLog = "prod-target-execution-" + DateTime.Now.Ticks + ".log";

                // TODO: switch to stored proc or prepared statement to  make it more efficient
                tlog.Info(DateTime.Now + " [ticket: " + id + "] start push to prod execution.");

                p = Executor.ExecuteTargetScript(id, prodTargetExeLog, commandArgs, dbType, true,clientId);

                ManageProcesses.StoredNewProcess(p, context);
                ManageProcesses.UpdateTicketForProcess(id, p.Id, prodTargetExeLog);
                //CheckProcessStatus(currentUserName, id, prodTargetExeLog, p, dbServer, dbName,isRecursive:false);
                return true;
            }
            catch (Exception e)
            {
                //TODO:  handle various execution exception that may not caused by script itself, but by OS, such as insufficient memory, DNS issue, etc; needs more details on workflow impact  
                tlog.Info(DateTime.Now + "[ticket: " + id + "] " + e.Message);
                return false;
            }
        }

        public static int GetScriptExecutionStatus(HttpContext context, string currentUserName, int ticketId, bool forProd = true, bool isRecursive = false)
        {
            string dbName = string.Empty;
            string dbServer = string.Empty;
            string dbType = "oracle";
            string prodTargetExeLog = string.Empty;

            ManageProcesses.CachedProcesses = context;
            Process storedProc = ManageProcesses.GetProcessTicket(ticketId);
            if (storedProc == null)
            {
                string logMsg = "Lost the background execution process related with this ticket, so the ticket has been aborted";
                StateMachine.AddUserAbortState(ticketId, logMsg);
                DaoUtil.ReleaseLock(ticketId, logMsg + " and lock has been released");
                ManageProcesses.RemoveStoredProcess(ticketId, context);
                ManageProcesses.UpdateTicketForProcess(ticketId, null);
                ILog tlog = GetLoggerForTicket(ticketId);
                tlog.Info(DateTime.Now + " [ticket :" + ticketId + "] " + logMsg);
                return 2;
            }

            using (var db = new EntDbContext())
            {
                var txtDetail = (from t in db.Tickets where t.TicketId == ticketId select t).FirstOrDefault();
                dbType = txtDetail.Project.DatabaseType;
                if (forProd)
                {
                    dbServer = txtDetail.Project.ProductionHost;
                    dbName = txtDetail.Project.ProductionDatabase;
                    prodTargetExeLog = txtDetail.LogFilePath;
                }
            }
            return CheckProcessStatus(currentUserName, ticketId, prodTargetExeLog, storedProc, dbServer, dbName, isRecursive) ? 1 : 0;
        }

        public static bool CheckProcessStatus(string currentUserName, int id, string prodTargetExeLog, Process p, string dbServer, string dbName, bool isRecursive = true)
        {
            string logJsonMsg = string.Empty;
            ILog tlog = null;
            //= GetProcessInfoJsonLog(currentUserName, id, prodTargetExeLog, p, dbServer, dbName);
            //tlog.Info(DateTime.Now + " [ticket: " + id + "] " + logJsonMsg);

            int statusCode = ExecutionSucceded(prodTargetExeLog, id);
            switch (statusCode)
            {
                case 0: tlog = GetLoggerForTicket(id);
                    logJsonMsg = GetProcessInfoJsonLog(currentUserName, id, prodTargetExeLog, p, dbServer, dbName, endtime: DateTime.Now);
                    tlog.Info(DateTime.Now + " [ticket: " + id + "] pushed into production with some error(s) ");
                    StateMachine.AddPushToProductionCompletedState(id, logJsonMsg, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);
                    break;
                case 1: tlog = GetLoggerForTicket(id);
                    logJsonMsg = GetProcessInfoJsonLog(currentUserName, id, prodTargetExeLog, p, dbServer, dbName, endtime: DateTime.Now);
                    tlog.Info(DateTime.Now + " [ticket: " + id + "] pushed into production succeefully ");
                    StateMachine.AddPushToProductionSucceedState(id, logJsonMsg, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);
                    break;
                default:
                    if (isRecursive)
                    {
                        using (var db = new EntDbContext())
                        {
                            int? tktStatus = db.Tickets.Where(tkt => tkt.TicketId == id).Select(t => t.ProcessId).FirstOrDefault();
                            if (tktStatus == null)
                            {
                                if (p != null) p.Dispose();
                                return false;
                            }
                        }
                        CheckProcessStatus(currentUserName, id, prodTargetExeLog, p, dbServer, dbName, isRecursive);
                    }
                    break;
            }

            if (statusCode < 2)
            {
                //NOTE: we do not want to terminate the state until we have the confirmation from approver.
                StateMachine.AddProdExecutionValidationWaitingState(id, "wait for approver's manual validation.");
                DaoUtil.ReleaseLock(id, "Successfully pushed script to production, release production locking.");
                return true;
            }
            return false;
            //bool succeed = (p.ExitCode == 0);
            //if (!succeed)
            //{
            //    tlog.Info(DateTime.Now + " [ticket: " + id + "] pushing to prod failed");
            //    StateMachine.AddPushToProductionFailedState(id, logJsonMsg, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);
            //    // we have to roll back changes 
            //    StateMachine.AddPushToProductionRollbackStartedState(id, "authorized by : " + currentUserName, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);

            //    //rollback both prod and staging
            //    var prodRollbackSucceeded = ProdRollbackAction(currentUserName, id, RollbackReason.OnPushToProd);
            //    StagingRollbackAction(currentUserName, id, RollbackReason.OnPushToProd);

            //    // roll back production locking after both production & staging rollback 
            //    DaoUtil.ReleaseLock(id, "Failed to push script to prodution, release production environment exeuction locks after prod and staging rollback attempt.");
            //    tlog.Info(DateTime.Now + " [ticket: " + id + "] release produciton locking after prod & staging rollback.");

            //    //no matter what, terminate current workflow!
            //    StateMachine.AddTerminateOnErrorState(id, "Workflow terminated on error.");
            //}
        }

        /// <summary>
        /// 0 -- False
        /// 1 -- Succeed
        /// 2 -- Exception
        /// </summary>
        /// <param name="logFile"></param>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        public static int ExecutionSucceded(string logFile, int ticketId)
        {
            try
            {
                string logDetails = File.ReadAllText(Conf.WorkingFolder + "ticket-" + ticketId.ToString() + @"/" + logFile);
                if (logDetails.ToLower().IndexOf("error") > 0)
                    return 0;

                return 1;
            }
            catch (Exception)
            {
                return 2;
            }
        }

        public static void AsyncPushToProd(string currentUserName, HttpContext context, int id, string dbUserName = null, string dbPassword = null, string clientId = null)
        {
            //NOTE: we just take the shortcut now to use .NET thread pool for faster coding... and yes we are aware of its limitations, and will adjust that if perfmon evidence shows pool size issues
            TicketPusher stagingPusher = new TicketPusher(PushToProd);
            IAsyncResult result = stagingPusher.BeginInvoke(currentUserName, context, id, dbUserName, dbPassword,clientId, null, null);
            while (result.IsCompleted != true)
            {
                //Call CheckStatus method
            }
            bool returnValue = stagingPusher.EndInvoke(result);
            if (returnValue)
                GetScriptExecutionStatus(context, currentUserName, id);
        }

        #endregion

        #region logger
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

        #region build command args
        private static string BuildExtraArgs(int ticktId, out string dbServer, out string dbName, out string dbType, bool forProd = false, string userName = null, string password = null)
        {
            string sqlAuthentication = string.Empty;
            if (!string.IsNullOrEmpty(userName) & !string.IsNullOrEmpty(password))
            {
                sqlAuthentication = string.Format(" -U {0} -P {1}", userName.Trim(), password.Trim());
            }

            using (var db = new EntDbContext())
            {
                var project = (from t in db.Tickets where t.TicketId == ticktId select t.Project).FirstOrDefault();
                dbType = project.DatabaseType;
                string loginPattern = project.Description;
                //TODO: Utilize Project description for Login pattern
                if (forProd)
                {
                    dbServer = project.ProductionHost;
                    dbName = project.ProductionDatabase;
                    if (dbType.ToLower() == "sql server")
                        return "-S " + project.ProductionHost + BuildPortArg(project.ProductionPort)
                                + " -d  " + project.ProductionDatabase + sqlAuthentication;
                    else
                        //return string.Format("{0}/{1}@//{2}:{3}/{4}", "dbcrdeploy", "dbcrdeploy", project.ProductionHost, project.ProductionPort, project.ProductionDatabase);
                        return string.Format(loginPattern, userName.Trim(), password.Trim(), project.ProductionHost, project.ProductionPort, project.ProductionDatabase);
                }
                else
                {
                    dbServer = project.StagingHost;
                    dbName = project.StagingDatabase;
                    if (dbType.ToLower() == "sql server")
                        return "-S " + project.StagingHost + BuildPortArg(project.StagingPort)
                            + " -d  " + project.StagingDatabase + sqlAuthentication;
                    else
                        //return string.Format("{0}/{1}@//{2}:{3}/{4}", "dbcrdeploy", "dbcrdeploy", project.StagingHost, project.StagingPort, project.StagingDatabase);
                        return string.Format(loginPattern, userName.Trim(), password.Trim(), project.StagingHost, project.StagingPort, project.StagingDatabase);
                }
            }
        }

        private static string BuildPortArg(int? port)
        {
            return (port.HasValue && port.Value != 1433)
                       ? "," + port.Value
                       : " ";
        }
        #endregion

        public static string GetProcessInfoJsonLog(string currentUserName, int id, string logName, Process p, string dbServer, string dbName, string description = null, DateTime? endtime = null)
        {
            var fullLogPath = new StringBuilder().Append(Conf.WorkingFolder).Append("/ticket-").Append(id).Append("/").Append(logName).ToString();
            var fileSize = new FileInfo(fullLogPath).Length;
            ProcessInfoLog plog = null;
            if (endtime != null)
                plog = new ProcessInfoLog { AuthorizedBy = currentUserName, LogFile = logName, LogFileSize = fileSize, ProcessId = p.Id, TotalProcessorTime = p.TotalProcessorTime, StartTime = p.StartTime, EndTime = endtime, Description = description, DBServer = dbServer, DBName = dbName };
            else
                plog = new ProcessInfoLog { AuthorizedBy = currentUserName, LogFile = logName, LogFileSize = fileSize, ProcessId = p.Id, TotalProcessorTime = p.TotalProcessorTime, StartTime = p.StartTime, Description = description, DBServer = dbServer, DBName = dbName };

            return plog.ToJsonString();
        }

        //TODO: Disabling the Staging functionality as per Calheers project requirements
        public static bool StagingRollbackAction(HttpContext context, string currentUserName, int id, string reason, string dbUserName = null, string dbPassword = null)
        {
            /*
            var stagingRollbackExeLog = "staging-rollback-execution-" + DateTime.Now.Ticks + ".log";

            string dbServer;
            string dbName;
            string dbType;
            string commandArgs = BuildExtraArgs(id, out dbServer, out dbName, out dbType, forProd: false);

            using (Process p = Executor.ExecuteRollbackScript(id, stagingRollbackExeLog, commandArgs)) {
                var stagingRollbackJsonLog = GetProcessInfoJsonLog(currentUserName, id, stagingRollbackExeLog, p, dbServer, dbName, reason);
                var rollbackSucceeed = (p.ExitCode == 0);
                switch (reason) {
                    case RollbackReason.OnPushToStage:
                        if (rollbackSucceeed) {
                            StateMachine.AddPushToStagingRollbackSucceedState(id, stagingRollbackJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName);
                        } else {
                            StateMachine.AddPushToStagingRollbackFailedState(id, stagingRollbackJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName);
                        }
                        break;
                    case RollbackReason.OnStageRejection:
                        if (rollbackSucceeed) {
                            StateMachine.AddStagingRollbackOnRejectionSucceededState(id, stagingRollbackJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName );
                        } else {
                            StateMachine.AddStagingRollbackOnRejectionFailedState(id, stagingRollbackJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName );
                        }
                        break;
                    case RollbackReason.OnPushToProd:
                        if (rollbackSucceeed) {
                            StateMachine.AddPushToProductionStagingRollbackSucceedState(id, stagingRollbackJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName);
                        } else {
                            StateMachine.AddPushToProductionStagingRollbackFailedState(id, stagingRollbackJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName);
                        }
                        break;
                    case RollbackReason.OnProdRejection:
                        if (rollbackSucceeed) {
                            StateMachine.AddStagingRollbackOnProdRejectionSucceededState(id, stagingRollbackJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName);
                        } else {
                            StateMachine.AddStagingRollbackOnProdRejectionFailedState(id, stagingRollbackJsonLog, authorizedBy: currentUserName, dbServer:dbServer, dbName:dbName);
                        }
                        break;
                    default:
                        throw new NotSupportedException("not support this option:" + reason);
                }
                return rollbackSucceeed;
            }*/
            return false;
        }

        /// <summary>
        /// Rollback staging execution in async way -- there's no easy way to return value in async call without help of async results, callback method, and possible blocking, so this emthod return type is void.
        /// </summary>
        /// <param name="currentUserName"></param>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        public static void AsyncStagingRollbackAction(HttpContext context, string currentUserName, int id, string reason, AsyncCallback callback = null, Object state = null, string dbUserName = null, string dbPassword = null)
        {
            TicketRollback stagingRollbacker = new TicketRollback(StagingRollbackAction);
            stagingRollbacker.BeginInvoke(context, currentUserName, id, reason, dbUserName, dbPassword, callback, state);
        }

        public static bool ProdRollbackAction(HttpContext context, string currentUserName, int id, string reason, string dbUserName = null, string dbPassword = null)
        {
            var prodRollbackExeLog = "production-rollback-execution-" + DateTime.Now.Ticks + ".log";

            string dbServer;
            string dbName;
            string dbType;
            string commandArgs = BuildExtraArgs(id, out dbServer, out dbName, out dbType, forProd: true, userName: dbUserName, password: dbPassword);

            ILog tlog = GetLoggerForTicket(id);
            Process p = null;
            try
            {
                if (dbType.ToLower() == "sql server")
                {
                    p = Executor.ExecuteRollbackScript(id, prodRollbackExeLog, commandArgs, dbType);
                }
                else
                {
                    //Get the instance of stored SQLPLUS process and execute rollback script
                    ManageProcesses.CachedProcesses = context;
                    p = ManageProcesses.GetProcessTicket(id);
                    p = Executor.ExecuteSqlPlus(p, "rollback.sql", prodRollbackExeLog, workingDir: Conf.WorkingFolder + "/ticket-" + id);
                }

                bool prodRollbackSucceeded = (p.ExitCode == 0);
                if (prodRollbackSucceeded) tlog.Info(DateTime.Now + " [ticket: " + id + "] rollback script has been executed sucessfully.");
                var prodRollbackJsonLog = GetProcessInfoJsonLog(currentUserName, id, prodRollbackExeLog, p, dbServer, dbName, reason);
                switch (reason)
                {
                    case RollbackReason.OnPushToProd:
                        if (prodRollbackSucceeded)
                        {
                            StateMachine.AddPushToProductionRollbackSucceedState(id, prodRollbackJsonLog, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);
                        }
                        else
                        {
                            StateMachine.AddPushToProductionRollbackFailedState(id, prodRollbackJsonLog, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);
                        }
                        break;
                    case RollbackReason.OnProdRejection:
                        if (prodRollbackSucceeded)
                        {
                            StateMachine.AddProdRollbackOnProdRejectionSucceededState(id, prodRollbackJsonLog, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);
                        }
                        else
                        {
                            StateMachine.AddProdRollbackOnProdRejectionFailedState(id, prodRollbackJsonLog, authorizedBy: currentUserName, dbServer: dbServer, dbName: dbName);
                        }
                        break;
                    default:
                        throw new NotSupportedException("not support this option:" + reason);
                }
                return prodRollbackSucceeded;
            }
            catch (Exception ex)
            {
                tlog.Info(DateTime.Now + "[ticket: " + id + "]_RollbackAction Error: " + ex.Message);
                //Dispose the process i.e. (SQLCMD or SQLPLUS)
                if (p != null)
                {
                    if (dbType.ToLower() != "sql server")
                    {
                        ManageProcesses.RemoveStoredProcess(p.Id, context);
                        ManageProcesses.UpdateTicketForProcess(id, null);
                    }
                    p.Dispose();
                }
                return false;
            }
        }

        public static void AsyncProdRollbackAction(HttpContext context, string currentUserName, int id, string reason, string dbUserName = null, string dbPassword = null, AsyncCallback callback = null, Object state = null)
        {
            TicketRollback prodRollback = new TicketRollback(ProdRollbackAction);
            prodRollback.BeginInvoke(context, currentUserName, id, reason, dbUserName, dbPassword, callback, state);
        }

        public static void TerminateWorkFlowOnErrorCallBack(IAsyncResult result)
        {
            //NOTE: it is not that necessary for use to know the returning results, we just need to handle the exceptions in a proper way. 
            try
            {
                AsyncResult ar = (AsyncResult)result;
                // in no way you should call this one already.... if so, means you already handled eveyrthing in blocking way.... 
                Debug.Assert(!ar.EndInvokeCalled);
                bool ok = ((TicketRollback)ar.AsyncDelegate).EndInvoke(result);

                Dictionary<string, object> d = (Dictionary<string, object>)ar.AsyncState;
                Object[] stateArr = ar.AsyncState as Object[];

                int ticketId = (int)d["TicketId"];

                // we may need to release lock here.
                bool releaselock = (bool)d["releaselock"];
                if (releaselock)
                {
                    DaoUtil.ReleaseLock(ticketId, (string)d["unlockreason"]);
                }

                string message = (string)d["LogMessage"];
                StateMachine.AddTerminateOnErrorState(ticketId, message);
            }
            catch (Exception e)
            {
                //TODO: handle exceptions properly
                throw;
            }
            finally
            {
                //NOTE: check if there's any unmanaged source that needs disposing here... now, it seems we don't have such resource
            }
        }

        //NOTE: it is really hard to control the sequence of production and staging exeuction sequence, hence is is hard to predict which one may finish last and then terminate the workflow, so it is easier to introduce a sync wrpper call here.
        public static bool ProdAndStagingRollbackAction(HttpContext context, string currentUserName, int id, string reason, string dbUserName = null, string dbPassword = null)
        {
            bool ok = ProdRollbackAction(context, currentUserName, id, reason, dbUserName, dbPassword);
            //TODO: Is Staging functionality required?, Commenting below line until final decision
            //ok = StagingRollbackAction(context, currentUserName, id, reason) && ok ;
            return ok;
        }

        public static void AsyncProdAndStagingRollbackAction(HttpContext context, string currentUserName, int id, string reason, string dbUserName = null, string dbPassword = null, AsyncCallback callback = null, Object state = null)
        {
            TicketRollback rollbackBoth = new TicketRollback(ProdAndStagingRollbackAction);
            rollbackBoth.BeginInvoke(context, currentUserName, id, reason, dbUserName, dbPassword, callback, state);
        }

        public static bool CommitChanges(HttpContext context, int id, string dbUserName = null, string dbPassword = null)
        {
            ILog tlog = GetLoggerForTicket(id);
            Process storedProc = null;
            string dbServer;
            string dbName;
            string dbType = string.Empty;
            try
            {
                string commandArgs = BuildExtraArgs(id, out dbServer, out dbName, out dbType, forProd: true, userName: Conf.OracleServiceAccountName, password: Conf.OracleServiceAccountPassword);

                //ToDo:Need to check same case for SQL Server as doing for Oracle
                if (dbType.ToLower() == "sql server") return true;

                //Get the instance of stored SQLPLUS process and execute commit command/standard script
                ManageProcesses.CachedProcesses = context;
                storedProc = ManageProcesses.GetProcessTicket(id);
                if (storedProc == null)
                {
                    tlog.Info(DateTime.Now + "[ticket: " + id + "]_CommitChanges Error: Lost the background execution process related with this ticket, so the ticket has been aborted");
                    return false;
                }
                storedProc = Executor.ExecuteSqlPlus(storedProc, Conf.CommitScript);
                bool succeed = (storedProc.ExitCode == 0);
                return succeed;
            }
            catch (Exception ex)
            {
                tlog.Info(DateTime.Now + "[ticket: " + id + "]_CommitChanges Error: " + ex.Message);
                return false;
            }
            finally
            {
                //Dispose the process associated with the ticket
                if (storedProc != null)
                {
                    if (dbType.ToLower() != "sql server")
                    {
                        ManageProcesses.RemoveStoredProcess(storedProc.Id, context);
                        ManageProcesses.UpdateTicketForProcess(id, null);
                    }
                    storedProc.Dispose();
                }
            }
        }

        public static void CheckRunningProcessStatus(HttpContext context)
        {
            try
            {
                using (var db = new EntDbContext())
                {
                    //var workState = (from ws in db.Workstates.Last()
                    //                 join t in db.Tickets on ws.TicketId equals t.TicketId
                    //                 where t.ProcessId != null && t.WorkStates.Last().Code == FlowState.ProductExecutionStarted.Code
                    //                 select ws);
                    var waitingTickets = db.Tickets.Where(tkt => tkt.WorkStates.OrderByDescending(ws => ws.WorkStateId).FirstOrDefault().Code == FlowState.ProductExecutionStarted.Code && tkt.ProcessId != null).ToList();
                    foreach (Ticket ticket in waitingTickets)
                    {
                        WorkState ws = ticket.WorkStates.Last();
                        if (ws.Code == FlowState.ProductExecutionStarted.Code)
                            GetScriptExecutionStatus(context, ws.AuthorizedBy, ws.TicketId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ProcessStatusCheck Error: " + ex.Message);
            }
        }
    }
}