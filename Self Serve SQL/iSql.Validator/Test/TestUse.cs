#if DEBUG 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestUse : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Use"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "USE database",
            "//here is a comment USE sp_who2",
            "//here is a comment\nUSE database\n//And another comment",
            "/*here is a comment*/USE database",
            "use",
            "use database"
       };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select * from tbl_use",
            "select * from tbl_use_database",
            "sp_usedb"
        };


    }
}

#endif