using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace iSql.Commons.Models {
    public class Comment {

        public int CommentId { get; set; }

        //NOTE: We still want to limit the amount of data that user can put into comments at one time -- I dont' want to make it nvarchar(max)
        [Required( ErrorMessage = "*")]
        [DataType(DataType.MultilineText)]
        [MaxLength(1024)]
        public string Text { get; set; }

        
        [Required( ErrorMessage = "*")]
        [MaxLength(50)]
        public string ByUser { get; set; }

        public DateTime CreatedAt { get; set; }

        //we may optionally want to let user know which workstate they are working on 
        public int WorkStateCode { get; set; }
        public string WorkStateName { get; set; }


        // need to associate with Ticket anyway
        public int TicketId { get; set; }
    }
}
