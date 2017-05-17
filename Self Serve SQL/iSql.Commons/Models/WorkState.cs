using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSql.Commons.Models {
    public class WorkState {

        public int WorkStateId { get; set; }

        // flow state code 
        public int Code { get; set;  }
        // flow state description
        [MaxLength(1024)]
        public string Name { get; set; }

        //the last state should always be the latest state
        //public bool IsCurrentState { get; set; }

        //public string Status { get; set; }
        public string Note { get; set;  }

        // stage now will be determined from the code and the name now
        //public string Stage { get; set; }

        public string LogMessage  { get; set; }

        public DateTime CreatedAt { get; set; }

        // do we really need start and end time of reach state? we may add more states for aync later  
        // public DateTime StartedAt { get; set; }
        // public DateTime EndedAt { get; set; }

        public int TicketId { get; set; }
//        public virtual Ticket Ticket { get; set;}

        // visual display related properties, make it even simpler, use string
        [MaxLength(100)]
        public string Category { get; set; }

        /// <summary>
        /// Per requirement of reporting/auditing, we need to extract approver info out of LogMessage and put it in a dedicated field., this field could be blank if no approver information is needed.
        /// </summary>
        [MaxLength(50)]
        public string AuthorizedBy { get; set;  }

        /// <summary>
        /// Per the latest reqquirement, we need to keep history of execution history.
        /// NOTE: this field cannot be a FK field, because project db servers may be changed later on, and we don't need port information for audi purpose at all.
        /// </summary>
        [MaxLength(200)]
        public string DbServer { get; set; }

        [MaxLength(200)]
        public string DbName { get; set;  }

        [MaxLength(200)]
        public string DbType { get; set; }
    }
}