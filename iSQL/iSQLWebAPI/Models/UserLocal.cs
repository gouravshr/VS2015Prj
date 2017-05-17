using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSQL_WebAPI.Models
{
    public class UserLocal
    {
        public string UserId { get; set; }
        public string SystemRole { get; set; }
        public string PreferredEmailId { get; set; }
        public bool IsActive { get; set; }
    }
}