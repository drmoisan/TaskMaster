using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class IListExtensions
    {
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
    }
}
