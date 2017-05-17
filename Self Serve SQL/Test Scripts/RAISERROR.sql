	RAISERROR ('More records identified than expected, update will not be executed', 11, 1) WITH NOWAIT
	RETURN

SELECT *
FROM sys.objects
WHERE xtype = 'U'
