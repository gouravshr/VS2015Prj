using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace iSql.Validator {

    public class SqlValidation
    {
        static internal List<SqlValidator> sqlValidators = new List<SqlValidator>
        {
            new SqlValidator("Drop", @"\bDROP\b", @"\bDROP\b.*\b(SYNONYM)\b", RegexOptions.IgnoreCase, "DROP statement is not allowed in SQL script."), //Note:  DROP is not allowed, explicitly allow DROP SYNONYM as an approved change.  The DROP SYNONYM must be on the same line.
            new SqlValidator("Truncate", @"\btruncate\b", "", RegexOptions.IgnoreCase, "TRUNCATE statement is not allowed in SQL script."),
            new SqlValidator("SpExecuteSql", @"\bsp_executesql\b", "", RegexOptions.IgnoreCase, "sp_executesql statement is not allowed in SQL script."),
            new SqlValidator("IsolationLevelSetting", @"\bISOLATION\b.*\bLEVEL\b", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, "ISOLATION LEVEL setting related statement is not allowed in SQL script."),
            new SqlValidator("Exec", @"\bEXEC(UTE)?\b", "", RegexOptions.IgnoreCase, "EXEC statement is not allowed in SQL script."),
            new SqlValidator("BackupRestore", @"\b(BACKUP|RESTORE)\b.*\bDATABASE\b", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, "BACKUP / RESTORE statements are not allowed in SQL script."),
            new SqlValidator("Use", @"\bUSE\b", "", RegexOptions.IgnoreCase, "USE statement is not allowed in SQL script."),
            new SqlValidator("Snapshot", @"\bSNAPSHOT\b", "", RegexOptions.IgnoreCase, "SNAPSHOT statement is not allowed in SQL script."),
            // File Changes are covered by Create Database
            // new SqlValidator("FileChanges", @"\bALTER\b.*\bDATABASE\b.*\bFILE\b", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, "Physical File Changes are not allowed in SQL script."),
            new SqlValidator("ResourceGovernor", @"\b(CREATE|ALTER|DROP)\b.*\b(RESOURCE|WORKLOAD)\b.*\b(POOL|GROUP|GOVERNOR)\b", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, "Resource Governor changes are not allowed in SQL script."),
            new SqlValidator("Authorization", @"\b(GRANT|REVOKE|DENY)\b", "", RegexOptions.IgnoreCase, "Security access changes are not allowed in SQL script."),
            new SqlValidator("Authentication", @"\b(GRANT|DENY|UPDATE|DELETE|ALTER|CREATE|DROP|ENABLE|DISABLE)\b.*\b(USER|LOGIN)\b", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, "Security access changes are not allowed in SQL script."),
            new SqlValidator("Trigger", @"\b(UPDATE|DELETE|ALTER|CREATE|DROP|ENABLE|DISABLE)\b.*\bTRIGGER\b", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, "ALTER, CREATE, DROP, ENABLE, DISABLE TRIGGER statements are not allowed in SQL script."),
            new SqlValidator("Job", @"\bsp_[\w]*_job.*\b", "", RegexOptions.IgnoreCase, "Job changes are not allowed"),
            new SqlValidator("Configuration", @"\b(sp_configure|RECONFIGURE)\b", "", RegexOptions.IgnoreCase, "Server configuration changes are not allowed"),
            new SqlValidator("SelectUpdateDeleteWhere", @"\b(SELECT|DELETE|UPDATE)\b", @"\b(SELECT|DELETE|UPDATE)\b.*\b(WHERE)\b", RegexOptions.IgnoreCase | RegexOptions.Singleline, "Must specify a WHERE condition when using SELECT, UPDATE or DELETE."),
            new SqlValidator("CreateDatabase", @"\b(CREATE|ALTER|DROP)\b.*\bDATABASE\b", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, "CREATE, ALTER, DROP DATABASE statements are not allowed in SQL script."),
            new SqlValidator("Alter", @"\bALTER\b", @"\bALTER\b.*\b(SYNONYM)\b", RegexOptions.IgnoreCase, "ALTER Table, Columns statements are not allowed in SQL script."),
            new SqlValidator("CreateTable", @"\b(CREATE|ALTER|DROP)\b.*\bTABLE\b", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, "CREATE, ALTER, DROP TABLE statements are not allowed in SQL script.")
        };

        public static bool Validate(string sqlStr, List<string> errors = null)
        {
            try
            {
                //Exclude comments inside /* .... */ from scripts
                while (sqlStr.IndexOf("/*") != -1)
                {
                    sqlStr = sqlStr.Replace(sqlStr.Substring(sqlStr.IndexOf("/*"), (sqlStr.IndexOf("*/") - sqlStr.IndexOf("/*")) + 2), "");
                }

                //Exclude comments started with -- from scripts
                while (sqlStr.IndexOf("--") != -1)
                {
                    sqlStr = sqlStr.Replace(sqlStr.Substring(sqlStr.IndexOf("--"), (sqlStr.IndexOf("\n", sqlStr.IndexOf("--")) - sqlStr.IndexOf("--")) + 1), "");
                }

                //Temporarily excluded Alter session statement from the script before sending it for auto validation
                sqlStr = Regex.Replace(sqlStr, "ALTER SESSION", "", RegexOptions.IgnoreCase);

                //Temporarily excluded backup table procedure statement from the script before sending it for auto validation
                sqlStr = Regex.Replace(sqlStr, "exec create_backup_table", "", RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                //TODO : Should be logged the exception?
            }
            return Validate(sqlStr, sqlValidators, errors);
        }

        //This function allows us to test an individual SqlValidator.
        internal static bool Validate(string sqlStr, List<SqlValidator> restrictedList, List<string> errors = null)
        {
            bool returnValue = true;
            foreach(SqlValidator check in restrictedList)
            {
                if (Regex.Matches(sqlStr, check.regexPattern, check.regexOptions).Count > 0)
                {
                    //The initial match was found, now let's check the exception condition if it exists
                    if (check.exceptRegexPattern.Length > 0 &&
                        Regex.Matches(sqlStr, check.exceptRegexPattern, check.regexOptions).Count > 0
                        )
                    {
                        //The exception was found, do nothing.
                    }
                    else
                    { 
                        //Exception was not found, so this is an error
                        returnValue = false;
                        if (errors != null)
                        {
                            errors.Add(check.errorMessage);
                        }
                    }
                }
            }
            return returnValue;
        }
    }
}