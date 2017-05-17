DECLARE @SchemaName nvarchar(64)
DECLARE @TableName nvarchar(64)
DECLARE @SQL nvarchar(max)
DECLARE @ColumnSQL nvarchar(max)
DECLARE @ColumnName nvarchar(64)
DECLARE @ColumnId nvarchar(64)

SET @ColumnSQL = N''
SET @SchemaName = N'dbo'
SET @TableName = N'Accesses'

DECLARE ColumnList CURSOR
FOR SELECT	name, column_id
FROM	sys.columns
WHERE	object_id = OBJECT_ID(@TableName, N'U')
ORDER BY column_id

OPEN ColumnList

FETCH NEXT FROM ColumnList 
INTO @ColumnName, @ColumnId

WHILE @@FETCH_STATUS = 0
BEGIN

	SET @ColumnSQL = @ColumnSQL + ', ' + @ColumnName + ', CASE (__$update_mask & ' + CAST(POWER(2, @ColumnId-1) AS NVARCHAR(16)) + ') WHEN 0 THEN 0 ELSE 1 END [' + @ColumnName + ' Updated]'

	FETCH NEXT FROM ColumnList 
	INTO @ColumnName, @ColumnId
END

CLOSE ColumnList
DEALLOCATE ColumnList

SET @SQL =
N'
DECLARE @start_lsn binary(10), @end_lsn binary(10)
SELECT @start_lsn = (select MIN(__$start_lsn) from cdc.' + @SchemaName + '_' + @TableName + '_CT)
SET @end_lsn   = sys.fn_cdc_get_max_lsn()

SELECT	__$start_lsn [Transaction]
		, __$seqval [Sequence]
		, sys.fn_cdc_map_lsn_to_time (__$start_lsn) [Transaction Time]
		, CASE __$operation
			WHEN 1 THEN ''Delete''
			WHEN 2 THEN ''Insert''
			WHEN 3 THEN ''Pre-Update''
			WHEN 4 THEN ''Post-Update''
		END [Operation]
		, sys.fn_sqlvarbasetostr(__$update_mask) [Column Mask] ' + 
		@ColumnSQL +
'FROM	cdc.fn_cdc_get_all_changes_' + @SchemaName + '_' + @TableName + '(
			@start_lsn,
			@end_lsn,
			N''all update old'');
'

PRINT @SQL
EXEC (@SQL)