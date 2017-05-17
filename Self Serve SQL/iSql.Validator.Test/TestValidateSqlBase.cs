﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using iSql.Validator;

namespace iSql.Validator.Test {
    [TestFixture]
    public abstract class TestValidateSqlBase {
        //since it is base class now, no long make them static, and make it List instead of array now, so setup method can easily add testing scripts.
        internal List<string> GoodSqlScripts = new List<string>();
        internal List<string> BadSqlScripts = new List<string>();
        internal List<SqlValidator> targets = new List<SqlValidator>();

        internal IEnumerable<SqlValidator> enTargets
        {
            set
            {
                targets = new List<SqlValidator>();

                foreach (SqlValidator val in value)
                    targets.Add(val);
            }       
        }

        //SetUpFixture can do the SetUp for you only once, but unfortunately only one SetUpFixture is supported for one namespace.... so here comes the workaround
        public bool InitializedFlag { get; set; }
        [SetUp]
        public virtual void SetUp() {
            if (!InitializedFlag) {
                InitSqlTestingScripts();
                InitializedFlag = true;
            }
        }

        public abstract void InitSqlTestingScripts();
      

        [Test]
        public void TestBadSql() {
            foreach (var badSqlScript in BadSqlScripts) {
                Assert.IsFalse(SqlValidation.Validate(badSqlScript, targets, null), "Checking should fail for SQL: " + badSqlScript);

                //Check No Errors returned
                List<string> errors = new List<string>();
                bool returnValue = SqlValidation.Validate(badSqlScript, targets, errors);
                Assert.IsTrue(errors.Count > 0, "Errors should be returned for SQL: " + badSqlScript);
            }
        }
        
        [Test, Description("Test good scripts against the target validators")]
        public void TestGoodSqlSingle() {
            foreach (var goodSqlScript in GoodSqlScripts) {
                Assert.IsTrue(SqlValidation.Validate(goodSqlScript, targets, null), "Checking should pass for SQL: " + goodSqlScript);
            }            
        }

        [Test, Description("Test good scripts against all validators")]
        public void TestGoodSqlAll()
        {
            List<string> errors = new List<string>();
            errors.Add("");
            foreach (var goodSqlScript in GoodSqlScripts) {
                Assert.IsTrue(SqlValidation.Validate(goodSqlScript, errors), String.Format("Checking should pass for SQL: {0}\n{1} error(s)\nError[{1}]:  {2}",goodSqlScript, errors.Count()-1, errors[0]));
            }            
        }
    }
}