using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace iSql.Commons.Test {
    [TestFixture]
    class TestExtension {
        [Test]
        public void TestToCsvDT() {
            DataTable dt = new DataTable();
            dt.Columns.Add("TestColumn1", typeof(int));
            dt.Columns.Add("TestColumn2", typeof(DateTime));
            dt.Columns.Add("TestColumn3", typeof(String));

            dt.Rows.Add(1, DateTime.Parse("2011-01-01"), "test string one");
            dt.Rows.Add(2, DateTime.Parse("9/12/2011 11:42:59 AM"), "yet another string with double string \" inside");

            string csvStr = dt.ToCsv();
            StringBuilder sb = new StringBuilder();
            sb.Append("TestColumn1,TestColumn2,TestColumn3").Append(Environment.NewLine);
            sb.Append("1,1/1/2011 12:00:00 AM,test string one").Append(Environment.NewLine);
            sb.Append("2,9/12/2011 11:42:59 AM,\"yet another string with double string \"\" inside\"").Append(Environment.NewLine);

            string targetStr  = sb.ToString();
            
            Assert.IsTrue( csvStr.Equals(targetStr), "generated csv strings should equal: \n target string:\n" + targetStr + "\ngenerated csv:\n" + csvStr);
        }

        [Test]
        public void TestToCsvDS()
        {
            DataSet ds = new DataSet();

            DataTable dt = new DataTable();
            dt.Columns.Add("TestColumn1", typeof(int));
            dt.Columns.Add("TestColumn2", typeof(DateTime));
            dt.Columns.Add("TestColumn3", typeof(String));

            dt.Rows.Add(1, DateTime.Parse("2011-01-01"), "test string one");
            dt.Rows.Add(2, DateTime.Parse("9/12/2011 11:42:59 AM"), "yet another string with double string \" inside");

            dt.TableName = "FirstTable";
            ds.Tables.Add(dt);

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("TestColumn1", typeof(String));

            dt2.Rows.Add("Some Data");
            dt2.Rows.Add("Some More Data");

            dt2.TableName = "SecondTable";
            ds.Tables.Add(dt2);

            string csvStr = ds.ToCsv();
            StringBuilder sb = new StringBuilder();
            sb.Append("FirstTable").Append(Environment.NewLine);
            sb.Append("TestColumn1,TestColumn2,TestColumn3").Append(Environment.NewLine);
            sb.Append("1,1/1/2011 12:00:00 AM,test string one").Append(Environment.NewLine);
            sb.Append("2,9/12/2011 11:42:59 AM,\"yet another string with double string \"\" inside\"").Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("SecondTable").Append(Environment.NewLine);
            sb.Append("TestColumn1").Append(Environment.NewLine);
            sb.Append("Some Data").Append(Environment.NewLine);
            sb.Append("Some More Data").Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            string targetStr = sb.ToString();

            Assert.IsTrue(csvStr.Equals(targetStr), "generated csv strings should equal: \n target string:\n" + targetStr + "\ngenerated csv:\n" + csvStr);
        }
    }
}
