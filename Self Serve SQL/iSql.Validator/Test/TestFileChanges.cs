#if DEBUG 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestFileChanges : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "FileChanges"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "ALTER DATABASE TEST ADD FILE (NAME='NEWFILE')",
            "//test seperate lines\nALTER\nDATABASE\nTEST\nREMOVE\nFILE\n(NAME='FILE')",
            "/*in between comments*/ALTER/*comment*/DATABASE/*comment*/TEST/*comment*/REMOVE/*comment*/FILE/*comment*/(NAME='FILE')",
            "//here is a comment\nALTER DATABASE TEST MODIFY FILE (NAME='FILE')//And another comment",
            "alter database test add file (name='newfile')"
       };


        private string[] _GoodSqlScripts = new string[] {
            "alter database set file_changes"
        };


    }
}

#endif