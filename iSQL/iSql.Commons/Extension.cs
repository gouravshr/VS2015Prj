using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace iSql.Commons {
    /// <summary>
    /// Place to hold variosus extension methods.
    /// </summary>
    public static class Extension {

        public static string ToXml(this object obj) {
            // assume obj is NOT null, otherwise, runtime error will be thrown
           
            //NOTE: this is just a generic extension to use xml serializer, in real world, it is more complicted -- for instance, if you want to serialzie a DataTable without name, exception will be thrown
            using (MemoryStream ms = new MemoryStream()) {
                XmlSerializer ser = new XmlSerializer(obj.GetType());
                ser.Serialize(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }


        public static string ToCsv(this DataTable dt, bool hasHeader = true) {
            StringBuilder sb = new StringBuilder();
            if( hasHeader ) {
                bool firstCol = true;
                foreach (DataColumn col in dt.Columns) {
                    if (firstCol) {
                        firstCol = false;
                    } else {
                        sb.Append(",");
                    }
                    sb.Append(EscapeQuotes(col.ColumnName));
                }
                sb.Append(Environment.NewLine);
            }

            foreach (DataRow r in dt.Rows) {
                bool firstCol = true;
                foreach (object o in r.ItemArray) {
                    if( firstCol ) {
                        firstCol = false;
                    }else {
                        sb.Append(",");
                    }
                    // DBNull will return empty string as well...
                    sb.Append(EscapeQuotes(o.ToString()));
                }
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public static string ToCsv(this DataSet ds, bool hasHeader = true)
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataTable dt in ds.Tables)
            {
                if (hasHeader)
                {
                    sb.Append(dt.TableName);
                    sb.Append(Environment.NewLine);

                    bool firstCol = true;
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (firstCol)
                        {
                            firstCol = false;
                        }
                        else
                        {
                            sb.Append(",");
                        }
                        sb.Append(EscapeQuotes(col.ColumnName));
                    }
                    sb.Append(Environment.NewLine);
                }

                foreach (DataRow r in dt.Rows)
                {
                    bool firstCol = true;
                    foreach (object o in r.ItemArray)
                    {
                        if (firstCol)
                        {
                            firstCol = false;
                        }
                        else
                        {
                            sb.Append(",");
                        }
                        // DBNull will return empty string as well...
                        sb.Append(EscapeQuotes(o.ToString()));
                    }
                    sb.Append(Environment.NewLine);
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        public static string EscapeQuotes(string s ) {
            if(s.IndexOf("\"") >= 0 ) {
                // NOTE: for double quoting stuff, we should escape them in csv following MS csv dialect
                return  new StringBuilder().Append("\"").Append( s.Replace("\"", "\"\"") ).Append("\"").ToString();
            }else {
                return s;
            }
        }

    }
}
