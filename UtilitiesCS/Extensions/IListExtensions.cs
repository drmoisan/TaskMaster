using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;

namespace UtilitiesCS
{
    public static class IListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            list.ThrowIfNull();
            items.ThrowIfNull();
            if (list is List<T> asList)
            {
                asList.AddRange(items);
            }
            else
            {
                foreach (var item in items)
                {
                    list.Add(item);
                }
            }
        }
        
        public static bool TryAddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list is null || items is null)
            {
                return false;
            }
            if (list is List<T> asList)
            {
                try
                {
                    asList.AddRange(items);
                }
                catch (System.Exception)
                {
                    return false;
                } 
            }
            else
            {
                try
                {
                    foreach (var item in items)
                    {
                        list.Add(item);
                    }
                }
                catch (System.Exception)
                {
                    return false;
                }                
            }
            return true;
        }

        public static bool Contains(this IList<string> list, string value, StringComparison comparison)
        {
            return list.FindIndex(value, comparison) != -1;
        }
    
        public static bool Exists<T>(this IList<T> list, Predicate<T> match)
        {
            return list.FindIndex(match) != -1;
        }

        public static T Find<T>(this IList<T> list, Predicate<T> match)
        {
            var index = list.FindIndex(0, list.Count, match);
            if (index == -1)
            {
                return default(T);
            }
            else
            {
                return list[index];
            }
        }

        public static (int DifferenceCount, IList<T> OnlyThis, IList<T> OnlyOther) CompareTo<T>(this IList<T> list, IList<T> other)
        {
            if (list is null)
            {
                if (other is null) { throw new ArgumentException($"Cannot compare differences because both lists were null"); }
                else { return (other.Count, [], [.. other]); }
            }
            else if (other is null) { return (list.Count, [.. list], []); }
            else
            {
                var onlyThis = list.Except(other).ToList();
                var onlyOther = other.Except(list).ToList();
                var differenceCount = onlyThis.Count + onlyOther.Count;
                return (differenceCount, onlyThis, onlyOther);
            }
        }

        public static int[] FindIndices<T>(this IList<T> list, Predicate<T> match)
        {
            return list.FindIndices(0, list.Count, match);
        }

        public static int[] FindIndices<T>(this IList<T> list, int startIndex, Predicate<T> match)
        {
            return list.FindIndices(startIndex, list.Count, match);
        }

        public static int[] FindIndices<T>(this IList<T> list, int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} has a value of {startIndex} which is greater than the list length of {list.Count}");
            }

            if (count < 0 || startIndex > list.Count - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var indices = new List<int>();

            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(list[i]))
                {
                    indices.Add(i);
                }
            }

            return indices.ToArray();
        }

        public static int FindIndex<T>(this IList<T> list, Predicate<T> match)
        {
            return list.FindIndex(0, list.Count, match);
        }

        public static int FindIndex<T>(this IList<T> list, int startIndex, Predicate<T> match)
        {
            return list.FindIndex(startIndex, list.Count - startIndex, match);
        }

        public static int FindIndex<T>(this IList<T> list, int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), $"{nameof(startIndex)} has a value of {startIndex} which is greater than the list length of {list.Count}");
            }

            if (count < 0 || startIndex > list.Count - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(list[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static int FindIndex(this IList<string> list, string value, StringComparison comparison)
        {
            return list.FindIndex(x => string.Equals(x, value, comparison));
        }

        public static T FindMax<T>(this IList<T> list, Func<T, T, T> selector)
        {
            list.ThrowIfNullOrEmpty();
            selector.ThrowIfNull();
            T max = list.Aggregate((a, b) => selector(a, b));
            return max;
        }

        public static bool TryFindMax<T>(this IList<T> list, Func<T, T, T> selector, out T max)
        {
            max = default;
            if (list.IsNullOrEmpty() || selector is null)
            {
                return false;
            }
            try
            {
                max = list.Aggregate((a, b) => selector(a, b));
            }
            catch (System.Exception)
            {
                return false;
            }
            
            return true;
        }

        public static bool IsNullOrEmpty(this IList<string> list) => list is null || list.Count == 0;

        public static (IList<T> Unique, IList<T> Duplicates) Split<T>(this IList<T> list, IEqualityComparer<T> comparer)
        {
            if (list == null)
                return (new List<T>(), new List<T>());
            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            var groups = list.GroupBy(x => x, comparer);
            var unique = groups.Where(g => g.Count() == 1).SelectMany(g => g).ToList();
            var duplicates = groups.Where(g => g.Count() > 1).SelectMany(g => g).ToList();
            return (unique, duplicates);
        }
    }
}
