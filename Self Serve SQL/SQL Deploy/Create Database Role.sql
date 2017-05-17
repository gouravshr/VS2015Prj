/****************************************************/
/*                                                  */
/* Create 3371_SelfServeSql_Role                    */
/* Erin D. Rakickas                                 */
/* 6-Jul-2011                                       */
/*                                                  */
/****************************************************/

--Delete existing users
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'3371_SelfServeSql_Role' AND type = 'R')
Begin
	DECLARE @RoleMemberName sysname
	DECLARE Member_Cursor CURSOR FOR
	select [name]
	from sys.database_principals 
	where principal_id in ( 
		select member_principal_id 
		from sys.database_role_members 
		where role_principal_id in (
			select principal_id
			FROM sys.database_principals where [name] = N'3371_SelfServeSql_Role'  AND type = 'R' ))

	OPEN Member_Cursor;

	FETCH NEXT FROM Member_Cursor
	into @RoleMemberName

	WHILE @@FETCH_STATUS = 0
	BEGIN

		exec sp_droprolemember @rolename=N'3371_SelfServeSql_Role', @membername= @RoleMemberName

		FETCH NEXT FROM Member_Cursor
		into @RoleMemberName
	END;

	CLOSE Member_Cursor;
	DEALLOCATE Member_Cursor;
End

GO

--Delete existing Role
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'3371_SelfServeSql_Role' AND type = 'R')
DROP ROLE [3371_SelfServeSql_Role]
GO

--Create the Role
CREATE ROLE [3371_SelfServeSql_Role] AUTHORIZATION [dbo]
GO

--Add the required permissions
GRANT ALTER ANY FULLTEXT CATALOG TO [3371_SelfServeSql_Role]
GRANT ALTER ANY MESSAGE TYPE TO [3371_SelfServeSql_Role]
GRANT ALTER ANY REMOTE SERVICE BINDING TO [3371_SelfServeSql_Role]
GRANT ALTER ANY SCHEMA TO [3371_SelfServeSql_Role]
GRANT ALTER ANY SERVICE TO [3371_SelfServeSql_Role]
GRANT CREATE AGGREGATE TO [3371_SelfServeSql_Role]
GRANT CREATE FUNCTION TO [3371_SelfServeSql_Role]
GRANT CREATE PROCEDURE TO [3371_SelfServeSql_Role]
GRANT CREATE QUEUE TO [3371_SelfServeSql_Role]
GRANT CREATE SYNONYM TO [3371_SelfServeSql_Role]
GRANT CREATE TABLE TO [3371_SelfServeSql_Role]
GRANT CREATE TYPE TO [3371_SelfServeSql_Role]
GRANT CREATE VIEW TO [3371_SelfServeSql_Role]
GRANT DELETE TO [3371_SelfServeSql_Role]
GRANT EXECUTE TO [3371_SelfServeSql_Role]
GRANT INSERT TO [3371_SelfServeSql_Role]
GRANT REFERENCES TO [3371_SelfServeSql_Role]
GRANT SELECT TO [3371_SelfServeSql_Role]
GRANT SHOWPLAN TO [3371_SelfServeSql_Role]
GRANT UPDATE TO [3371_SelfServeSql_Role]
GRANT VIEW DATABASE STATE TO [3371_SelfServeSql_Role]
GRANT VIEW DEFINITION TO [3371_SelfServeSql_Role]
GO

-- Create the user
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'DIR\3371_SSS_App_Dev' AND type = 'U')
BEGIN
	CREATE USER [DIR\3371_SSS_App_Dev] FOR LOGIN [DIR\3371_SSS_App_Dev] WITH DEFAULT_SCHEMA=[dbo]
END
GO

exec sp_addrolemember '3371_SelfServeSql_Role', 'DIR\3371_SSS_App_Dev'
GO
