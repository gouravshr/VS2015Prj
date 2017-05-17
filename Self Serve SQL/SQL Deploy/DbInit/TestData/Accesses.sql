-- Setup approvers access for project 1-5 for Erin 

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'erin.d.rakickas',@2=1


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'erin.d.rakickas',@2=2


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'erin.d.rakickas',@2=3

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'erin.d.rakickas',@2=4


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'erin.d.rakickas',@2=5




-- Setup approvers access for project 1-5 for Josh 

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'josh.davidson',@2=1


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'josh.davidson',@2=2


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'josh.davidson',@2=3

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'josh.davidson',@2=4


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'josh.davidson',@2=5



-- Setup approvers access for project 1-5 for Scott 

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'scott.a.albrecht',@2=1


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'scott.a.albrecht',@2=2


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'scott.a.albrecht',@2=3

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'scott.a.albrecht',@2=4


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'scott.a.albrecht',@2=5


-- Setup approvers access for project 1-5 for Weinan 

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'weinan.li',@2=1


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'weinan.li',@2=2


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'weinan.li',@2=3

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'weinan.li',@2=4


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'weinan.li',@2=5


-- Setup Requester access for 1-5 for Justin

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'justin.j.wodstrchill',@2=1


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'justin.j.wodstrchill',@2=2


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'justin.j.wodstrchill',@2=3

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'justin.j.wodstrchill',@2=4


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'justin.j.wodstrchill',@2=5


-- Setup Requester access for 1-2, and Approver access for 3-5 for Jim

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'james.d.spring',@2=1


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'james.d.spring',@2=2


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'james.d.spring',@2=3

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'james.d.spring',@2=4


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'james.d.spring',@2=5


-- Setup Approver access for 1-2, and Requester access for 3-5 for Eugene

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'eugene.khazin',@2=1


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Approver',@1=N'eugene.khazin',@2=2


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'eugene.khazin',@2=3

exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'eugene.khazin',@2=4


exec sp_executesql N'insert [dbo].[Accesses]([Role], [UserId], [ProjectId]) values (@0, @1, @2) select [AccessId] from [dbo].[Accesses] where @@ROWCOUNT > 0 and [AccessId] = scope_identity()',N'@0 nvarchar(30),@1 nvarchar(50),@2 int',@0=N'Requester',@1=N'eugene.khazin',@2=5










