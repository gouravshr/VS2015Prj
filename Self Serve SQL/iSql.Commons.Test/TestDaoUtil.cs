#if DEBUG
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using iSql.Commons;

namespace iSql.Commons.Test
{
    [TestFixture]
    [Ignore("Relies on database to complete.")]
    public class TestDaoUtil
    {
        [SetUp]
        public void initializeConnection()
        {
            //System.Configuration.ConfigurationManager.ConnectionStrings["EntDbContext"].ConnectionString = "TestValue";
        }
    }
}

#endif