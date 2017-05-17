using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestRestore : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "BackupRestore"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "RESTORE database",
            "//here is a comment RESTORE sp_who2",
            "//here is a comment\nRESTORE database\n//And another comment",
            "/*here is a comment*/RESTORE database",
            "restore",
            "restore database"
       };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select * from tbl_restore Where 1=1",
            "select * from tbl_restore_database Where 1=1",
            "sp_restoredb"
        };


    }
}