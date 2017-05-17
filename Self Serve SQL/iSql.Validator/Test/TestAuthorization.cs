#if DEBUG 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestAuthorization : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Authorization"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "GRANT ALL TO ERIN",
            "REVOKE ALL TO ERIN",
            "DENY ALL TO ERIN",
            "//here is a comment GRANT ALL TO ERIN",
            "//here is a comment\nGRANT ALL TO ERIN",
            "/*here is a comment*/GRANT/*another comment*/ ALL TO ERIN",
            "GRANT @sql",
            "GRANT(@SQL)",
            "GRANT@SQL",
            "[GRANT]",
            "grant",
            "grant all to erin"
       };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select * from tbl_grant",
            "select * from tbl_grant_single_statement",
            "sp_grant"
        };

    }
}

#endif