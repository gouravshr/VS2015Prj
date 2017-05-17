
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetLock] 
	-- prjoect id may be really necessary since we can get it from the trace, but it makes
	  @TicketId as int		
	, @ServerCategory as varchar(20)
	, @LockedBy as varchar(200)
	, @LockReason as varchar(1024)			
AS

declare  @ErrCode int 
declare  @ErrMsg varchar(500)

BEGIN TRY

	insert into ProcessLocks
	( 
		  TicketId
		, ProjectId
		, ProjectName
		, ServerCategory
		, TicketCreator
		, LockReason
		, LockedBy
		, LockedAt
	)
	select   t.TicketId 
		   , p.ProjectId
		   , p.Name As ProjectName 
		   , @ServerCategory as ServerCategory
		   , t.UserId as TicketCreator  
		   , @LockReason as LockReason
		   , @LockedBy as LockedBy
		   , GetDate() as LockedAt  
	from  Projects p
		, Tickets t 
	where t.TicketId = @TicketId
	and t.ProjectId = p.ProjectId
	
END TRY

BEGIN CATCH
	set @ErrCode = ERROR_NUMBER() 
	set @ErrMsg = ERROR_MESSAGE()
END CATCH;

-- now no matter what happend, you should see the existing item plus the error description, if any
-- here we use the ticket id instead of project id, we already de-normalized the process lock table to contain such info to aovid extra join
select	  l.LockId
		, l.TicketId
		, l.ProjectId
		, l.ProjectName
		, l.ServerCategory
		, l.TicketCreator
		, l.LockReason
		, l.LockedBy
		, l.LockedAt
		, @ErrCode as ErrorCode
		, @ErrMsg as ErrorMsg
from ProcessLocks l, 
	 Tickets t
where t.TicketId = @TicketId
and l.ProjectId = t.ProjectId

GO


