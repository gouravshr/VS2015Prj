using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iSql.Commons.Models
{
    public class Project {
        public int ProjectId { get; set; }

        [Required( ErrorMessage = "*")]
        [MaxLength(1024)]
        public string Name { get; set; }

        [Required( ErrorMessage = "*")]
        public string Description { get; set; }

        [Required( ErrorMessage = "*")] 
        [DataType(DataType.MultilineText)]
        [MaxLength(1024)]
        public string ContactInfo { get; set; }

        [Required( ErrorMessage = "*")]
        public bool IsActive { get; set; }

        //NOTE: for now, just add staging and producton DB connection informaiton here, and assume only one staging and one production server for each project
        //      in the future, we may extend to support multiple DBs -- we can either add more columns here or add one-to-many relationship

        [MaxLength(200)]
        public string StagingHost { get; set; }

        //NOTE: by default we want to assign the standard SQL Server port 
        private int _stagingPort = 1433;
        public int StagingPort { 
            get { return _stagingPort;  } 
            set { _stagingPort = value; } 
        }

        [MaxLength(200)]
        public string StagingDatabase { get; set; }

        [Required( ErrorMessage = "*")]
        [MaxLength(200)]
        public string ProductionHost { get; set; }

        private int _productionPort = 1433;
        [Required( ErrorMessage = "*")]
        public int ProductionPort {
            get { return _productionPort;  }
            set { _productionPort = value;  }
        }

        [Required( ErrorMessage = "*")]
        [MaxLength(200)]
        public string ProductionDatabase { get; set; }

        //TODO: this needs to be required field in the future, now we have to allow null until migration was done.
        [MaxLength(50)]
        public string AdGroupKeyName { get; set; }

        [Required(ErrorMessage = "*")]
        [MaxLength(10)]
        public string DatabaseType { get; set; }

        //NOTE: access is not required -- if you do make this virutal colleciton required, you may see EntityValidationErrors 
        public virtual ICollection<Access> Accesses { get; set; }
        public virtual ICollection<UserProjectRequest> AccessRequest { get; set; }
    }
}