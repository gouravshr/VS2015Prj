using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Diagnostics;
using iSql.Commons;
using iSql.Commons.Models;

namespace iSql.Commons {
    /// <summary>
    /// Handle simple state creation and flow. 
    /// NOTE: we only create or add state, never drop or delete any existing state to preserve history.
    /// </summary>
    public class StateMachine {

        //TODO: secuirty checking, make sure user only try to access their own tickets 
        public static string versionNumber = System.Configuration.ConfigurationManager.AppSettings["VersionNumber"].ToString();

        #region security

        private const string ValidateAccessSql = "select a.Role from accesses a join tickets t on a.ProjectId=t.ProjectId and a.userid={1} and t.TicketId={0} ";
        public static string HasAccessToTicket(int ticketId, string enterpriseId) {
            using (var db = new EntDbContext()) {
                return db.Database.SqlQuery<string>(ValidateAccessSql, ticketId, enterpriseId).FirstOrDefault();
            }
        } 

        public const string IsCreatorOfTicketSql = "SELECT  COUNT(1)  FROM Tickets WHERE TicketId = {0} and UserId={1} ";
        public static bool IsCreator( int ticketId, string enterpriseId ) {
            using( var db = new EntDbContext()) {
                int count = db.Database.SqlQuery<int>(IsCreatorOfTicketSql, ticketId, enterpriseId).First();
                return count > 0;
            }
            
        }

        #endregion

        #region special states

        public static bool AddTerminateOnErrorState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.FlowTerminatedOnError, logMessage, StateCategory.FailedState);
        }

        public static bool AddTerminateNormallyState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.FlowTerminatedNormally, logMessage, StateCategory.SuccessState);
        }

        public static bool AddUserAbortState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.FlowTerminatedByUserAbort, logMessage, StateCategory.FailedState);
        }

        #endregion

        #region init stage

        public static bool AddFileUploadedState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.FileUploadSucceeded, logMessage, StateCategory.SuccessState);
        }

        public static bool AddValidationSucceedState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.ValidationSucceeded, logMessage, StateCategory.SuccessState);
        }

        public static bool AddValidationFailedState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.ValidationFailed, logMessage, StateCategory.FailedState);
        }

        #endregion

        #region supervisor's decision

        public static bool AddSupervisorWaitForDecision(int ticketId, string logMessage = null)
        {
            return AddWorkState(ticketId, FlowState.TicketWaitingForApproval, logMessage, StateCategory.UserAction);
        }

        public static bool AddSupervisorApprovalSucceedState(int ticketId, string logMessage = null, string authorizedBy = null)
        {
            return AddWorkState(ticketId, FlowState.TicketApproved, logMessage, StateCategory.SuccessState, authorizedBy);
        }

        public static bool AddSupervisorRejectFailedState(int ticketId, string logMessage = null, string authorizedBy = null)
        {
            return AddWorkState(ticketId, FlowState.TicketRejected, logMessage, StateCategory.FailedState, authorizedBy);
        }

        #endregion

        public static bool AddDBAOverwriteWaitForDecisionState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.DBAOverwriteWaitForDecision, logMessage, StateCategory.UserAction);
        }

        public static bool AddDBAOverwriteRequestedState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.DBAOverwriteRequested, logMessage, StateCategory.TransientState);
        }

        public static bool AddDBAOverwriteApprovedState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.DBAOverwriteApproved, logMessage, StateCategory.SuccessState);
        }

        public static bool AddDBAOverwriteRejectedState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.DBAOverwriteRejected, logMessage, StateCategory.FailedState);
        }

        #region staging stage
        
        //public static bool AddStageWaitForPushState(int ticketId, string logMessage = null, string authorizedBy = null ) {
        //    return AddWorkState(ticketId, FlowState.StageWaitForPush, logMessage, StateCategory.UserAction, authorizedBy);
        //}

        //public static bool AddPushToStagingStartedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
        //    return AddWorkState(ticketId, FlowState.StageExecutionStarted, logMessage, StateCategory.TransientState,  authorizedBy, dbServer: dbServer, dbName: dbName);
        //}

        //public static bool AddPushToStagingSucceedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
        //    return AddWorkState(ticketId, FlowState.StageExecutionSucceeded, logMessage, StateCategory.SuccessState, authorizedBy, dbServer: dbServer, dbName: dbName);
        //}

        //public static bool AddPushToStagingFailedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
        //    return AddWorkState(ticketId, FlowState.StageExecutionFailed, logMessage, StateCategory.FailedState, authorizedBy, dbServer: dbServer, dbName: dbName);
        //}

        //public static bool AddPushToStagingRollbackStartedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
        //    return AddWorkState(ticketId, FlowState.StageRollbackStarted, logMessage, StateCategory.TransientState, authorizedBy, dbServer: dbServer, dbName: dbName);
        //}
        //public static bool AddPushToStagingRollbackSucceedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
        //    return AddWorkState(ticketId, FlowState.StageRollbackSucceeded, logMessage, StateCategory.SuccessState, authorizedBy, dbServer: dbServer, dbName: dbName);
        //}

        //public static bool AddPushToStagingRollbackFailedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
        //    return AddWorkState(ticketId, FlowState.StageRollbackFailed, logMessage, StateCategory.FailedState, authorizedBy, dbServer: dbServer, dbName: dbName);
        //}

        //// validaiton by approver 
        //public static bool AddStagingExecutionValidationWaitingState(int ticketId, string logMessage = null, string authorizedBy = null ) {
        //    return AddWorkState(ticketId, FlowState.StageExecutionValidationWaiting, logMessage, StateCategory.UserAction, authorizedBy);
        //}

        //public static bool AddStagingExecutionValidationiConfirmedState(int ticketId, string logMessage = null, string authorizedBy = null ) {
        //    return AddWorkState(ticketId, FlowState.StageExecutionValidationConfirmed, logMessage, StateCategory.SuccessState, authorizedBy);
        //}

        //public static bool AddStagingExecutionValidationiRejectedState(int ticketId, string logMessage = null, string authorizedBy = null ) {
        //    return AddWorkState(ticketId, FlowState.StageExecutionValidationRejected, logMessage, StateCategory.FailedState, authorizedBy);
        //}

        //// staging roll back on validation rejection
        //public static bool AddStagingRollbackOnRejectionStartState(int ticketId, string logMessage = null, string authorizedBy = null ) {
        //    return AddWorkState(ticketId, FlowState.StageRollbackOnRejectionStarted, logMessage, StateCategory.TransientState, authorizedBy);
        //}

        //public static bool AddStagingRollbackOnRejectionSucceededState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null) {
        //    return AddWorkState(ticketId, FlowState.StageRollbackOnRejectionSucceeded, logMessage, StateCategory.SuccessState, authorizedBy, dbServer:dbServer, dbName:dbName );
        //}

        //public static bool AddStagingRollbackOnRejectionFailedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null) {
        //    return AddWorkState(ticketId, FlowState.StageRollbackOnRejectionFailed, logMessage, StateCategory.FailedState, authorizedBy, dbServer:dbServer, dbName:dbName );
        //}
        
        #endregion

        #region prod stage

        #region push to prod
        public static bool AddProductWaitForPush(int ticketId, string logMessage = null, string authorizedBy = null ) {
            return AddWorkState(ticketId, FlowState.ProductWaitForPush, logMessage, StateCategory.UserAction, authorizedBy);
        }

        public static bool AddPushToProductionStartedState(int ticketId, string logMessage = null, string authorizedBy = null, string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.ProductExecutionStarted, logMessage, StateCategory.TransientState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }

        public static bool AddPushToProductionSucceedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.ProductExecutionSucceeded, logMessage, StateCategory.SuccessState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }

        public static bool AddPushToProductionCompletedState(int ticketId, string logMessage = null, string authorizedBy = null, string dbServer = null, string dbName = null)
        {
            return AddWorkState(ticketId, FlowState.ProductExecutionCompletedWithError, logMessage, StateCategory.PartialSucess, authorizedBy, dbServer: dbServer, dbName: dbName);
        }

        public static bool AddPushToProductionFailedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.ProductExecutionFailed, logMessage, StateCategory.FailedState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }
        #endregion

        #region  prod roll back on push to prod failure
        public static bool AddPushToProductionRollbackStartedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.ProductRollbackStarted, logMessage, StateCategory.TransientState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }

        public static bool AddPushToProductionRollbackSucceedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.ProductRollbackSucceeded, logMessage, StateCategory.SuccessState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }

        public static bool AddPushToProductionRollbackFailedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.ProductRollbackFailed, logMessage, StateCategory.FailedState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }
        #endregion

        #region  staging rollback at prod stage
        public static bool AddPushToProductionStagingRollbackStartedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null) {
            return AddWorkState(ticketId, FlowState.StageRollbackOnProdStageStarted, logMessage, StateCategory.TransientState, authorizedBy, dbServer:dbServer, dbName:dbName );
        }

        public static bool AddPushToProductionStagingRollbackSucceedState(int ticketId, string logMessage = null, string authorizedBy = null, string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.StageRollbackOnProdStageSucceeded, logMessage, StateCategory.SuccessState, authorizedBy, dbServer:dbServer, dbName:dbName );
        }

        public static bool AddPushToProductionStagingRollbackFailedState(int ticketId, string logMessage = null, string authorizedBy = null, string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.StageRollbackOnProdStageFailed, logMessage, StateCategory.FailedState, authorizedBy, dbServer:dbServer, dbName:dbName );
        }

        #endregion

        #region  production validation states
        public static bool AddProdExecutionValidationWaitingState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.ProductExeuctionValidationWaiting, logMessage, StateCategory.UserAction);
        }

        public static bool AddProdExecutionValidationConfirmedState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.ProductExecutionValidationApproved, logMessage, StateCategory.SuccessState);
        }

        public static bool AddProdExecutionValidationRejectedState(int ticketId, string logMessage = null) {
            return AddWorkState(ticketId, FlowState.ProductExecutionValidationRejected, logMessage, StateCategory.FailedState);
        }

        #endregion

        #region  prod roll back on prod validation rejection
        public static bool AddProdRollbackOnProdRejectionStartState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null) {
            return AddWorkState(ticketId, FlowState.ProductRollbackOnProdRejectionStarted, logMessage, StateCategory.TransientState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }

        public static bool AddProdRollbackOnProdRejectionSucceededState(int ticketId, string logMessage = null, string authorizedBy = null, string dbServer = null, string dbName = null ) {
            return AddWorkState(ticketId, FlowState.ProductRollbackOnProdRejectionSucceeded, logMessage, StateCategory.SuccessState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }

        public static bool AddProdRollbackOnProdRejectionFailedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null) {
            return AddWorkState(ticketId, FlowState.ProductRollbackOnProdRejectionFailed, logMessage, StateCategory.FailedState, authorizedBy, dbServer:dbServer, dbName:dbName);
        }

        #endregion

        #region  staging roll back on prod validation rejection
        
        //public static bool AddStagingRollbackOnProdRejectionStartState(int ticketId, string logMessage = null, string authorizedBy = null, string dbServer = null, string dbName = null ) {
        //    return AddWorkState(ticketId, FlowState.StagingRollbackOnProdRejectionStarted, logMessage, StateCategory.TransientState, authorizedBy, dbServer:dbServer, dbName:dbName);
        //}

        //public static bool AddStagingRollbackOnProdRejectionSucceededState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null) {
        //    return AddWorkState(ticketId, FlowState.StagingRollbackOnProdRejectionSucceeded, logMessage, StateCategory.SuccessState, authorizedBy, dbServer:dbServer, dbName:dbName);
        //}

        //public static bool AddStagingRollbackOnProdRejectionFailedState(int ticketId, string logMessage = null, string authorizedBy = null , string dbServer = null, string dbName = null) {
        //    return AddWorkState(ticketId, FlowState.StagingRollbackOnProdRejectionFailed, logMessage, StateCategory.FailedState, authorizedBy, dbServer:dbServer, dbName:dbName);
        //}
        
        #endregion

        #endregion

        #region helper methods

        public static bool AddWorkState(int ticketId, WorkState workState) {
            //TODO: discard EF and go with pure stored proc for efficiency
            
            using (var db = new EntDbContext()) {
                string dbType = (from prj in db.Projects
                                 join tkt in db.Tickets on prj.ProjectId equals tkt.ProjectId
                                 where tkt.TicketId == ticketId
                                 select prj.DatabaseType).FirstOrDefault();
                workState.DbType = dbType;
                db.Workstates.Add(workState);
                db.SaveChanges();
            }
            //TODO: what if SQL exception thrown during state saving? Do we just return false or directly throw exception? Should we handle it here or what?  
            return true;
        }

        public static bool AddWorkState(int ticketId, State state, string logMessage = null, string category = null, string authorizedBy = null, string dbServer = null, string dbName = null ) {
            var workstate = new WorkState {
                Code = state.Code,
                Name = state.Description,
                TicketId = ticketId,
                LogMessage = logMessage,
                CreatedAt = DateTime.Now,
                Category = category,

                // now we have the AuthorizedBy field 
                AuthorizedBy = authorizedBy,

                // now add additional db server and db name fields
                DbServer = dbServer, 
                DbName = dbName ,
            };
            return AddWorkState(ticketId, workstate);
        }

        #endregion
    }

     public static class RollbackReason {
        public const string OnPushToStage = "Script execution failed when pushing to staging environment." ;
        public const string OnPushToProd = "Script execution failed when pushing to product environment." ;

        public const string OnStageRejection = "User rejected staging exeuciton results via manual validation process." ;
        public const string OnProdRejection = "User rejected product exeuciton results via manual validation process." ;
    }
}