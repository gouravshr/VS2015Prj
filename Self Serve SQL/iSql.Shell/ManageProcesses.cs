using iSql.Commons.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace iSql.Shell
{
    public class ManageProcesses
    {
        public static Process GetStoredProcess(int processId)
        {
            if (StoredProcesses == null) return null;
            List<Process> processList = StoredProcesses as List<Process>;
            return processList.Where(p => p.Id == processId).FirstOrDefault();
        }

        public static bool RemoveStoredProcess(int processId, HttpContext context)
        {
            CachedProcesses = context;
            if (StoredProcesses == null) return false;
            bool status = false;
            List<Process> processList = StoredProcesses as List<Process>;
            Process process = processList.Where(proc => proc.Id == processId).FirstOrDefault();
            if (process != null)
            {
                status = processList.Remove(process);
                context.Cache["StoredProcesses"] = processList;
                CachedProcesses = context;
            }
            return status;
        }

        public static bool StoredNewProcess(Process newProcess, HttpContext context)
        {
            Process p = null;
            CachedProcesses = context;
            if (StoredProcesses != null)
            {
                List<Process> processList = StoredProcesses as List<Process>;
                p = processList.Where(proc => proc.Id == newProcess.Id).FirstOrDefault();
            }
            
            if (p == null)
            {
                StoredProcesses = newProcess;
                return true;
            }
            return false;
        }

        public static Process GetProcessTicket(int ticketId)
        {
            int processId = GetProcessByTicket(ticketId);
            if (processId > 0)
                return GetStoredProcess(processId);

            return null;
        }

        public static bool UpdateTicketForProcess(int ticketId, int? ProcessId, string logFile = null)
        {
            //TODO: discard EF and go with pure stored proc for efficiency
            using (var db = new EntDbContext())
            {
                Ticket txt = (from tkt in db.Tickets
                              where tkt.TicketId == ticketId
                              select tkt).FirstOrDefault();
                txt.ProcessId = ProcessId;
                if (!string.IsNullOrEmpty(logFile))
                    txt.LogFilePath = logFile;
                db.SaveChanges();
            }
            //TODO: what if SQL exception thrown during state saving? Do we just return false or directly throw exception? Should we handle it here or what?  
            return true;
        }

        public static int GetProcessByTicket(int ticketId)
        {
            //TODO: discard EF and go with pure stored proc for efficiency
            int? processId;
            using (var db = new EntDbContext())
            {
                processId = (from tkt in db.Tickets
                             where tkt.TicketId == ticketId
                             select tkt.ProcessId).FirstOrDefault();
            }
            //TODO: what if SQL exception thrown during state saving? Do we just return false or directly throw exception? Should we handle it here or what?  
            if (processId != null)
                return Convert.ToInt32(processId);
            else
                return 0;
        }

        public static HttpContext CachedProcesses
        {
            get;
            set;
        }

        private static object StoredProcesses
        {
            get
            {
                if (CachedProcesses !=null && CachedProcesses.Cache["StoredProcesses"] != null)
                    return CachedProcesses.Cache["StoredProcesses"];
                else
                    return null;
            }
            set
            {
                List<Process> procLst = null;
                if (CachedProcesses.Cache["StoredProcesses"] != null)
                    procLst = CachedProcesses.Cache["StoredProcesses"] as List<Process>;
                else
                    procLst = new List<Process>();

                procLst.Add(value as Process);
                CachedProcesses.Cache["StoredProcesses"] = procLst;
            }
        }
    }
}