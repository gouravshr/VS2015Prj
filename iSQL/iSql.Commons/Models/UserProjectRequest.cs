
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iSql.Commons.Models
{
    public class UserProjectRequest
    {
        [Key, Column(Order = 0)]
        [Required]
        public int RequestId { get; set; }
        [Key, Column(Order = 1)]
        [Required]
        public int ProjectId { get; set; }

        public virtual UserAccessRequest Request { get; set; }
        public virtual Project Project { get; set; }

        public bool IsSelected { get; set; }
    }
}
