using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestAuthentication : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Authentication"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "CREATE USER ERIN",
            "UPDATE USER ERIN",
            "DELETE USER ERIN",
            "//here is a comment CREATE USER ERIN",
            "//here is a comment\nUPDATE USER ERIN",
            "/*here is a comment*/DELETE/*another comment*/USER ERIN",
            "CREATE USER @sql",
            "CREATE USER(@SQL)",
            "CREATE USER@SQL",
            "[CREATE USER]",
            "create user",
            "create user erin"
       };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select * from tbl_create_user Where 1=1",
            "select * from tbl_create_user_single_statement Where 1=1",
            "sp_create_user"
        };

    }
}