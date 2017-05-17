using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace iSql.Commons {
    
    public class DaoUtil
    {

        /// <summary>
        /// General stored proc helper method, exception will be thrown as it is if any, but sql connection should be safely closed.
        /// </summary>
        /// <param name="spName">Stored Proc name.</param>
        /// <param name="parameters">SqlParameter array to provide best cotnrol to developers, who can always create wrapper method to simply the calling.</param>
        /// <returns></returns>
        public static DataTable ExecStoredProc(string spName, SqlParameter[] parameters) {

            /*
            DataTable dt = new DataTable();
            var connStr = ConfigurationManager.ConnectionStrings["EntDbContext"].ConnectionString;
            using( var conn = new SqlConnection( connStr)) {
                using(var cmd = conn.CreateCommand()) {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = spName;
                    cmd.Parameters.AddRange(parameters);
                    using (  SqlDataAdapter da = new SqlDataAdapter(cmd) ) {
                        da.Fill(dt);
                    }
                }                                
            }
            return dt;
            */
            return ExecQuery(CommandType.StoredProcedure, spName, parameters);
        }

        public static  DataTable ExecClearTextQuery( string queryString , SqlParameter[] parameters ) {
            return ExecQuery(CommandType.Text, queryString, parameters); 
        }

        public static DataTable ExecQuery( CommandType queryType, string query, SqlParameter[] parameters) {
             DataTable dt = new DataTable();
            var connStr = ConfigurationManager.ConnectionStrings["EntDbContext"].ConnectionString;
            using( var conn = new SqlConnection( connStr)) {
                using(var cmd = conn.CreateCommand()) {
                    cmd.CommandType = queryType;
                    cmd.CommandText = query;
                    if ( parameters != null )  { cmd.Parameters.AddRange(parameters);}
                    using (  SqlDataAdapter da = new SqlDataAdapter(cmd) ) {
                        da.Fill(dt);
                    }
                }                                
            }
            return dt;
        }

        public static DataSet ExecQueryMulti(CommandType queryType, string query, SqlParameter[] parameters)
        {
            DataSet ds = new DataSet();
            var connStr = ConfigurationManager.ConnectionStrings["EntDbContext"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = queryType;
                    cmd.CommandText = query;
                    if (parameters != null) { cmd.Parameters.AddRange(parameters); }
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }
            }
            return ds;
        }
       
        /// <summary>
        /// Get lock for certain project -- if everything works fine, no error code or error message returned, otherwise, user will see error code/message and existing locks held by other process.
        /// </summary>
        /// <param name="ticketId">Ticket ID</param>
        /// <param name="stagingOrProdStage">Must be either Staging or Production</param>
        /// <param name="LockReason">Describe the purpose of this locking</param>
        /// <param name="lockedByUser">user who requested this lock</param>
        /// <returns></returns>
        public static DataTable GetLock( int ticketId, string stagingOrProdStage, string lockedByUser, string lockReason ) {
            var dt = ExecStoredProc("GetLock", new SqlParameter[] {
                                           new SqlParameter("TicketId", ticketId),
                                           new SqlParameter("ServerCategory", stagingOrProdStage),
                                           new SqlParameter("LockedBy", lockedByUser),
                                           new SqlParameter("LockReason", lockReason),
                                   });
            //NOTE: why bother? because when we try to xml serialze DataTable without a TableName, exception will throw.... ridiculous 
            dt.TableName = "lockinginfo"; 
            return dt;
        }

        //TODO: not very solid sql, we know, but we also know it is a small table and we will enhance it in the future
        public const string AllLocksSql = "SELECT l.*,t.ProcessId from ProcessLocks l INNER JOIN Tickets t ON t.TicketId = l.TicketId where 1=1 ";
        public static DataTable GetAllLocks() {
            return ExecClearTextQuery(AllLocksSql, null);
        }

        public const string SingleLockSql = "SELECT * from ProcessLocks where LockId=@LockId ";
        public static DataTable GetSingleLock(int lockId)
        {
            return ExecQuery(CommandType.Text, SingleLockSql, new SqlParameter[]{new SqlParameter("LockId", lockId)});
        } 

        public static DataTable ReleaseLock( int ticketId, string unlockReason = null ) {
            var dt = ExecStoredProc("ReleaseLock", new SqlParameter[]
                                                      {
                                                          new SqlParameter("TicketId", ticketId),
                                                         // new SqlParameter("ServerCategory", stagingOrProdState),
                                                          new SqlParameter("UnlockReason", unlockReason)
                                                      });
            dt.TableName = "unlockinfo";

            return dt; 
        }
    }
}