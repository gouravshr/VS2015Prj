using System.Text.RegularExpressions;

namespace iSql.Validator
{
    internal class SqlValidator
    {
        public string title { get; set; }
        public string regexPattern { get; set; }
        /// <summary>
        /// If regexPattern matches and exceptRegexPattern does not match, then this is an invalid statement
        /// If regexPattern and exceptRegexPattern match, then this is considered valid code
        /// </summary>
        public string exceptRegexPattern { get; set; }
        public RegexOptions regexOptions { get; set; }
        public string errorMessage { get; set; }        

        /// <summary>
        /// Create a SqlValidator object.  If regexPattern is matched, the code is considered invalid.  If excepRegexPattern is also matched, then the code is considered valid.
        /// </summary>
        /// <param name="title">Unique Title</param>
        /// <param name="regexPattern">Pattern to match to check for invalid code.</param>
        /// <param name="exceptRegexPattern">If this matches, then the code is considered valid</param>
        /// <param name="regexOptions"></param>
        /// <param name="errorMessage"></param>
        public SqlValidator(string title, 
                            string regexPattern, 
                            string exceptRegexPattern, 
                            RegexOptions regexOptions, 
                            string errorMessage)
        {
            this.title = title;
            this.regexPattern = regexPattern;
            this.exceptRegexPattern = exceptRegexPattern;
            this.regexOptions = regexOptions;
            this.errorMessage = errorMessage;
        }
    }
}
