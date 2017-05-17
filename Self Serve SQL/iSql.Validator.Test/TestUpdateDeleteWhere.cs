using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Validator.Test {
    [TestFixture]
    class TestSelectUpdateDeleteWhere : TestValidateSqlBase
    {
     


        public override void InitSqlTestingScripts() {

            BadSqlScripts.AddRange(_BadSqlScripts);
            GoodSqlScripts.AddRange(_GoodSqlScripts);

            enTargets = (from val in SqlValidation.sqlValidators
                         where val.title == "SelectUpdateDeleteWhere"
                         select val);
        }

        private static string[] _BadSqlScripts = new string[] {

            // ------------------------------------------------------------------------
            // Real world multiline drop query 
            // ------------------------------------------------------------------------
            @"
                UPDATE  Table
                SET     Field = 1
            ", 

             @"
                DELETE FROM Table
             ", 

            //NOTE: questionalble drop trigger.... since it is such a common task during massive update, insesrt, etc
             @"
                update  Table
                set     Field = 1
             ",

            //NOTE: for now, we consider drop user scripts are bad scripts 
            @"
                delete from Table
             ",
                    //NOTE: for now, we consider select without Where is bad script 
             @"
                select * from Table
             ",
                    //NOTE: for now, we consider drop user scripts are bad scripts 
             @"
                select Field from Table
             ",
        };

        private static string[] _GoodSqlScripts = new string[] {
            @"
                UPDATE  Table
                SET     Field = 1
                WHERE   ID = 2
            ", 

             @"
                DELETE FROM Table
                WHERE   ID = 2
             ", 

             @"
                update  Table
                set     Field = 1
                where   ID = 2
             ",

            @"
                delete from Table
                where   ID = 2
             ",
              //Select Example
              @"
                Select * from Table
                where   ID = 2
             ",
              //Real world example
            @"
                UPDATE sacer.QuestionAnswerChoice
                SET AnswerChoiceTxt = 'Project Management for Program (e.g. more than one related project)'
                WHERE QuestionAnswerChoiceId IN (483, 355)

                UPDATE sacer.QuestionAnswerChoice
                SET AnswerChoiceTxt = 'Non-ADM compliant estimator'
                WHERE AnswerChoiceTxt = 'Non- ADM compliant estimator'

                UPDATE sacer.QuestionAnswerChoice
                SET AnswerChoiceTxt = 'Uses Standard SLA/OLA'
                WHERE AnswerChoiceTxt = 'Using Standard SLA/ OLA '

                UPDATE sacer.Question
                SET QuestionTxt = '1.10) Select the geographical profile for this Solution.'
                WHERE QuestionTxt = '1.10) Select the geographical profile for this Solution'

                UPDATE sacer.Question
                SET QuestionTxt = '1.9) Select the geographical profile for this Solution.'
                WHERE QuestionTxt = '1.9) Select the geographical profile for this Solution'
        
                 SELECT Questiontxt FROM sacer.Question
                 WHERE QuestionTxt = '1.9) Select the geographical profile for this Solution'
            "
        };

    }
}