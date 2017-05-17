#if DEBUG 
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
                                                   
            "select * from tbl_execute",
            "select * from tbl_exec_single_statement",
            "sp_executesql"
        };

    }
}

#endif