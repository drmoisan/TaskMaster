using Deedle.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS
{
    public static class MergeSortImplementations
    {

        /// <summary>
        /// 2 Overloads
        /// <para>Generic implementation of merge sort algorithm. </para>
        /// <para> Parameter <c><paramref name="inplace"/></c>
        /// determines whether the method returns a new <seealso cref="IList{T}"/> or copies the results 
        /// back into the original parameter named <c><paramref name="list"/></c>.</para>
        /// </summary>
        /// <typeparam name="T">Generic type parameter</typeparam>
        /// <param name="list">the <seealso cref="IList{T}"/> to be sorted </param>
        /// <param name="comparison">Represents the method that compares two objects of the same type.</param>
        /// <param name="inplace"><seealso cref="bool"/> to determine whether to copy back into the original variable</param>
        /// <returns><code>null | <seealso cref="IList{T}"/></code> 
        /// <br/>
        /// <c>null</c> - when <c>InPlace</c> is <c>true</c>. Values are copied to <paramref name="list"/>
        /// <br/>
        /// <seealso cref="IList{T}"/> - when <c>InPlace</c> is <c>false</c>
        /// </returns>
        public static IList<T> MergeSort<T>(this IList<T> list, Comparison<T> comparison, bool inplace)
        {
            var count = list.Count;
            var result = list.MergeSort(comparison);

            if (inplace)
            {
                for (int i = 0; i < count; i++)
                {
                    list[i] = result[i];
                }

                return null;
            }
            
            return result; 
        }

        /// <summary>
        /// Generic implementation of merge sort algorithm
        /// </summary>
        /// <typeparam name="T">Generic type parameter</typeparam>
        /// <param name="list">the <seealso cref="IList{T}"/> to be sorted </param>
        /// <param name="comparison">Represents the method that compares two objects of the same type.</param>
        /// <returns>The sorted <seealso cref="IList{T}"/></returns>
        public static IList<T> MergeSort<T>(this IList<T> list, Comparison<T> comparison)
        {
            var left = new Queue<T>();
            var right = new Queue<T>();
            var count = list.Count;
            if (count <= 1)
                return list;
            int midpoint = (int)Math.Round(count / 2d);

            for (int i = 0, loopTo = midpoint - 1; i <= loopTo; i++)
                left.Enqueue(list[i]);

            for (int i = midpoint, loopTo = count - 1; i <= loopTo; i++)
                right.Enqueue(list[i]);

            left = new Queue<T>(MergeSort(left.ToList(), comparison));
            right = new Queue<T>(MergeSort(right.ToList(), comparison));
            var result = left.Merge(right, comparison);

            for (int i = 0; i< count; i++)
            {
                list[i] = result[i];
            }

            return list;
        }

        private static List<T> Merge<T>(this Queue<T> left, Queue<T> right, Comparison<T> comparison)
        {
            var result = new List<T>();

            while (left.Count > 0 && right.Count > 0)
            {
                int comp = comparison(left.Peek(), right.Peek());
                if (comp < 0) { result.Add(left.Dequeue()); }
                else { result.Add(right.Dequeue()); }
            }

            while (left.Count > 0)
                result.Add(left.Dequeue());

            while (right.Count > 0)
                result.Add(right.Dequeue());

            return result;
        }

    }
}