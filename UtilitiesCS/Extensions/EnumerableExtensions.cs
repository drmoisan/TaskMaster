using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Casts the elements of an System.Collections.IEnumerable to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to cast the elements of source to.</typeparam>
        /// <param name="source">The System.Collections.<seealso cref="System.Collections.IEnumerable"/> that contains the elements to be cast to type TResult</param>
        /// <returns>An System.Collections.Generic.<seealso cref="IEnumerable{TResult}"/> that contains each element of the
        ///     source sequence cast to the specified type.</returns>
        /// <exception cref="ArgumentNullException">An element in the sequence cannot be cast to type TResult.</exception>
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

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }

        public static IEnumerable<T> WithProgressReporting<T>(this IEnumerable<T> enumerable, long count, Action<int> progress)
        {
            if (enumerable is null) { throw new ArgumentNullException($"{nameof(enumerable)}"); }

            int completed = 0;
            foreach (var item in enumerable)
            {
                yield return item;

                completed++;
                progress((int)(((double)completed / count) * 100));
            }
        }


        public static async IAsyncEnumerable<(TFirst, TSecond)> Zip<TFirst, TSecond>(this IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second)
        {
            await using var e1 = first.GetAsyncEnumerator();
            await using var e2 = second.GetAsyncEnumerator();

            while (true)
            {
                var t1 = e1.MoveNextAsync().AsTask();
                var t2 = e2.MoveNextAsync().AsTask();
                await Task.WhenAll(t1, t2);

                if (!t1.Result || !t2.Result)
                    yield break;

                yield return (e1.Current, e2.Current);
            }
        }


    }
}
