using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static UtilitiesCS.ArrayExtensions;

namespace UtilitiesCS
{
    public static class StringExtensions
    {
        public static string[] Split(this string str, char separator, bool trim)
        {
            if (trim) { return str.Split(separator).Select(s => s.Trim()).ToArray(); } 
            else { return str.Split(separator); }
        }

        public static string[] Split(this string str,  string delimiter) 
        { return str.Split(new string[] { delimiter }, StringSplitOptions.None); }


        /// <summary>
        /// Function finds all matching substrings within a delimited string and returns 
        /// them in a new delimited string. How matches are performed is defined by the
        /// search options. See also <seealso cref="StringSearchOptions"/>.
        /// </summary>
        /// <param name="sourceString">A comma delimited string that will be searched</param>
        /// <param name="searchString">Target substring to find</param>
        /// <param name="delimiter">String used as delimiter</param>
        /// <param name="options">Defines how matches are performed. 
        /// See also <seealso cref="StringSearchOptions"/>.</param>
        /// <returns></returns>
        public static string SearchDelimitedString(this string sourceString,
                                                   string searchString,
                                                   string delimiter,
                                                   ArrayExtensions.SearchOptions options = ArrayExtensions.SearchOptions.Standard)
        {
            string[] sourceArray = sourceString.Split(new string[] { delimiter }, StringSplitOptions.None);
            string[] filteredArray = sourceArray.SearchArry4Str(searchString, options);
            return string.Join(delimiter, filteredArray);
        }

    }
}
