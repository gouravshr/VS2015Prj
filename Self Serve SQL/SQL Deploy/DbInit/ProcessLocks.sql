
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ProcessLocks](
	[LockId] [int] IDENTITY(1,1) NOT NULL,
	[TicketId] [int] NOT NULL,
	[ProjectId] [int] NOT NULL,
	[ProjectName] [varchar](256) NOT NULL,
	[ServerCategory] [varchar](20) NOT NULL,
	[TicketCreator] [nvarchar](128) NOT NULL,
	[LockReason] [varchar](1024) NOT NULL,
	[LockedBy] [nvarchar](128) NOT NULL,
	[LockedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_ProcessLocks] PRIMARY KEY CLUSTERED 
(
	[LockId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


