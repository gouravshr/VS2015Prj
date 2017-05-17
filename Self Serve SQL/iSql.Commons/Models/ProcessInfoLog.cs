using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace iSql.Commons.Models {
    public class ProcessInfoLog {

        public string Type { get { return "ProcessInfoLog"; } }
        // business logic related
        public string AuthorizedBy { get; set; }
        public string LogFile { get; set; }
        public long LogFileSize { get; set; }

        public int ProcessId { get; set;  }

        // reason
        public string Description { get; set;  }

        // runtime execution info

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public int ExitCode { get; set; }

        //connection information
        public string DBServer { get; set; }
        public string DBName { get; set; }


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

        public static ProcessInfoLog LoadFromJson(string jsonStr) {
            JavaScriptSerializer js;
            try {
                js = new JavaScriptSerializer();
                return js.Deserialize<ProcessInfoLog>(jsonStr);
            } finally {
                js = null;
            }
        }
    }
}
