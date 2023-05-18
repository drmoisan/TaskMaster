using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToDoModel.Test
{
    internal static class TempClass
    {
        internal static string MakeSearchPattern(string searchString)
        {
            string searchPattern = searchString.Replace("%", "*");
            if (searchPattern.Contains("*"))
            {
                searchPattern = string.Join("(.*)",
                                            searchPattern.Split('*')
                                            .Select(x => Regex.Escape(x)));
            }
            searchPattern = "^" + searchPattern + "$";
            return searchPattern;
        }

        internal static string MakeReplacePattern(string searchPattern)
        {
            int groupNum = searchPattern.Count(x => (x == '*'));
            string replacePattern = string.Join("", (from number in Enumerable.Range(1, groupNum)
                                                     select string.Concat("$", number.ToString())));
            return replacePattern;
        }

        internal static (Regex rg, string searchPattern) MakeRegex(string searchString)
        {
            string searchPattern = MakeSearchPattern(searchString);
            Regex rg = new Regex(searchPattern, RegexOptions.IgnoreCase);

            return (rg, searchPattern);
        }

    }
}
