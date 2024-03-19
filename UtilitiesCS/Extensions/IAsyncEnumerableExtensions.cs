using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.Extensions
{
#nullable enable
    
    public static class IAsyncEnumerableExtensions
    {
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


        /// <summary>
        /// Creates a <seealso cref="SortedList{TKey, TValue}"/> from an async-enumerable sequence according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <typeparam name="TElement">The type of the dictionary value computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="elementSelector"/> or <paramref name="comparer"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static ValueTask<SortedList<TKey, TElement>> ToSortedListAsync<TSource, TKey, TElement>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, 
            IComparer<TKey>? comparer, 
            CancellationToken cancellationToken = default) where TKey : notnull
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));

            return Core(source, keySelector, elementSelector, comparer, cancellationToken);
        }

        /// <summary>
        /// Creates a <seealso cref="SortedList{TKey, TValue}"/> from an async-enumerable sequence according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static ValueTask<SortedList<TKey, TSource>> ToSortedListAsync<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            CancellationToken cancellationToken = default) where TKey : notnull =>
            ToSortedListAsync(source, keySelector, comparer: null, cancellationToken);

        /// <summary>
        /// Creates a <seealso cref="SortedList{TKey, TValue}"/> from an async-enumerable sequence according to a specified key selector function, and a comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="comparer"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public static ValueTask<SortedList<TKey, TSource>> ToSortedListAsync<TSource, TKey>(
            this IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IComparer<TKey>? comparer, 
            CancellationToken cancellationToken = default) where TKey : notnull
        {
            source.ThrowIfNull();
            keySelector.ThrowIfNull();
            
            return Core(source, keySelector, comparer, cancellationToken);
        }

        private static async ValueTask<SortedList<TKey, TElement>>
            Core<TSource, TKey, TElement>(
                IAsyncEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector,
                IComparer<TKey>? comparer,
                CancellationToken cancellationToken)
        {
            SortedList<TKey, TElement> sl = comparer is null ? new(comparer!) : [];
            await foreach (var item in source
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                var key = keySelector(item);
                var value = elementSelector(item);
                sl.Add(key, value);
            }

            return sl;
        }

        private static async ValueTask<SortedList<TKey, TSource>> Core<TKey, TSource>(
            IAsyncEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, 
            IComparer<TKey>? comparer, 
            CancellationToken cancellationToken)
        {
            var sl = comparer is null ? [] : new SortedList<TKey, TSource>(comparer);
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var key = keySelector(item);
                sl.Add(key, item);
            }

            return sl;
        }
    }
}
