using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestExec : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Exec"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "EXEC (\"SELECT * FROM TABLE\");",
            "//here is a comment EXEC sp_who2",
            "//here is a comment\nEXEC sp_who2",
            "/*here is a comment*/EXEC sp_who2",
            "EXECUTE @sql",
            "EXECUTE(@SQL)",
            "EXECUTE@SQL",
            "[EXECUTE]",
            "exec",
            "exec sp_who2"
       };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select * from tbl_execute Where 1=1",
            "select * from tbl_exec_single_statement Where 1=1",
            "sp_executesql_now",
            "INSERT INTO Table1  (Col1, Col2, Col3) VALUES (\"Senior Executives\", \"Managers\", \"Programmer\")" //From Erwin Rocha 12-Mar-2013
        };

    }
}