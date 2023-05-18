using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace UtilitiesVB
{

    public static class ToSortedDictionaryType
    {
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
}