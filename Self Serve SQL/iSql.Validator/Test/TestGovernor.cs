#if DEBUG 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestGovernor : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "ResourceGovernor"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "CREATE RESOURCE POOL",
            "ALTER RESOURCE POOL",
            "DROP RESOURCE POOL",
            "CREATE WORKLOAD GROUP",
            "ALTER WORKLOAD GROUP",
            "DROP WORKLOAD GROUP",
            "ALTER RESOURCE GOVERNOR",
            "//here is a comment CREATE RESOURCE POOL",
            "//multiline\nCREATE\nRESOURCE\nPOOL\n",
            "create resource pool",
            "create /*something*/ resource /*something else*/ pool"
       };


        private string[] _GoodSqlScripts = new string[] {
            "//Seperated by spaces C R E A T E R E S O U R C E P O O L",
            "//Incomplete words CREAT RESOURC POL",
            "//Underscores CREATE_RESOURCE_POOL"
        };


    }
}

#endif