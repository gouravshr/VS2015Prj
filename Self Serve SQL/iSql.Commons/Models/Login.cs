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
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
