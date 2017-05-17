#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace iSql.Commons.Test {
    [TestFixture]
    class TestFlowState {
        
        [Test]
        public void TestStage() { 
            //TODO: test special stages

            // Test init stage 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.TicketInitialized.Code)); 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.FileUploadStarted.Code)); 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.FileUploadFailed.Code)); 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.FileUploadSucceeded.Code)); 
            
            Assert.IsTrue( FlowState.IsInitStage( FlowState.ValidationStarted.Code)); 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.ValidationSucceeded.Code)); 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.ValidationFailed.Code)); 

            
            Assert.IsTrue( FlowState.IsInitStage( FlowState.DBAOverwriteWaitForDecision.Code)); 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.DBAOverwriteRequested.Code)); 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.DBAOverwriteRejected.Code)); 
            Assert.IsTrue( FlowState.IsInitStage( FlowState.DBAOverwriteApproved.Code)); 

            // test staging stage
            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageExecutionStarted.Code));
            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageExecutionSucceeded.Code));
            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageExecutionFailed.Code));

            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageRollbackStarted.Code));
            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageRollbackSucceeded.Code));
            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageRollbackFailed.Code));

            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageExecutionValidationWaiting.Code));
            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageExecutionValidationConfirmed.Code));
            Assert.IsTrue( FlowState.IsStagingStage( FlowState.StageExecutionValidationWaiting.Code)); 
            
            // test prod stage
            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductExecutionStarted.Code));
            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductExecutionSucceeded.Code));
            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductExecutionFailed.Code));
           

            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductRollbackStarted.Code));
            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductRollbackSucceeded.Code));
            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductRollbackFailed.Code));


            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductExeuctionValidationWaiting.Code));
            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductExecutionValidationApproved.Code));
            Assert.IsTrue( FlowState.IsProductionStage( FlowState.ProductExecutionValidationRejected.Code));


            // speical working flow stage -- may apply to any stage
            Assert.IsTrue( FlowState.IsSpeicalStage( FlowState.FlowTerminatedOnError.Code ));
            Assert.IsTrue( FlowState.IsSpeicalStage( FlowState.FlowTerminatedNormally.Code ));

        }


        [Test] 
        public void TestNoDuplicatedCode( ) {
            //var props = typeof (FlowState).GetMembers();
            var props = typeof (FlowState).GetFields();
            var q = from f in props where f.FieldType == typeof (State) select ((State) f.GetValue(null)).Code;

            // identify which code may not be uniq 
            var codeHash = new Dictionary<int, bool>(); // we don't really need the value, so just make it very light weight one  
            
            q.ToList().ForEach(c => { Assert.IsFalse( codeHash.Keys.Contains( c ) , "Code should be unique: " + c); codeHash.Add( c, true); }  )  ; 
        }

        [Test] 
        public void TestNoDuplicatedDescription( ) {
            //var props = typeof (FlowState).GetMembers();
            var props = typeof (FlowState).GetFields();
            var q = from f in props where f.FieldType == typeof (State) select ((State) f.GetValue(null)).Description;

            // identify which code may not be uniq 
            var codeHash = new Dictionary<string, bool>(); // we don't really need the value, so just make it very light weight one  
            
            q.ToList().ForEach(s => { Assert.IsFalse( codeHash.Keys.Contains( s ) , "Description should be unique: " + s); codeHash.Add( s, true); }  )  ; 
        }

        /// <summary>
        ///  This dumb test case is not really intented to be a useful test case, but provide a simple way to get the generated sql.
        /// </summary>
        [Test]
        public void TestGenLookUpTableSql() {
            string creationSql = FlowState.GenLookUpTableSql(true);
            Assert.IsTrue( creationSql.Contains("CREATE TABLE"), "Table definiation sql should be generated.");

            Console.Out.WriteLine(creationSql);
            
            string insertSql = FlowState.GenLookUpTableSql(true);
            Assert.IsTrue( insertSql.Contains("CREATE TABLE"), "Table definiation sql should be NOT generated, just insert script should be generated");
        }

        [Test]
        public void CreateStateObject()
        {
            iSql.Commons.State data = new iSql.Commons.State(1, "New Data");

            Assert.AreEqual(1, data.Code, "Code is not the same");
            Assert.AreEqual("New Data", data.Description, "Description is not the same");
        }

        [Test]
        public void CheckStageInit()
        {
            Assert.IsTrue(iSql.Commons.FlowState.IsInitStage(10001));
            Assert.IsTrue(iSql.Commons.FlowState.IsInitStage(30000));
            Assert.IsFalse(iSql.Commons.FlowState.IsInitStage(10000));
            Assert.IsFalse(iSql.Commons.FlowState.IsInitStage(30001));
            Assert.IsTrue(iSql.Commons.FlowState.IsInitStage( FlowState.DBAOverwriteRequested.Code));
        }

        [Test]
        public void CheckStageStage()
        {
            Assert.IsTrue(iSql.Commons.FlowState.IsStagingStage(30001));
            Assert.IsTrue(iSql.Commons.FlowState.IsStagingStage(50000));
            Assert.IsFalse(iSql.Commons.FlowState.IsStagingStage(30000));
            Assert.IsFalse(iSql.Commons.FlowState.IsStagingStage(50001));
            Assert.IsFalse(iSql.Commons.FlowState.IsStagingStage(FlowState.DBAOverwriteRejected.Code));
        }

        [Test]
        public void CheckProdStage()
        {
            Assert.IsTrue(iSql.Commons.FlowState.IsProductionStage(50601));
            Assert.IsFalse(iSql.Commons.FlowState.IsProductionStage(FlowState.DBAOverwriteApproved.Code));
        }

    }
}

#endif
