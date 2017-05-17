using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSql.Commons.Models
{
    public class Login
    {
        public int LoginId { get; set; }
        [MaxLength(50)]
        public string LoginName { get; set; }

        [MaxLength(30)]
        public string Password { get; set; }
    }
}
