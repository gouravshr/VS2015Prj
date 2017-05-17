CREATE TABLE [dbo].[FlowStates](
        [Code] [int] NOT NULL,
        [Description] [nvarchar](1024) NOT NULL,
        [Category] [nvarchar](100) NULL,
 CONSTRAINT [PK_FlowStates] PRIMARY KEY CLUSTERED
(
        [Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]



------------------------------------------------------------------------------------------------------
-- Insert Flow State Definitions as of date: 8/25/2011 2:30:03 PM
------------------------------------------------------------------------------------------------------
INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (200, 'Workflow - finished',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (500, 'Workflow - terminated on error',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (1200, 'workflow - aborted by user',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (10001, 'Ticket initialized',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (10101, 'File upload - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (10102, 'File upload - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (10103, 'File upload - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (10201, 'Validation - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (10202, 'Validation - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (10203, 'Validation - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30001, 'Stage ready - wait for approval',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30101, 'Stage execution - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30102, 'Stage execution - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30103, 'Stage execution - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30201, 'Stage rollback - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30202, 'Stage rollback - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30203, 'Stage rollback - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30301, 'Stage validation - waiting',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30302, 'Stage validation - confirmed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30303, 'Stage validation - rejected',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30401, 'Staging rollback on rejection - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30402, 'Staging rollback on rejection - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (30403, 'Staging rollback on rejection - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50001, 'Production ready - wait for approval',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50101, 'Production exeuction - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50102, 'Production exeuction - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50103, 'Production exeuction - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50201, 'Production rollback - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50202, 'Production rollback - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50203, 'Production rollback - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50301, 'Staging rollback on prod stage - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50302, 'Staging rollback on prod stage - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50303, 'Staging rollback on prod stage - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50401, 'Production validation - waiting',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50402, 'Production validation - approved',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50403, 'Production validation - rejected',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50501, 'Prod rollback on rejection - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50502, 'Prod rollback on rejection - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50503, 'Prod rollback on rejection - failed',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50601, 'Staging rollback on prod rejection - started',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50602, 'Staging rollback on prod rejection - succeeded',NULL)
 INSERT INTO [FlowStates]([Code] ,[Description],[Category]) VALUES (50603, 'Staging rollback on prod rejection - failed',NULL)
