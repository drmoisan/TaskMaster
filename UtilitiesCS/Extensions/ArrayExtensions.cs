using System;
using System.Collections;
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

        public static string[,] ToStringArray<T>(this T[,] array, string nullReplacement)
        {
            int rowCount = array.GetLength(0);
            int columnCount = array.GetLength(1);
            string[,] stringArray = new string[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    if (array[i, j] is null) 
                    {
                        stringArray[i, j] = nullReplacement;
                    }
                    else
                    {
                        stringArray[i, j] = array[i, j].ToString();
                    }
                }
            }
            return stringArray;
        }

        public static string[] ToStringArray<T>(this T[] array, string nullReplacement)
        {
            int rowCount = array.Length;
            string[] stringArray = new string[rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                if(array[i] is null)
                {
                    stringArray[i] = nullReplacement;
                }
                else
                {
                    stringArray[i] = array[i].ToString();
                }
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

        public static string FlattenStringTree(this object[] branches, bool strictValidation = true)
        {
            if (!Array.TrueForAll(branches, branch => branch is string))
            {
                for (int i = 0; i < branches.Length; i++)
                {
                    if (branches[i] is Array) { branches[i] = FlattenStringTree((object[])branches[i]); }
                    else if (!(branches[i] is string))
                    {
                        if (strictValidation)
                        { throw new ArgumentException($"branches[{i}] is of type {branches[i].GetType().Name}"
                                + $". Array elements in FlattenStringTree must be arrays or strings."); }
                        branches[i] = "Error";
                    }
                }
            }
            string result = string.Join(", ", branches);
            if (result.Contains("Error")) { result = "Error"; }
            
            return result;
        }

        internal static bool IsStringArrayTree(this object[] branches, bool strictValidation)
        {
            return false;
        }

        //
        // Summary:
        //     Casts the elements of an System.Collections.IEnumerable to the specified type.
        //
        // Parameters:
        //   source:
        //     The System.Collections.IEnumerable that contains the elements to be cast to type
        //     TResult.
        //
        // Type parameters:
        //   TResult:
        //     The type to cast the elements of source to.
        //
        // Returns:
        //     An System.Collections.Generic.IEnumerable`1 that contains each element of the
        //     source sequence cast to the specified type.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        //
        //   T:System.InvalidCastException:
        //     An element in the sequence cannot be cast to type TResult.
        public static IEnumerable<TResult> CastNullSafe<TResult>(this IEnumerable source)
        {
            IEnumerable<TResult> enumerable = source as IEnumerable<TResult>;
            if (enumerable != null)
            {
                return enumerable;
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return CastIteratorNullSafe<TResult>(source);
        }

        private static IEnumerable<TResult> CastIteratorNullSafe<TResult>(IEnumerable source)
        {
            foreach (object item in source)
            {
                if (item is null)
                {
                    yield return default(TResult);
                }
                else { yield return (TResult)item; }
            }
        }


    }


}
