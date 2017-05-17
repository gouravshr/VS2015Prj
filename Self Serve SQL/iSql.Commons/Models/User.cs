using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSql.Commons.Models {
    public class User {
        // NOTE: It could be called EnterpriseId , but if we do that, we then break the code-first convention, and have to do a lot of work around,  
        //       such as the [Key] attribute, or direct setting in the DbContext level, etc, to make it work.  So I decide to follow the convention
        //       and change the primary key name.  Another solution is,  you use pseudo pk, and keep EnterpriseId as separate property, that's fine 
        //       but it may make relationship mappting complicated as well.

        //NOTE: go with the MaxLength attribute coming from EF4.1, and not the old StringLength atrribute 
        [MaxLength(50)]
        public string UserId{ get; set; }

        //NOTE: users may have different roles -- most of them are Project based, such as commiiter and approver, etc, we'll model that kind of roles
        //      in Access. The system role is basically indicate the login user has this Web App's admin rights or not. Yes, we can create another class,
        //      and make it only points to the ID, etc, but for our simple needs, why bother?
        [MaxLength(30)]
        public string SystemRole { get; set; }

        [MaxLength(100)]
        public string PreferredEmailId { get; set; }

        public bool IsActive { get; set;  }

        public virtual ICollection<Access> Accesses { get; set; }
    }
}