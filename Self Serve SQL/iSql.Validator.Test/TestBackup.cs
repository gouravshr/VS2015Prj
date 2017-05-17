using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestBackup : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "BackupRestore"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "BACKUP database",
            "//here is a comment BACKUP sp_who2",
            "//here is a comment\nBACKUP database\n//And another comment",
            "/*here is a comment*/BACKUP database",
            "backup",
            "backup database"
       };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select * from tbl_backup Where 1=1",
            "select * from tbl_backup_database Where 1=1",
            "sp_backupdb"
        };


    }
}