using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestSnapshot : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Snapshot"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "SNAPSHOT database",
            "//here is a comment SNAPSHOT sp_who2",
            "//here is a comment\nSNAPSHOT database\n//And another comment",
            "/*here is a comment*/SNAPSHOT database",
            "snapshot",
            "snapshot database"
       };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select * from tbl_snapshot Where 1=1",
            "select * from tbl_snapshot_database Where 1=1",
            "sp_snapshotdb"
        };


    }
}