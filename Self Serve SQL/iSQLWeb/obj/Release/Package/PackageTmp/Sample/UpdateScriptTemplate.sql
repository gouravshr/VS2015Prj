/**********************************************/
/*                                            */
/* Title:                                     */
/* Application Name:                          */
/* Author:                                    */
/* Date:                                      */
/* Purpose:                                   */
/* Notes:                                     */
/*                                            */
/**********************************************/

PRINT	'Selecting Data prior to update'

SELECT	KEY_COLUMN
		, COLUMN_TO_BE_UPDATED
FROM	TABLE_NAME
WHERE	SELECTION_CRITERIA

IF @@ROWCOUNT <> EXPECTED_ROWCOUNT
BEGIN
	RAISERROR ('More records identified than expected, update will not be executed', 11, 1) WITH NOWAIT
	RETURN
END

PRINT	'Updating Data'

UPDATE	TABLE_NAME
SET		COLUMN_TO_BE_UPDATED = VALUE
WHERE	SELECTION_CRITERIA

IF @@ROWCOUNT <> EXPECTED_ROWCOUNT
BEGIN
	PRINT 'Error, Rowcount does not match, recommend rolling back the update'
END

PRINT	'Selecting Data after update'

SELECT	KEY_COLUMN
		, COLUMN_TO_BE_UPDATED
FROM	TABLE_NAME
WHERE	SELECTION_CRITERIA
