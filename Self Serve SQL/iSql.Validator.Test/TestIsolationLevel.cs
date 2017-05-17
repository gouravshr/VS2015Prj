using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestIsolationLevel : TestValidateSqlBase {
        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "IsolationLevelSetting"
                         select val);
        }

        private static string[] _BadSqlScripts = new string[] {
            @"
                USE AdventureWorks2008R2;
                GO
                SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
                GO
                BEGIN TRANSACTION;
                SELECT BusinessEntityID
                    FROM HumanResources.Employee;
                GO
            ",                                     
        };

        private static string[] _GoodSqlScripts = new string[] {
       
            @"
                SELECT * INTO [Clustering Europe Region]
                USING [Microsoft_Clustering] WITH FILTER(Region='Europe')
                FROM [TM Clustering] where 1=1;
            "
        };

    }
}