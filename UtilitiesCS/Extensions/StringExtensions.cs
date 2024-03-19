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
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);


        public static string[] Split(this string str, char separator, bool trim)
        {
            if (trim) { return str.Split(separator).Select(s => s.Trim()).ToArray(); } 
            else { return str.Split(separator); }
        }

        public static string[] Split(this string str,  string delimiter) 
        { return str.Split(new string[] { delimiter }, StringSplitOptions.None); }

        public static string[] Split(this string str, string delimiter, bool trim)
        {
            if (trim)
            {
                return str.Split(new string[] { delimiter }, StringSplitOptions.None)
                                  .Select(s => s.Trim())
                                  .ToArray();
            }
            else { return str.Split(new string[] { delimiter }, StringSplitOptions.None); }
        }

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

        /// <summary>
        /// Compare two strings and return the index of the first difference.  
        /// Return -1 if the strings are equal.
        /// </summary>
        /// <param name="s1">base string</param>
        /// <param name="s2">string to compare</param>
        /// <returns>integer index of 1st difference or -1 if equal</returns>
        public static int FirstDiffIndex(this string s1, string s2)
        {
            int index = 0;
            int min = Math.Min(s1.Length, s2.Length);
            while (index < min && s1[index] == s2[index])
                index++;

            return (index == min && s1.Length == s2.Length) ? -1 : index;
        }
        
        public static string PadToCenter(this string str, int totalWidth, char paddingChar = ' ')
        {
            if (str.Length >= totalWidth) { return str; }
            int padLength = totalWidth - str.Length;
            int padLeft = padLength / 2 + str.Length;
            return str.PadLeft(padLeft, paddingChar).PadRight(totalWidth, paddingChar);
        }
    }
}
