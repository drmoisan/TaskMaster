using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class ArrayExtensions
    {
        public static string[,] ToStringArray<T>(this T[,] array)
        {
            int rowCount = array.GetLength(0);
            int columnCount = array.GetLength(1);
            string[,] stringArray = new string[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    stringArray[i, j] = array[i, j].ToString();
                }
            }
            return stringArray;
        }
                
        public static string[] ToStringArray<T>(this T[] array)
        {
            int rowCount = array.Length;
            string[] stringArray = new string[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                stringArray[i] = array[i].ToString();
            }
            return stringArray;
        }

        public static IEnumerable<T> SliceRow<T>(this T[,] array, int row)
        {
            for (var i = 0; i < array.GetLength(1); i++)
            {
                yield return array[row, i];
            }
        }

        public static IEnumerable<T> SliceColumn<T>(this T[,] array, int column)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                yield return array[i, column];
            }
        }

        public static bool IsInitialized<T>(this T[,] array)
        {
            if (array == null) { return false; }
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j] == null) { return false; }
                }
            }

            return true;
        }

        public static bool IsInitialized<T>(this T[,] array, bool partially)
        {
            if (array == null) { return false; }
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j] != null) { return true; }
                }
            }
            return false;
        }

        public static bool IsInitialized<T>(this T[] array)
        {
            if (array == null) { return false; }
            for (int i = 0; i < array.GetLength(0); i++)
            {
                if (array[i] == null) { return false; }
            }

            return true;
        }

        public static bool IsInitialized<T>(this T[] array, bool partially)
        {
            if (array == null) { return false; }
            for (int i = 0; i < array.GetLength(0); i++)
            {
                if (array[i] != null) { return true; }
            }
            return false;
        }
                
        /// <summary>
        /// Function searches a string array and returns matching elements in a new string array.
        /// How matches are performed is defined by the search options. See 
        /// <see cref="StringSearchOptions"/>.
        /// </summary>
        /// <param name="sourceArray">String array to search</param>
        /// <param name="searchString">Target substring to search</param>
        /// <param name="options">Defines how matches are performed. 
        /// See also <see cref="StringSearchOptions"/>.</param>
        /// <returns>String array with elements that match the criteria. Null if no matches</returns>
        public static string[] SearchArry4Str(this string[] sourceArray, 
                                              string searchString = "", 
                                              SearchOptions options = SearchOptions.Standard)
        {
            if (searchString.Trim().Length != 0)
            {
                switch (options)
                {
                    case SearchOptions.Standard:
                        (Regex rg, string searchPattern) = SimpleRegex.MakeRegex(searchString);
                        return sourceArray.Where(x => rg.IsMatch(x)).ToArray();

                    case SearchOptions.Complement:
                        (rg, searchPattern) = SimpleRegex.MakeRegex(searchString);
                        return sourceArray.Where(x => !rg.IsMatch(x)).ToArray();

                    case SearchOptions.DeleteFromMatches:
                        (rg, searchPattern) = SimpleRegex.MakeRegex(searchString);
                        string replacePattern = SimpleRegex.MakeReplacePattern(searchPattern);
                        return sourceArray.Where(x => rg.IsMatch(x))
                                          .Select(x => rg.Replace(x, replacePattern))
                                          .ToArray();

                    case SearchOptions.ExactMatch:
                        return sourceArray.Where(x => x == searchString).ToArray();

                    case SearchOptions.ExactComplement:
                        return sourceArray.Where(x => x != searchString).ToArray();

                    default:
                        return sourceArray;
                }
            }
            return sourceArray;
        }

        /// <summary>
        /// Enumeration with search options.
        /// <list type="number">
        /// <listheader>
        ///     <term>Standard</term>
        ///     <description>Performs a simple regex search using a * or % as a wildcard</description>
        /// </listheader>
        /// <item>
        /// <term>Complement</term>
        /// <description>Elements that do NOT match the regex pattern will be returned</description>
        /// </item>
        /// <item>
        /// <term>DeleteFromMatches</term>
        /// <description>Similar to Standard except that the matching substring is removed from each 
        /// matching element</description>
        /// </item>
        /// <item>
        /// <term>ExactMatch</term>
        /// <description>Return elements that match the literal search string (case sensitive)</description>
        /// </item>
        /// <item>
        /// <term>ExactComplement</term>
        /// <description>Return elements that do Not match the literal search string (case sensitive)</description>
        /// </item>
        /// </list>
        /// </summary>
        public enum SearchOptions
        {
            Standard = 0,
            Complement = 1,
            DeleteFromMatches = 2,
            ExactMatch = 3,
            ExactComplement = 4
        }
    }
}
