using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using iSql.Shell;

namespace iSql.Shell.Test {
    [TestFixture]
    class TestExecutor {

        #region  init 
        [SetUp]
        public void SetUp()
        {
            if (!_initFlag) {
                CleanUpLogFiles();
                _initFlag = true; 
            }
        }

        protected bool _initFlag;
        protected static string  _currenctDir = Directory.GetCurrentDirectory();

        private void CleanUpLogFiles()
        {
            foreach( var f in Directory.GetFiles(_currenctDir, "unit-test-*.log") ) { 
                Console.Out.WriteLine("\n -- Delete old test log file : " + f )  ; 
                File.Delete(f); 
            }

            // try to get those file again, and they must be deleted
            Assert.IsTrue( Directory.GetFiles(_currenctDir, "unit-test-*.log").Length == 0 );
        }
        #endregion 

        [Test]
        public void TestExecute() {

            Process p = null; 

            //this will cause sql syntax errors, we know that
            try {
                p = Executor.ExecuteSqlCmd(
                        " -o unit-test-1.log -b -Q \"select from Some_weird_table_that_should_NotExist\"  ");

                Assert.IsTrue(p.ExitCode != 0, "Exit code should be non-zero value");
                p.Dispose();

                var targetLogFile = Path.Combine(_currenctDir, "unit-test-1.log");
                // we are sure such file should now exist, even the bad query caused the failure of sqlcmd
                Assert.IsTrue(File.Exists(targetLogFile), "Unit testing log file unit-test-1.log should exist.");

                // And let's take a peek
                string logContent = File.OpenText(targetLogFile).ReadToEnd();
                Assert.IsNotNull(logContent);
                Assert.IsTrue(logContent.Contains("Incorrect syntax near the keyword"));
            } finally {
               if( p!=null ) { p.Dispose();} 
            }
        }

    }
}