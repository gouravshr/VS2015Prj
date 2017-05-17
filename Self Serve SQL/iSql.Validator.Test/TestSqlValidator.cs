using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestSqlValidator : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {
            BadSqlScripts.AddRange(_BadScript);

            targets = SqlValidation.sqlValidators;
        }

        protected static string[] _BadScript = new string[] {
           // calls has both 
           @"   
            DROP DATABASE sales_table_blah_blah_blah
            Go
           "
           };

        //BS stands for Bad Sql.... BTW.
        protected static string BS_Drop_Truncate = @"

            DROP TABLE AdventureWorks2008R2.dbo.SalesPerson2 ;
            GO

 
            truncate table MyTestDb.dbo.SalesHistory
            Go
        ";

        [Test]
        public void TestScriptWithMultipleErrors() {
            var errors = new List<string>();
            bool flag = SqlValidation.Validate(BS_Drop_Truncate, errors);
            //bool flag = TargetMethod(BS_Drop_Truncate, errors);

            Assert.IsFalse(flag);
            Assert.IsTrue(errors.Count > 1);
            Assert.IsTrue(errors.Contains("DROP statement is not allowed in SQL script."));
            Assert.IsTrue(errors.Contains("TRUNCATE statement is not allowed in SQL script."));
            Assert.AreEqual(expected: 2, actual: errors.Count);

        }
    }
}