using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class DictionaryExtensions
    {
        public static bool ContentEquals<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDictionary)
        {
            return (otherDictionary ?? new Dictionary<TKey, TValue>())
                .OrderBy(kvp => kvp.Key)
                .SequenceEqual((dictionary ?? new Dictionary<TKey, TValue>())
                                   .OrderBy(kvp => kvp.Key));
        }

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
            IEnumerable<IGrouping<TKey, KeyValuePair<TKey, TValue>>> duplicateKVPsByKey = l.GroupBy(kvp => kvp.Key).Where(g => g.Count() > 1);
            if (duplicateKVPsByKey.Any())
                throw new InvalidOperationException($"Cannot convert to dictionary with duplicate keys: {string.Join(",", duplicateKVPsByKey)}");

            var result = new Dictionary<TKey, TValue>();
            foreach (var e in l)
                result[e.Key] = e.Value;

            return result;
        }

        public static SortedDictionary<K, V> ToSortedDictionary<K, V>(this Dictionary<K, V> existing)
        {
            return new SortedDictionary<K, V>(existing);
        }

        public static SortedDictionary<string, bool> SearchSortedDictKeys(SortedDictionary<string, bool> source_dict, string search_string)
        {

            var filtered_cats = (from x in source_dict
                                 where x.Key.Contains(search_string)
                                 select x).ToDictionary(x => x.Key, x => x.Value);
            return new SortedDictionary<string, bool>(filtered_cats);
        }

    }
    
    
    //public class DictionaryComparer<TKey, TValue> :
    //IEqualityComparer<Dictionary<TKey, TValue>>
    //{
    //    private IEqualityComparer<TValue> valueComparer;
    //    public DictionaryComparer(IEqualityComparer<TValue> valueComparer = null)
    //    {
    //        this.valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
    //    }
    //    public bool Equals(Dictionary<TKey, TValue> x, Dictionary<TKey, TValue> y)
    //    {
    //        if (x.Count != y.Count)
    //            return false;
    //        if (x.Keys.Except(y.Keys).Any())
    //            return false;
    //        if (y.Keys.Except(x.Keys).Any())
    //            return false;
    //        foreach (var pair in x)
    //            if (!valueComparer.Equals(pair.Value, y[pair.Key]))
    //                return false;
    //        return true;
    //    }

    //    public int GetHashCode(Dictionary<TKey, TValue> obj)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class ListComparer<T> : IEqualityComparer<List<T>>
    //{
    //    private IEqualityComparer<T> valueComparer;
    //    public ListComparer(IEqualityComparer<T> valueComparer = null)
    //    {
    //        this.valueComparer = valueComparer ?? EqualityComparer<T>.Default;
    //    }

    //    public bool Equals(List<T> x, List<T> y)
    //    {
    //        return x.SetEquals(y, valueComparer);
    //    }

    //    public int GetHashCode(List<T> obj)
    //    {
    //        throw new NotImplementedException();
    //    }

    //}

    //public static class DictionaryComparerHelper
    //{
    //    public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second,
    //        IEqualityComparer<T> comparer)
    //    {
    //        return new HashSet<T>(second, comparer ?? EqualityComparer<T>.Default)
    //            .SetEquals(first);
    //    }

    //    //public static bool Equals<TKey, TValue>(this Dictionary<TKey,TValue> dictOriginal, Dictionary<TKey, TValue> dictCompare) 
    //    //{
    //    //    ListComparer<TKey> listComparer = new ListComparer<TKey>();
    //    //    DictionaryComparer<TKey, TValue> comparer = new DictionaryComparer<TKey, TValue>(listComparer);
    //    //    return (new DictionaryComparer<TKey, List<TKey>>(new ListComparer<TKey>())).Equals(dictOriginal, dictCompare); 
    //    //}
    //}

}
