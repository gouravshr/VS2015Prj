using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iSql.Commons.Models
{
    public class UserAccessRequest
    {
        [Key, Column(Order = 0)]
        [Required]
        public int RequestId { get; set; }
        [Required]
        [MaxLength(30)]
        public string FirstName { get; set; }
        [MaxLength(30)]
        public string LastName { get; set; }
        [MaxLength(100)]
        public string UserId { get; set; }
        [Required]
        [MaxLength(100)]
        public string PreferredEmailId { get; set; }
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        public virtual ICollection<UserProjectRequest> AccessRequest { get; set; }
    }
}
