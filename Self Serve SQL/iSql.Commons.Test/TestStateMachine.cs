using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using iSql.Commons;

namespace iSql.Commons.Test
{
    [TestFixture]
    [Ignore("All tests rely on database connection to actually save state change.  This throws error.  Need to determine how to test without using the database.  Ideally, look at the workflow state struct returned")]
    class TestStateMachine
    {

        #region security

        [Test]
        public void TestHasAccessToTicket() {
            // for sure we know no enterprise id like this one below
            string noSuchId = "__________NO_SUCH_ID_SHOULD_EXIST________________";
            string access1 = StateMachine.HasAccessToTicket(1, noSuchId);
            Assert.IsNull(access1);
        }

        [Test]
        public void TestIsCreator(){}

#endregion

#region special states
        [Test]
        public void TestAddTerminateOnErrorState()
        {
            Assert.IsTrue(StateMachine.AddTerminateOnErrorState(1, "Test Add Terminate On Error State"));
        }

        [Test]
        public void TestAddTerminateNormallyState(){}

        [Test]
        public void TestAddUserAbortState(){}

#endregion

#region init stage

        [Test]
        public void TestAddFileUploadedState(){}

        [Test]
        public void TestAddValidationSucceedState(){}

        [Test]
        public void TestAddValidationFailedStat(){}

#endregion

#region staging state

        [Test]
        public void TestAddStageWaitForPushState(){}

        [Test]
        public void TestAddPushToStagingStartedState(){}

        [Test]
        public void TestAddPushToStagingSucceedState(){}

        [Test]
        public void TestAddPushToStagingFailedState(){}

        [Test]
        public void TestAddPushToStagingRollbackStartedState(){}

        [Test]
        public void TestAddPushToStagingRollbackSucceedState(){}

        [Test]
        public void TestAddPushToStagingRollbackFailedState(){}

        [Test]
        public void TestAddStagingExecutionValidationWaitingState(){}

        [Test]
        public void TestAddStagingExecutionValidationConfirmedState(){}

        [Test]
        public void TestAddStagingExecutionValidationRejectedState(){}

        [Test]
        public void TestAddStagingRollbackOnRejectionStartState(){}

        [Test]
        public void TestAddStagingrollbackOnRejectionSucceededState(){}

        [Test]
        public void TestAddStagingRollbackOnRejectionFailedState(){}

#endregion

#region prod stage

        [Test]
        public void TestAddProductWaitForPush() { }

        [Test]
        public void TestAddPushToProductionStartedState() { }

        [Test]
        public void TestAddPushToProductionSucceedState() { }

        [Test]
        public void TestAddPushToProductionFailedState() { }

        [Test]
        public void TestAddPushToProductionRollbackStartedState() { }

        [Test]
        public void TestAddPushToProductionRollbackSucceedState() { }

        [Test]
        public void TestAddPushToProductionRollbackFailedState() { }

        [Test]
        public void TestAddPushToProductionStagingRollbackStartedState() { }

        [Test]
        public void TestAddPushToProductionStagingRollbackSucceedState() { }

        [Test]
        public void TestAddPushToProductionStagingRollbackFailedState() { }

        [Test]
        public void TestAddProdExecutionValidationWaitingState() { }

        [Test]
        public void TestAddProdExecutionValidationConfirmedState() { }

        [Test]
        public void TestAddProdExecutionValidationRejectedState() { }

        [Test]
        public void TestAddProdRollbackOnProdRejectionStartState() { }

        [Test]
        public void TestAddProdRollbackOnProdRejectionSucceededState() { }

        [Test]
        public void TestAddProdRollbackOnProdRejectionFailedState() { }

        [Test]
        public void TestAddStagingRollbackOnProdRejectionStartState() { }

        [Test]
        public void TestAddStagingRollbackOnProdRejectionSucceededState() { }

        [Test]
        public void TestAddStagingRollbackOnProdRejectionFailedState() { }

#endregion
    }
}
