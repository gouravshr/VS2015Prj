#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestTruncate : TestValidateSqlBase {
        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Truncate"
                         select val);
        }

        private static string[] _BadSqlScripts = new string[] {
             @"truncate 
               table_test
             ", 

             @"truncate tbl_dummy ", 

             @"TRUNCATE tblBlah ", 

             @" truncate\ttblTest ", 
        };

        private static string[] _GoodSqlScripts = new string[] {
            @"selct col1, col2 
              from table_trunc
            ",

            @"UPDATE peopel SET email='test.user@accenture.com' "
        };


    }
}

#endif