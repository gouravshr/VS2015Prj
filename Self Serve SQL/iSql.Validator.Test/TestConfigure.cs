using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestConfigure : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Configuration"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "sp_configure",
            "RECONFIGURE",
            "sp_configure 'MAX DOP', 1\nRECONFIGURE WITH OVERRIDE",
            "//here is a comment sp_configure",
            "//here is a comment\nRECONFIGURE",
            "/*here is a comment*/sp_configure/*another comment*/",
            "sp_configure@sql",
            "RECONFIGURE(@SQL)",
            "SP_CONFIGURE",
            "reconfigure",
        };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select * from configure Where 1=1",
            "sp_conf/*a comment/*ifgure"
        };

    }
}