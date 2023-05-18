using System.Collections.Generic;

namespace UtilitiesVB
{


    public static class KeyValuePairEnumerableExtensions
    {
        /// <summary>
    /// Helper function used in conjunction with Linq query to enable a SortedDictionary to be filtered efficiently.
    /// Sample usage is Dim filtered_dict = source_dict.Where(Function(x) x.Value.foo = bar).ToSortedDictionary()
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="l">IEnumerable of a KeyValuePair from a dictionary</param>
    /// <returns>A Sorted Dictionary</returns>
        public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> l)
        {
            var result = new SortedDictionary<TKey, TValue>();

            foreach (var e in l)
                result[e.Key] = e.Value;

            return result;
        }

        /// <summary>
    /// Helper function used in conjunction with Linq query to enable a Dictionary to be filtered efficiently.
    /// Sample usage is Dim filtered_dict = source_dict.Where(Function(x) x.Value.foo = bar).ToDictionary()
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="l">IEnumerable of a KeyValuePair from a dictionary</param>
    /// <returns>A Sorted Dictionary</returns>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> l)
        {
            var result = new Dictionary<TKey, TValue>();

            foreach (var e in l)
                result[e.Key] = e.Value;

            return result;
        }
    }
}