SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetMyTickets] 
	  @UserId as varchar(30)
	, @SinceDate as datetime 
AS

select t.* , w.Name as [Status] 
from Tickets t 
join WorkStates w
on t.TicketId = w.TicketId
and w.WorkStateId = (select max(WorkStateId) from WorkStates where TicketId = t.TicketId ) 
and projectid  in ( select projectId from Accesses where userid = @UserId  )
and  t.CreatedAt >= @SinceDate
and t.UserId = @UserId
order by createdat desc



GO

