#if DEBUG 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestDrop : TestValidateSqlBase {


        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Drop"
                         select val);

        }

        private static string[] _BadSqlScripts = new string[] {

            // ------------------------------------------------------------------------
            // Real world multiline drop query 
            // ------------------------------------------------------------------------
            @"
                USE AdventureWorks2008R2;
                GO
                CREATE TABLE #temptable (col1 int);
                GO
                INSERT INTO #temptable
                VALUES (10);
                GO
                SELECT * FROM #temptable;
                GO
                IF OBJECT_ID(N'tempdb..#temptable', N'U') IS NOT NULL 
                DROP TABLE #temptable;
                GO
                --Test the drop.
                SELECT * FROM #temptable;
            ", 

             @"
                DROP USER AbolrousHazem;
                GO
             ", 

            //NOTE: questionalble drop trigger.... since it is such a common task during massive update, insesrt, etc
             @"
                IF OBJECT_ID ('employee_insupd', 'TR') IS NOT NULL
                   drop TRIGGER employee_insupd;
                GO
             ",

            //NOTE: for now, we consider drop user scripts are bad scripts 
            @"
                USE mydb;
                DROP USER someone;
                GO
             ",
        };

        private static string[] _GoodSqlScripts = new string[] {
            @"
                EXEC sp_addlinkedserver 'OracleSvr', 
                   'Oracle 7.3', 
                   'MSDAORA', 
                   'ORCLDB'
                GO
                SELECT *
                FROM OPENQUERY(OracleSvr, 'SELECT name, id FROM joe.titles') 
                GO
            ",
            @"
                IF  EXISTS (SELECT * FROM myTable WHERE name = N'syn_myTable')
                    DROP SYNONYM [dbo].[syn_myTable]
                GO
            ",

        };

    }
}

#endif