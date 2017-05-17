SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[ReleaseLock] 
	-- most app code handle ticket id, and they have to get the Ticket object first to get its associated project id because of Entity Framwork, 
	-- now we can just pass ticket id direclty here, and we'll figure out the ProjectId on the fly.. without join, just simple select
	
	-- NOTE: per latest discussion, no need to distinguish staging/prod stage, just simply lock and unlock the whole project
	
	@TicketId as int				, 	
	-- @ServerCategory as varchar(20)	,
	@UnlockReason varchar(500) = NULL
AS

declare  @ErrCode int 
declare  @ErrMsg varchar(500)

declare	 @ProjectId int

BEGIN TRY

	select @ProjectId = ProjectId from Tickets where TicketId = @TicketId 

	-- move items to archive table, keep main transactional table tiny and fast
	insert into ProcessLocksArchive
	( 
		  LockId
		, TicketId
		, ProjectId
		, ProjectName
		, ServerCategory
		, TicketCreator
		, LockReason
		, LockedBy
		, LockedAt
		, UnlockedAt
		, UnlockReason
	)
	select		p.LockId
		   ,	p.TicketId 
		   ,	p.ProjectId
		   ,	p.ProjectName
		   ,	p.ServerCategory
		   ,	p.TicketCreator
		   ,	p.LockReason
		   ,	p.LockedBy 
		   ,	p.LockedAt 
		   ,	GetDate() 
		   ,	@UnlockReason
	from  ProcessLocks p
	 
	where  p.ProjectId = @ProjectId
	-- and	   p.ServerCategory = @ServerCategory
	
	-- then delete records from main table 
	delete from ProcessLocks 
	where  ProjectId = @ProjectId
	--and	   ServerCategory = @ServerCategory
	
END TRY

BEGIN CATCH
	set @ErrCode = ERROR_NUMBER() 
	set @ErrMsg = ERROR_MESSAGE()
END CATCH;

-- now no matter what happend, you should see the existing item plus the error description, if any

select	  @ErrCode as ErrorCode
		, @ErrMsg as ErrorMsg

GO




