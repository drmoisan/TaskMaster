using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

        internal static List<T> ToList<T>(this IEnumerable<T> enumerable, int count, ProgressTracker progress)
        {
            int completed = 0;
            List<T> list = null;
            progress.Report(0, $"Consuming {0:N0} of {count:N0}");

            using (new System.Threading.Timer(_ => progress.Report(
                    completed,
                    $"Consuming {(int)((double)completed * (double)count / 100):N0} of {count:N0}"),
                    null, 0, 500))
            {
                list = enumerable.WithProgressReporting(count, (x) => completed = x).ToList();
            }
            return list;
        }

        public static IAsyncEnumerable<T> WithProgressReporting<T>(this IAsyncEnumerable<T> enumerable, long count, Action<int> progress)
        {
            if (enumerable is null) { throw new ArgumentNullException($"{nameof(enumerable)}"); }

            int completed = 0;
            return enumerable.Select(x =>
            {
                Interlocked.Increment(ref completed);
                progress((int)(((double)completed / count) * 100));
                return x;
            });
        }
        
        public static IEnumerable<T> WithProgressReporting<T>(this IEnumerable<T> enumerable, long count, Action<int> progress)
        {
            if (enumerable is null) { throw new ArgumentNullException($"{nameof(enumerable)}"); }

            int completed = 0;
            foreach (var item in enumerable)
            {
                yield return item;
                
                Interlocked.Increment(ref completed);
                progress((int)(((double)completed / count) * 100));
            }
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            var props = typeof(T).GetProperties();

            var dt = new DataTable();
            dt.Columns.AddRange(
              props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray()
            );

            source.ToList().ForEach(
              i => dt.Rows.Add(props.Select(p => p.GetValue(i, null)).ToArray())
            );

            return dt;
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

        public static IEnumerable<IEnumerable<T>> Transpose<T>(
            this IEnumerable<IEnumerable<T>> source)
        {
            var enumerators = source.Select(e => e.GetEnumerator()).ToArray();
            try
            {
                while (enumerators.All(e => e.MoveNext()))
                {
                    yield return enumerators.Select(e => e.Current).ToArray();
                }
            }
            finally
            {
                Array.ForEach(enumerators, e => e.Dispose());
            }
        }


        public static Tuple<IEnumerable<T>, IEnumerable<U>> Unzip<T, U>(this IEnumerable<(T, U)> source)
        {
            var first = new List<T>();
            var second = new List<U>();

            foreach (var item in source)
            {
                first.Add(item.Item1);
                second.Add(item.Item2);
            }

            return new Tuple<IEnumerable<T>, IEnumerable<U>>(first, second);
        }


        public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            return ChunkIterator(source, size);
        }

        private static IEnumerable<TSource[]> ChunkIterator<TSource>(IEnumerable<TSource> source, int size)
        {
            using IEnumerator<TSource> e = source.GetEnumerator();

            // Before allocating anything, make sure there's at least one element.
            if (e.MoveNext())
            {
                // Now that we know we have at least one item, allocate an initial storage array. This is not
                // the array we'll yield.  It starts out small in order to avoid significantly overallocating
                // when the source has many fewer elements than the chunk size.
                int arraySize = Math.Min(size, 4);
                int i;
                do
                {
                    var array = new TSource[arraySize];

                    // Store the first item.
                    array[0] = e.Current;
                    i = 1;

                    if (size != array.Length)
                    {
                        // This is the first chunk. As we fill the array, grow it as needed.
                        for (; i < size && e.MoveNext(); i++)
                        {
                            if (i >= array.Length)
                            {
                                arraySize = (int)Math.Min((uint)size, 2 * (uint)array.Length);
                                Array.Resize(ref array, arraySize);
                            }

                            array[i] = e.Current;
                        }
                    }
                    else
                    {
                        // For all but the first chunk, the array will already be correctly sized.
                        // We can just store into it until either it's full or MoveNext returns false.
                        TSource[] local = array; // avoid bounds checks by using cached local (`array` is lifted to iterator object as a field)
                        Debug.Assert(local.Length == size);
                        for (; (uint)i < (uint)local.Length && e.MoveNext(); i++)
                        {
                            local[i] = e.Current;
                        }
                    }

                    if (i != array.Length)
                    {
                        Array.Resize(ref array, i);
                    }

                    yield return array;
                }
                while (i >= size && e.MoveNext());
            }
        }
    
    }
}
