using System.ComponentModel.DataAnnotations;

namespace iSql.Commons.Models
{
    public class Access {
        public int AccessId { get; set; }
        [MaxLength(30)]
        public string Role { get; set; }

        [MaxLength(50)]
        public string UserId { get; set; }
        public int ProjectId { get; set; } 

        public virtual User User { get; set; }
        public virtual Project Project { get; set; }
    }
}