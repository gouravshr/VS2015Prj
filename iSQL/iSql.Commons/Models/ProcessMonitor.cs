using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSql.Commons.Models {
    class ProcessMonitor {
        public int ProjectId { get; set; }         
        public string UserId { get; set; }         
        public int StateCode { get; set; }
        public string LockedServer { get; set;  }
        public bool Locked { get; set;  }
        public DateTime LockedAt { get; set; }         
        public DateTime UnlockedAt { get; set; }         
    }
}
