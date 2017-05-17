using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace iSql.Commons.Test {
    [TestFixture]
    class TestExtension {
        [Test]
        public void TesttoXML()
        {
            List<string> SampleData = new List<string>(4);
            SampleData.Add("One");
            SampleData.Add("Two");
            SampleData.Add("Three");
            SampleData.Add("Four");

            string result = Extension.ToXml(SampleData);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);

            Assert.AreEqual(xDoc.ChildNodes[1].Name, "ArrayOfString");
            Assert.AreEqual(xDoc.ChildNodes[1].ChildNodes[0].InnerText, "One");
            Assert.AreEqual(xDoc.ChildNodes[1].ChildNodes[1].InnerText, "Two");
            Assert.AreEqual(xDoc.ChildNodes[1].ChildNodes[2].InnerText, "Three");
            Assert.AreEqual(xDoc.ChildNodes[1].ChildNodes[3].InnerText, "Four");
            
            //string expected = "<?xml version=\"1.0\"?>\r\n<ArrayOfString xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <string>One</string>\r\n  <string>Two</string>\r\n  <string>Three</string>\r\n  <string>Four</string>\r\n</ArrayOfString>";
            //Assert.AreEqual(expected, result);
        }

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
            sb.Append("1,"+DateTime.Parse("1/1/2011 12:00:00 AM").ToString()+",test string one").Append(Environment.NewLine);
            sb.Append("2,"+DateTime.Parse("9/12/2011 11:42:59 AM").ToString()+",\"yet another string with double string \"\" inside\"").Append(Environment.NewLine);

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
            sb.Append("1,"+DateTime.Parse("1/1/2011 12:00:00 AM").ToString()+",test string one").Append(Environment.NewLine);
            sb.Append("2," + DateTime.Parse("9/12/2011 11:42:59 AM").ToString() + ",\"yet another string with double string \"\" inside\"").Append(Environment.NewLine);
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
