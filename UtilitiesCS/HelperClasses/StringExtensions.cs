using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class StringExtensions
    {
        public static string[] Split(this string str, char separator, bool trim)
        {
            if (trim) { return str.Split(separator).Select(s => s.Trim()).ToArray(); } 
            else { return str.Split(separator); }
        }

        public static string[] Tokenize(this string doc)
        {
            var _regex = new Regex(@"\b\w\w+\b");

            return _regex.Matches(doc).Cast<Match>().Select(x => x.Value.ToLower()).ToArray();
        }
    }
}
