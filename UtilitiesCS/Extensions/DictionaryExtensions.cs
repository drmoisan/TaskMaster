using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Generic.Math;
using UtilitiesCS.Extensions;

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
        /// <param name="source">IEnumerable of a KeyValuePair from a dictionary</param>
        /// <returns>A Sorted Dictionary</returns>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            if (source.IsNullOrEmpty()) { return []; }
            IEnumerable<IGrouping<TKey, KeyValuePair<TKey, TValue>>> duplicateKVPsByKey = source.GroupBy(kvp => kvp.Key).Where(g => g.Count() > 1);
            if (duplicateKVPsByKey.Any())
                throw new InvalidOperationException($"Cannot convert to dictionary with duplicate keys: {string.Join(",", duplicateKVPsByKey)}");

            var result = new Dictionary<TKey, TValue>();
            foreach (var e in source)
                result[e.Key] = e.Value;

            return result;
        }

        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            var duplicateKVPsByKey = source.GroupBy(kvp => kvp.Key).Where(g => g.Count() > 1);
            if (duplicateKVPsByKey.Any())
                throw new InvalidOperationException($"Cannot convert to dictionary with duplicate keys: {string.Join(",", duplicateKVPsByKey)}");
            return new ConcurrentDictionary<TKey, TValue>(source);
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

        public static bool TryAddValues<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {            
            while (dictionary.TryGetValue(key, out var existingValue))
            {
                TValue newValue = GenericMath<TValue>.Add(existingValue, value);
                if (dictionary.TryUpdate(key, newValue, existingValue))
                    return true;
            }
            return false;
        }

        public static bool TrySubtractValues<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            while (dictionary.TryGetValue(key, out var existingValue))
            {
                TValue newValue = GenericMath<TValue>.Subtract(existingValue, value);
                if (dictionary.TryUpdate(key, newValue, existingValue))
                    return true;
            }
            return false;
        }

        public static bool TryOperateValues<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value, Func<TValue, TValue, TValue> operation)
        {
            while (dictionary.TryGetValue(key, out var existingValue))
            {
                TValue newValue = operation(existingValue, value);
                if (dictionary.TryUpdate(key, newValue, existingValue))
                    return true;
            }
            return false;
        }

        public static async Task<bool> TryAddValuesAsync<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value, CancellationToken token)
        {
            var linkedTS = CancellationTokenSource.CreateLinkedTokenSource(token);
            linkedTS.CancelAfter(500);
            
            return await Task.Run(() => dictionary.TryAddValues(key, value), linkedTS.Token);
        }

        public static Enums.DictionaryResult UpdateOrRemove<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue, bool> removeCondition,
            Func<TKey, TValue, TValue> updateValueFactory,
            out TValue value)
        {
            while (dictionary.TryGetValue(key, out value))
            {
                if (removeCondition(key, value))
                {
                    // removes if KVP is exact match. If another thread has
                    // updated the value, it will fail and try the loop again
                    if (((ICollection<KeyValuePair<TKey,TValue>>)dictionary)
                        .Remove(new KeyValuePair<TKey, TValue>(key, value)))
                    {
                        value = default;
                        return Enums.DictionaryResult.KeysChanged;
                    }
                }
                else
                {
                    TValue newValue = updateValueFactory(key, value);
                    if (dictionary.TryUpdate(key, newValue, value))
                        return Enums.DictionaryResult.ValueChanged | Enums.DictionaryResult.KeyExists;
                }
            }
            return default;
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
