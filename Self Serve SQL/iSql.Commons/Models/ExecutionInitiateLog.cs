using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace iSql.Commons.Models {
    public class ExecutionInitiateLog {

        // business logic related
        public string Type { get { return "ExecutionInitiateLog"; } }
        public string AuthorizedBy { get; set; }
        public DateTime StartTime { get; set; }
        public string DBServer { get; set; }
        public string DBName { get; set; }
        public string Description { get; set; }

        public string ToJsonString() {
            //NOTE: for now just use built-in .NET JavaScriptSerializer
            JavaScriptSerializer js;
            try {
                js = new JavaScriptSerializer();
                return js.Serialize(this);
            } finally {
                js = null;
            }
        }

        public static ExecutionInitiateLog LoadFromJson(string jsonStr) {
            JavaScriptSerializer js;
            try {
                js = new JavaScriptSerializer();
                return js.Deserialize<ExecutionInitiateLog>(jsonStr);
            } finally {
                js = null;
            }
        }
    }
}
