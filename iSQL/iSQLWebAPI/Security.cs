using iSql.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iSQLWebAPI
{
    public class Security
    {

        public static bool Login(string userName, string password)
        {
            using (EntDbContext sql = new EntDbContext())
            {
                return sql.Logins.Any(lg => lg.LoginName.Equals(userName, StringComparison.OrdinalIgnoreCase) && lg.Password == password);
            }
        }
    }
}