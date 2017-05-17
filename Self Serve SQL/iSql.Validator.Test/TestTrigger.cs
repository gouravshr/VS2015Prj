using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestTrigger : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Trigger"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "ALTER TRIGGER",
            "CREATE TRIGGER",
            "UPDATE TRIGGER",
            "DELETE TRIGGER",
            "DROP TRIGGER",
            "ENABLE TRIGGER",
            "DISABLE TRIGGER",
            "//here is a comment CREATE TRIGGER",
            "//multiline\nCREATE\nTRIGGER\n",
            "create trigger",
            "/*something*/create/*something*/trigger/*something*/"
       };


        private string[] _GoodSqlScripts = new string[] {
            "//Seperated by spaces C R E A T E T R I G G E R",
            "//Incomplete words CREAT TRIGGE",
            "//Underscores CREATE_TRIGGER",
            "sp_helptrigger",
            "SELECT * FROM sys.triggers Where 1=1",
            "SELECT * FROM sys.trigger_events Where 1=1",
            "sp_settriggerorder"            
        };


    }
}