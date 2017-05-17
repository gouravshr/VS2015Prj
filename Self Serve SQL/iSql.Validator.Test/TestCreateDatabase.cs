using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestCreateDatabase : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "CreateDatabase"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "CREATE DATABASE ERIN",
            "ALTER DATABASE ERIN",
            "DROP DATABASE ERIN",
            "//here is a comment CREATE DATABASE ERIN",
            "//here is a comment\nALTER DATABASE ERIN",
            "/*here is a comment*/DROP/*another comment*/DATABASE ERIN",
            "CREATE DATABASE @sql",
            "CREATE DATABASE(@SQL)",
            "CREATE DATABASE@SQL",
            "[CREATE DATABASE]",
            "create database",
            "create database erin",
            "CREATE\nDATABASE"
       };

        private string[] _GoodSqlScripts = new string[] {                                                   
            "select * from tbl_create_database Where 1=1",
            "select * from tbl_create_database_single_statement Where 1=1",
            "sp_create_database",
            "DATABASE CREATE"
        };

    }
}