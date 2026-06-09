using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.Threading;

namespace UtilitiesCS
{
    public static class IEnumerableExtensions
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
                else
                {
                    yield return (TResult)item;
                }
            }
        }

        public static (
            int DifferenceCount,
            IEnumerable<T> OnlyThis,
            IEnumerable<T> OnlyOther
        ) CompareTo<T>(this IEnumerable<T> enumerable, IEnumerable<T> other)
        {
            if (enumerable is null)
            {
                if (other is null)
                {
                    throw new ArgumentException(
                        $"Cannot compare differences because both {nameof(IEnumerable<T>)} parameters were null"
                    );
                }
                else
                {
                    return (other.Count(), [], [.. other]);
                }
            }
            else if (other is null)
            {
                return (enumerable.Count(), [.. enumerable], []);
            }
            else
            {
                var onlyThis = enumerable.Except(other);
                var onlyOther = other.Except(enumerable);
                var differenceCount = onlyThis.Count() + onlyOther.Count();
                return (differenceCount, onlyThis, onlyOther);
            }
        }

        //public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        //{
        //    foreach (T item in enumerable)
        //    {
        //        action(item);
        //    }
        //}

        public static bool IsSubsetOf<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            if (source is null || other is null)
            {
                return false;
            }
            return !source.Except(other).Any();
        }

        public static IEnumerable<TValue> SelectGroup<TKey, TValue>(
            this IEnumerable<IGrouping<TKey, TValue>> groups,
            TKey key
        )
        {
            return groups.Where(x => x.Key.Equals(key)).SelectMany(x => x);
        }

        public static string StringJoin(this IEnumerable<string> strings, string seperator = ",") =>
            string.Join(seperator, strings);

        public static string StringJoin(this IEnumerable<char> chars, string seperator = "") =>
            string.Join(seperator, chars);

        internal static List<T> ToList<T>(
            this IEnumerable<T> enumerable,
            int count,
            ProgressTracker progress,
            Action<int> onItemCompleted = null
        )
        {
            int completed = 0;
            List<T> list = null;
            progress.Report(0, $"Consuming {0:N0} of {count:N0}");

            using (
                new System.Threading.Timer(
                    _ => progress.Report(completed, $"Consuming {completed:N0} of {count:N0}"),
                    null,
                    500,
                    500
                )
            )
            {
                list = enumerable
                    .WithProgressReporting(
                        count,
                        (x) =>
                        {
                            completed = x;
                            // Optional deterministic per-item hook (null in production). Lets a test
                            // observe progress without relying on the wall-clock timer to tick.
                            onItemCompleted?.Invoke(x);
                        }
                    )
                    .ToList();
            }
            return list;
        }

        public static Stack<T> ToStack<T>(this IEnumerable<T> enumerable)
        {
            return new Stack<T>(enumerable);
        }

        public static IEnumerable<T> WithProgressReporting<T>(
            this IEnumerable<T> enumerable,
            long count,
            Action<int> progress
        )
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException($"{nameof(enumerable)}");
            }

            int completed = 0;
            foreach (var item in enumerable)
            {
                yield return item;

                Interlocked.Increment(ref completed);
                progress((int)(((double)completed / count) * 100));
            }
        }

        public static IEnumerable<T> WithProgressReporting<T>(
            this IEnumerable<T> enumerable,
            long count,
            Action<long, long> progress
        )
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException($"{nameof(enumerable)}");
            }

            long completed = 0;
            foreach (var item in enumerable)
            {
                yield return item;

                Interlocked.Increment(ref completed);
                progress(completed, count);
            }
        }

        public static IEnumerable<T> WithProgressReporting<T>(
            this IEnumerable<T> enumerable,
            int count,
            ProgressTrackerPane progress,
            Stopwatch sw
        )
        {
            enumerable.ThrowIfNullOrEmpty();
            progress.ThrowIfNull();
            sw ??= Stopwatch.StartNew();

            int completed = 0;
            foreach (var item in enumerable)
            {
                yield return item;

                Interlocked.Increment(ref completed);

                progress.Report(
                    (double)completed / count * 100,
                    $"Testing Classifiers -> {GetProgressMessage(completed, count, sw)}"
                );
            }
        }

        private static string GetProgressMessage(int complete, int count, Stopwatch sw)
        {
            double seconds = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg =
                $"Completed {complete} of {count} ({seconds:N2} spm) "
                + $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        public static IEnumerable<T> WithAction<T>(
            this IEnumerable<T> enumerable,
            System.Action action
        )
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException($"{nameof(enumerable)}");
            }

            foreach (var item in enumerable)
            {
                action();
                yield return item;
            }
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            var props = typeof(T).GetProperties();

            var dt = new DataTable();
            dt.Columns.AddRange(
                props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray()
            );

            source
                .ToList()
                .ForEach(i => dt.Rows.Add(props.Select(p => p.GetValue(i, null)).ToArray()));

            return dt;
        }

        public static Tuple<IEnumerable<T>, IEnumerable<U>> Unzip<T, U>(
            this IEnumerable<(T, U)> source
        )
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

        public static Tuple<IEnumerable<T>, IEnumerable<U>, IEnumerable<V>> Unzip<T, U, V>(
            this IEnumerable<(T, U, V)> source
        )
        {
            var first = new List<T>();
            var second = new List<U>();
            var third = new List<V>();

            foreach (var item in source)
            {
                first.Add(item.Item1);
                second.Add(item.Item2);
                third.Add(item.Item3);
            }

            return new Tuple<IEnumerable<T>, IEnumerable<U>, IEnumerable<V>>(first, second, third);
        }

        public static IEnumerable<IEnumerable<T>> Transpose<T>(
            this IEnumerable<IEnumerable<T>> source
        )
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

        public static IEnumerable<TSource[]> Chunk<TSource>(
            this IEnumerable<TSource> source,
            int size
        )
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

        private static IEnumerable<TSource[]> ChunkIterator<TSource>(
            IEnumerable<TSource> source,
            int size
        )
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
                } while (i >= size && e.MoveNext());
            }
        }

        private static IEnumerable<T[]> SplitIterator<T>(IEnumerable<T> source, int size)
        {
            using IEnumerator<T> e = source.GetEnumerator();

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
                    var array = new T[arraySize];

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
                        T[] local = array; // avoid bounds checks by using cached local (`array` is lifted to iterator object as a field)
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
                } while (i >= size && e.MoveNext());
            }
        }

        public static (T[] Train, T[] Test) SplitTestTrain<T>(
            this IEnumerable<T> collection,
            double trainPercent
        )
        {
            collection.ThrowIfNullOrEmpty();
            if (trainPercent < 0 || trainPercent > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(trainPercent),
                    "Train percentage must be between 0 and 1"
                );
            }

            var array = collection.ToArray();

            // Use a deterministic sequential split: the first trainCount items go to Train and
            // the remainder go to Test. This guarantees stable, repeatable partitions across runs
            // (required for deterministic unit testing) and avoids the non-zero probability of
            // degenerate all-train or all-test splits that a random per-item assignment produces.
            var trainCount = (int)Math.Round(array.Length * trainPercent);
            var train = array.Take(trainCount).ToArray();
            var test = array.Skip(trainCount).ToArray();

            return (train, test);
        }
    }
}
