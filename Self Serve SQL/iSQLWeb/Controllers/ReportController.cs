using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iSql.Commons;

namespace iSQLWeb.Controllers {
    [Authorize(Roles = "Admin")]
    public class ReportController : BaseController {

        public ActionResult Index() {
            // we open all projects to admin
            var projects = (from p in db.Projects select p).ToList();
            ViewBag.ProjectId = new SelectList(projects, "ProjectId", "Name");

            return View();
        }

        private const string GetProjectUsersSql = @"select [UserId], [Role] from [Accesses] where [ProjectId]=@ProjectId order by [Role]";
        public ActionResult ListOfUsersForProject() {
            int projectId;
            if( ! int.TryParse(Request.Params["ProjectId"], out projectId) ) {
                return Content("Bad project id.");
            }

            DataTable dt = DaoUtil.ExecClearTextQuery(GetProjectUsersSql, new SqlParameter[] { new SqlParameter("ProjectId", projectId), });
            return RenderCsv(dt, "project_user_list_");
        }

        private const string GetProjectTicketsSql = @"
                select t.[TicketId], t.[Description], t.[UserId] as [Creator], t.[CreatedAt], t.[ItgNumber] , p.[Name] as [Project]
                from [Tickets] t
                join [Projects] p
                on t.[ProjectId] = p.[ProjectId] 
                where t.[ProjectId] = @ProjectId 
                and t.[CreatedAt] >= @StartDate
                and t.[CreatedAt] <= @EndDate 
                ";

        public ActionResult ListOfTicketsForProject() {
            int projectId;
            if( ! int.TryParse(Request.Params["ProjectId"], out projectId) ) {
                return Content("Bad project id.");
            }

            DateTime startDate;
            DateTime endDate; 
            if( ! DateTime.TryParse(Request.Params["startDate"], out startDate) ) {
               startDate = new DateTime(1753,1,1, 12, 0, 0); 
            }

            if( ! DateTime.TryParse(Request.Params["endDate"], out endDate) ) {
               endDate = new DateTime(9999,12,31, 11, 59, 59); 
            }


            DataTable dt = DaoUtil.ExecClearTextQuery(GetProjectTicketsSql, 
                                                      new SqlParameter[] {
                                                          new SqlParameter("ProjectId", projectId),
                                                          new SqlParameter("StartDate", startDate),
                                                          new SqlParameter("EndDate", endDate),
                                                      });

            return RenderCsv(dt, "project_ticket_list_");
        }

        private const string GetQueryStatsByServerSql = @"
                SELECT w.ticketid     AS [Ticket ID],
                       p.name         AS [Project],
                       t.itgnumber    AS [ITG Number],
                       w.name         AS [State Description],
                       w.createdat    AS [Execution Finished Time],
                       w.authorizedby AS [Authorized By],
                       w.dbserver     AS [DB Server],
                       w.dbname       AS [DB Name]
                FROM   workstates w
                       JOIN tickets t
                         ON w.ticketid = t.ticketid
                            AND w.createdat >= @StartDate 
                            AND w.createdat <= @EndDate 
                            AND w.dbserver = @DbServer 
                            AND w.category <> 'Transient'
                       JOIN projects p
                         ON t.projectid = p.projectid  
                ";

        public ActionResult ListOfQueriesForDatabaseServer() {

            string dbServer = Request.Params["dbServer"];
            if( String.IsNullOrWhiteSpace( dbServer)) {
                return Content("Bad database server name or IP.");
            }

            DateTime startDate;
            DateTime endDate; 
            if( ! DateTime.TryParse(Request.Params["startDate"], out startDate) ) {
               startDate = new DateTime(1753,1,1, 12, 0, 0); 
            }

            if( ! DateTime.TryParse(Request.Params["endDate"], out endDate) ) {
               endDate = new DateTime(9999,12,31, 11, 59, 59); 
            }

            //TODO: change TIME information for start/end dates, not just date 

            DataTable dt = DaoUtil.ExecClearTextQuery(GetQueryStatsByServerSql, 
                                                      new SqlParameter[] {
                                                          new SqlParameter("DbServer", dbServer),
                                                          new SqlParameter("StartDate", startDate),
                                                          new SqlParameter("EndDate", endDate),
                                                      });

            return RenderCsv(dt, "queries_for_db_");
        }

        #region Management Reports        
        #region Statistics Summary        
        private const string UsageSummarySQL = @"
            DECLARE @from_lsn binary(10), @to_lsn binary(10)
            SET @from_lsn = sys.fn_cdc_get_min_lsn('dbo_Projects')
            SET @to_lsn   = sys.fn_cdc_get_max_lsn();

            WITH Transaction_CTE AS
            (
            SELECT	__$start_lsn [Transaction]
		            , __$seqval [Sequence]
		            , sys.fn_cdc_map_lsn_to_time (__$start_lsn) [Transaction Time]
		            , CASE __$operation
			            WHEN 1 THEN 'Delete'
			            WHEN 2 THEN 'Insert'
			            WHEN 3 THEN 'Pre-Update'
			            WHEN 4 THEN 'Post-Update'
		            END [Operation]
		            , sys.fn_sqlvarbasetostr(__$update_mask) [Column Mask] 
		            , *
            FROM	cdc.fn_cdc_get_all_changes_dbo_Projects(
			            @from_lsn,
			            @to_lsn,
			            N'all update old')
            WHERE	__$operation = 2
            )

            SELECT	YEAR([Transaction Time]) AS [Year]
		            ,MONTH([Transaction Time]) AS [Month]
		            ,COUNT(1) as [New Projects]
            FROM	Transaction_CTE		
            GROUP BY	YEAR([Transaction Time])
			            ,MONTH([Transaction Time])


            SELECT	'Tickets Uploaded' AS [State]
		            ,YEAR(CreatedAt) AS [Year]
		            ,MONTH(CreatedAt) AS [Month]
		            ,COUNT(1) AS [Count]
            FROM	WorkStates
            WHERE	Code = 10102
            GROUP BY
		            YEAR(CreatedAt)
		            ,MONTH(CreatedAt)
            UNION ALL
            SELECT	'Pushed To Stage'
		            ,YEAR(CreatedAt)
		            ,MONTH(CreatedAt)
		            ,COUNT(1)
            FROM	WorkStates
            WHERE	Code = 30101
            GROUP BY
		            YEAR(CreatedAt)
		            ,MONTH(CreatedAt)
            UNION ALL
            SELECT	'Pushed To Prod'
		            ,YEAR(CreatedAt)
		            ,MONTH(CreatedAt)
		            ,COUNT(1)
            FROM	WorkStates
            WHERE	Code = 50101
            GROUP BY
		            YEAR(CreatedAt)
		            ,MONTH(CreatedAt)

            SELECT	p.Name [Project]
		            ,YEAR(w.CreatedAt) [Year]
		            ,MONTH(w.CreatedAt) [Month]
		            ,COUNT(1) [SQL Execution]
            FROM	Projects p
            INNER JOIN Tickets t
	            ON	p.ProjectId = t.ProjectId
            INNER JOIN WorkStates w
	            ON	t.TicketId = w.TicketId
            WHERE	w.Code IN (30101, 50101)
            GROUP BY p.Name 
		            ,YEAR(w.CreatedAt) 
		            ,MONTH(w.CreatedAt) 
            ORDER BY 1, 2, 3;

            SET	DATEFIRST 1
            DECLARE		@MinDate	DATETIME
            SELECT	@MinDate = MIN(CreatedAt) FROM WorkStates;

            WITH CTE_LAST_YEAR AS (
	            SELECT	1 AS DayID
			            ,GETDATE() AS [Date]
			            ,DATEPART(yyyy, GETDATE()) AS [Year]
			            ,DATEPART(wk, GETDATE()) AS [Week]
	            UNION ALL
	            SELECT	CTE_LAST_YEAR.DayID + 1 AS DayID
			            ,DATEADD(d, -1, CTE_LAST_YEAR.[Date]) AS [Date]
			            ,DATEPART(yyyy, DATEADD(d, -1, CTE_LAST_YEAR.[Date])) AS [Year]
			            ,DATEPART(wk, DATEADD(d, -1, CTE_LAST_YEAR.[Date])) AS [Week]
	            FROM	CTE_LAST_YEAR
	            WHERE	DATEADD(d, -1, CTE_LAST_YEAR.[Date]) > DATEADD(d, -370, GETDATE())
	              AND	DATEADD(d, -1, CTE_LAST_YEAR.[Date]) > DATEADD(d, -4, @MinDate)	  
            ),
             CTE_WEEK_DATE AS (
	            SELECT	[Year]
			            ,[Week]
			            ,MIN([Date]) AS [Date]
			            ,CAST(DATEPART(yyyy, MIN([Date])) AS CHAR(4)) + '-' + RIGHT('00' + RTRIM(CAST(DATEPART(m, MIN([Date])) AS NCHAR(2))), 2) + '-' + RIGHT('00' + RTRIM(CAST(DATEPART(d, MIN([Date])) AS NCHAR(2))), 2) AS [ShortDate]
			            ,DATENAME(dd, MIN([Date])) + ' ' + DATENAME(m, MIN([Date])) + ' ' + DATENAME(yyyy, MIN([Date])) AS [LongDate]
	            FROM	CTE_LAST_YEAR
	            GROUP BY [Year]
			            , [Week]
            ),
            CTE_HOURS AS (
	            SELECT	DATEPART(yyyy, CreatedAt) [Year]
			            , DATEPART(ww, CreatedAt) [Week]		
			            , COUNT(1) [Hours Saved]
			            , COUNT(1) * 35 [Cost Saved]
	            FROM	WorkStates
	            WHERE	Code IN (30101,50101)
	              AND	(DATEPART(yyyy, CreatedAt) < DATEPART(yyyy, GETDATE())
		            OR	 DATEPART(ww, CreatedAt) < DATEPART(ww, GETDATE()))
	            GROUP BY DATEPART(yyyy, CreatedAt)
			            , DATEPART(ww, CreatedAt)
            )

            SELECT		wd.ShortDate AS [Week]
			            ,ISNULL(h.[Hours Saved], 0) AS [Hours Saved]
			            ,ISNULL(h.[Cost Saved], 0) AS [Cost Saved]
            FROM	CTE_WEEK_DATE wd
            LEFT OUTER JOIN	CTE_HOURS h
	            ON	wd.[Year] = h.[Year]
	            AND	wd.[Week] = h.[Week]
            WHERE	wd.[Year] < DATEPART(yyyy, GETDATE())
               OR	wd.[Week] < DATEPART(ww, GETDATE())
            ORDER BY	wd.[Date]
            OPTION (MaxRecursion 400)

		";
        public ActionResult UsageSummary()
        {
            DataSet ds = DaoUtil.ExecQueryMulti(CommandType.Text, UsageSummarySQL, null);
            ds.Tables[0].TableName = "Projects Rolled-On";
            ds.Tables[1].TableName = "Ticket Count By Stage";
            ds.Tables[2].TableName = "Ticket Count By Project";
            ds.Tables[3].TableName = "Cost Saving By Week";
            return RenderCsv(ds, "Usage_Summary_");
        }
        #endregion

        #region Daily Statistics Summary
        private const string UsageSummaryByDaySQL = @"
            --Estimated Savings by Day
            SET	DATEFIRST 1
            DECLARE		@MinDate	DATETIME
            SELECT	@MinDate = MIN(CreatedAt) FROM WorkStates;

            WITH CTE_LAST_YEAR AS (
	            SELECT	1 AS DayID
			            ,CAST(GETDATE() AS date) AS [Date]
	            UNION ALL
	            SELECT	CTE_LAST_YEAR.DayID + 1 AS DayID
			            ,DATEADD(d, -1, CTE_LAST_YEAR.[Date]) AS [Date]
	            FROM	CTE_LAST_YEAR
	            WHERE	DATEADD(d, -1, CTE_LAST_YEAR.[Date]) > DATEADD(d, -1000, GETDATE())
	              AND	DATEADD(d, -1, CTE_LAST_YEAR.[Date]) > DATEADD(d, -4, @MinDate)	  
            ),
            CTE_HOURS AS (
	            SELECT	CONVERT(CHAR(10), CreatedAt, 121) [Day]
			            , COUNT(1) [Hours Saved]
			            , COUNT(1) * 35 [Cost Saved]
	            FROM	WorkStates
	            WHERE	Code IN (30101,50101)
	            GROUP BY CONVERT(CHAR(10), CreatedAt, 121)
            )

            SELECT		wd.[Date] AS [Day]
			            ,ISNULL(h.[Hours Saved], 0) AS [Hours Saved]
			            ,ISNULL(h.[Cost Saved], 0) AS [Cost Saved]
            FROM	CTE_LAST_YEAR wd
            LEFT OUTER JOIN	CTE_HOURS h
	            ON	wd.[Date] = h.[Day]
            ORDER BY	wd.[Date]
            OPTION (MaxRecursion 1000)

            -- Savings by AIR Id
            SELECT	REPLACE(LEFT(p.Name, 4), '_', '') [AIR ID]
		            , MAX(SUBSTRING(p.Name, CHARINDEX('_', p.Name, 0)+1, 1000)) [Name]
		            , COUNT(1) [Hours Saved]
            FROM	WorkStates w
            INNER JOIN Tickets t
	            ON	w.TicketId = t.TicketId
            INNER JOIN Projects p
	            ON	t.ProjectId = p.ProjectId
            WHERE	w.Code IN (30101,50101)
            GROUP BY LEFT(p.Name, 4)
            ORDER BY COUNT(1) DESC;

            -- Speed to complete migration
            WITH CTE_TICKETS_COMPLETED AS (
	            SELECT	TicketId
	            FROM	WorkStates
	            WHERE	Code = 50102
            ),
            CTE_DURATION AS
            (
            SELECT	WS.TicketId
		            , MIN(CreatedAt) as 'StartTime'
		            , MAX(CreatedAt) as 'EndTime'
		            , DATEDIFF(s, min(CreatedAt), MAX(CreatedAt)) as 'Duration (s)'
            FROM	dbo.WorkStates WS
            INNER JOIN	CTE_TICKETS_COMPLETED TC
	            ON	WS.TicketId = TC.TicketId
            GROUP BY WS.TicketId
            )

            SELECT	[Duration (s)]
		            , COUNT(1) [Count]
            FROM	CTE_DURATION
            GROUP BY [Duration (s)]
            ORDER BY [Duration (s)];

            --Final State for Tickets
            WITH CTE_TICKETS_MAX_STATE AS (
	            SELECT	TicketId
			            , MAX(CreatedAt) [MaxTime]
	            FROM	WorkStates
	            GROUP BY TicketId
            )

            SELECT	WS.Code [State]
		            , WS.Name [Description]
		            , COUNT(1) [Count]
            FROM	CTE_TICKETS_MAX_STATE CTMS
            INNER JOIN 
		            WorkStates WS
	            ON	CTMS.TicketId = WS.TicketId
	            AND	CTMS.MaxTime = WS.CreatedAt
            GROUP BY WS.Code
		            , WS.Name
            ORDER BY WS.Code
";
        public ActionResult UsageStats()
        {
            DataSet ds = DaoUtil.ExecQueryMulti(CommandType.Text, UsageSummaryByDaySQL, null);
            ds.Tables[0].TableName = "Savings By Day";
            ds.Tables[1].TableName = "Tickets By AIR Id";
            ds.Tables[2].TableName = "Speed to complete migration";
            ds.Tables[3].TableName = "Ticket Final State";
            return RenderCsv(ds, "Usage_Stats_");
        }
        #endregion

        #region Statistics Details
        private const string WorkflowDetailsSQL = @"
            SELECT  p.Name [ProjectName]
		            , t.TicketId
		            , t.ItgNumber
		            , t.UserId [CreatedBy]
		            , w.Code
		            , w.Name [WorkState]
		            , w.LogMessage
		            , w.CreatedAt
		            , w.AuthorizedBy
		            , w.DbServer
		            , w.DbName
            FROM	WorkStates w
            INNER JOIN Tickets t
	            ON	w.TicketId = t.TicketId
            INNER JOIN Projects p
	            ON	t.ProjectId = p.ProjectId
            WHERE	w.CreatedAt BETWEEN @StartDate AND @EndDate
            ORDER BY p.ProjectId
		            , t.TicketId
		            , w.WorkStateId";

        public ActionResult WorkflowDetails()
        {
            DateTime startDate;
            DateTime endDate;
            if (!DateTime.TryParse(Request.Params["startDate"], out startDate))
            {
                startDate = new DateTime(1753, 1, 1, 12, 0, 0);
            }

            if (!DateTime.TryParse(Request.Params["endDate"], out endDate))
            {
                endDate = new DateTime(9999, 12, 31, 11, 59, 59);
            }

            DataTable dt = DaoUtil.ExecQuery(CommandType.Text, WorkflowDetailsSQL, 
                                                        new SqlParameter[] {
                                                            new SqlParameter("StartDate", startDate),
                                                            new SqlParameter("EndDate", endDate),
                                                        });

            return RenderCsv(dt, "Workflow_Details_");
        }

        #endregion
        #endregion

        #region Ticket Reports
        #region Ticket Details
        private const string TicketDetailSQL = @"
            select	t.TicketId [Ticket ID]
		            , t.Description [Description]
		            , p.Name [Project Name]
		            , p.StagingHost [Staging Host]
		            , p.StagingPort [Staging Port]
		            , p.StagingDatabase [Staging Database]
		            , p.ProductionHost [Production Host]
		            , p.ProductionPort [Production Port]
		            , p.ProductionDatabase [Production Database]
		            , t.UserId [Submitter]
		            , t.ItgNumber [ITG Number]
            from Tickets t
            inner join Projects p
              on t.ProjectId = p.ProjectId
            where t.TicketId = @TicketId

            select	Code
		            , Name
		            , Note
		            , CreatedAt
		            , LogMessage
            from WorkStates
            where TicketId = @TicketId
            order by CreatedAt
                    , Code

            select  ProjectId
                    , TicketId
                    , LockId
                    , ProjectName
                    , ServerCategory
                    , TicketCreator
                    , LockReason
                    , LockedBy
                    , LockedAt
                    , UnlockedAt
                    , UnlockReason
            from ProcessLocksArchive
            where TicketId = @TicketId
            Order by LockedAt

            select  ByUser [Author]
		            , CreatedAt [Date]
                    , Text [Text]
            from Comments
            where TicketId = @TicketId
            Order by CommentId
            ";
        public ActionResult TicketDetails()
        {
            int ticketId;
            if (!int.TryParse(Request.Params["TicketId"], out ticketId))
            {
                return Content("Bad Ticket Number.");
            }

            DataSet ds = DaoUtil.ExecQueryMulti(CommandType.Text, TicketDetailSQL, new SqlParameter[]{new SqlParameter("TicketId", ticketId)});
            ds.Tables[0].TableName = "Ticket Summary";
            ds.Tables[1].TableName = "Workflow Details";
            ds.Tables[2].TableName = "Lock History";
            ds.Tables[3].TableName = "Comments";
            return RenderCsv(ds, "Ticket_Details_" + ticketId.ToString());
        }
        #endregion

        #endregion

    }
}
