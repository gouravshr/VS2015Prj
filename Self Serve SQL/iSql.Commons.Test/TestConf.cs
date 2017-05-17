using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace iSql.Commons.Test
{
    [TestFixture]
    class TestConf
    {
        Configuration config;

        [SetUp]
        //[Ignore("Unable to change configuration in the test code")]
        public void TestConfSetup()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = config.FilePath;
            config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            config.AppSettings.Settings.Clear();

            config.Save();
        }

        [Test]
        [RequiresThread] // Marking to require own thread.  Believe this will cause this test to reinitialize with no entries in the application configuration
        public void TestConstructor()
        {
            Conf sample = new Conf();

            Assert.IsNotNullOrEmpty(Conf.WorkingFolder, "WorkingFolder does not contain a value");
            Assert.AreEqual(0, Conf.MaximumLogFileSize, "MaximumLogFileSize should be 0 by default");
        }

        [Test]
        [Ignore("Unable to change configuration in the test code")]
        public void TestConstructorWithConfig()
        {
            config.AppSettings.Settings.Add("WorkingFolder", @"C:\WorkingFolder");
            config.AppSettings.Settings.Add("MaximumLogFileSize", "1024");
            config.Save();

            Conf.Reload();

            Assert.AreEqual(1024, Conf.MaximumLogFileSize, "MaximumLogFileSize value is incorrect.");
            Assert.AreEqual(@"C:\WorkingFolder", Conf.WorkingFolder, "Incorrect value for working folder.");
        }
    }
}
