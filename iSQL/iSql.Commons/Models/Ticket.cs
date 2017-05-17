using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSql.Commons.Models {

    public class Ticket {
        public int TicketId { get; set; }

        [Required( ErrorMessage = "*")]
        public string Description { get; set; }

        [Required( ErrorMessage = "*")]
        public int ProjectId { get; set; }

        [MaxLength(50)]
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } 

        public virtual Project Project { get; set; }
        public virtual User CreatedBy{ get; set; }

        //Per latest request, it is no longer needed
        //NOTE: per Scott's request, ITG should be a required field.
        //[Required( ErrorMessage = "*")]
        public string ItgNumber { get; set; }

        public int? ProcessId { get; set; }
        public string LogFilePath { get; set; }
        // TODO: may go for lazy loading, etc. For now, workstates comments are always loaded... it is just used in overview page anyway  

        public virtual ICollection<WorkState> WorkStates { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}