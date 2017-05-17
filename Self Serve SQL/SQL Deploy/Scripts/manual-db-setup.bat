REM TODO: make the folder configurable....
@echo on
c:
cd "C:\src\AccProjects\iSQL\src\SQL Deploy\DbInit"
echo "create process locks db .... " 
sqlcmd -E -S . -d iSqlDb-Dev -i ProcessLocks.sql
sqlcmd -E -S . -d iSqlDb-Dev -i ProcessLocksArchive.sql

REM we also need to import flow states data now


echo "create index... " 
cd  Index

sqlcmd -E -S . -d iSqlDb-Dev -i Idx_ProcessLocks_ProjectId_Unique.sql 
sqlcmd -E -S . -d iSqlDb-Dev -i Idx_WorkStates_TicketID.sql 

echo "create stored proc"
cd ..\..\StoredProcs
sqlcmd -E -S . -d iSqlDb-Dev -i GetFollowUpTickets.sql  
sqlcmd -E -S . -d iSqlDb-Dev -i GetLock.sql  
sqlcmd -E -S . -d iSqlDb-Dev -i GetMyTickets.sql  
sqlcmd -E -S . -d iSqlDb-Dev -i ReleaseLock.sql


echo "execute script to alter FK constrains with UPDATE CASCADE support that not coming with Entity Framwork 4.x code-first approach ..."


