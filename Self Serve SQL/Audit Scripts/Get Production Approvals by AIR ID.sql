SELECT  p.Name
        , ws.TicketId
        , t.UserId [Submitter]
        , ws.*
FROM    WorkStates ws
LEFT OUTER JOIN Tickets t
  ON    t.TicketID = ws.TicketID
LEFT OUTER JOIN Projects p
  ON    t.ProjectId = p.ProjectId
WHERE   p.Name like '2700%' --AIR ID goes here
  AND   Code in (50101, 50102)  --Pulls back Prod Started Execution (50101) and Execution concluded with success (50102)
ORDER BY p.Name
        , ws.TicketID
        , ws.WorkStateId
