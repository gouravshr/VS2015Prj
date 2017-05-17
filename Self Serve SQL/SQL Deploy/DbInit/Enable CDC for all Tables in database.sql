EXEC sys.sp_cdc_enable_db
GO

DECLARE @TableName varchar(64)
DECLARE @SchemaName varchar(64)

DECLARE crsr_Tables CURSOR FOR
SELECT  name, SCHEMA_NAME(schema_id)
FROM	sys.tables
WHERE	is_tracked_by_cdc = 0
  AND	is_ms_shipped = 0

OPEN crsr_Tables

FETCH NEXT FROM crsr_Tables 
INTO	@TableName, @SchemaName

WHILE @@FETCH_STATUS = 0
BEGIN

	EXEC sys.sp_cdc_enable_table
	@source_schema = @SchemaName,
	@source_name   = @TableName,
	@role_name     = NULL,
	@supports_net_changes = 1

	print @TableName
		
	FETCH NEXT FROM crsr_Tables
	INTO	@TableName, @SchemaName
END

CLOSE crsr_Tables
DEALLOCATE crsr_Tables

GO