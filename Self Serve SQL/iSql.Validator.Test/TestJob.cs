using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestJob : TestValidateSqlBase {

        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "Job"
                         select val);
        }

        private string[] _BadSqlScripts = new string[] {

            "sp_create_job",
            "sp_update_job",
            "sp_delete_job",
            "//here is a comment sp_create_job",
            "//here is a comment\nsp_update_job",
            "/*here is a comment*/sp_delete_job/*another comment*/",
            "sp_delete_job@sql",
            "sp_create_job(@SQL)",
            "SP_CREATE_JOB",
            "SP_DELETE_JOB",
            "sp_512_job"
        };


        private string[] _GoodSqlScripts = new string[] {
                                                   
            "select sprocket_job from tbl_create_user where 1=1",
            "sp_who/*some comment*/This_Is_A_Job"
        };

    }
}