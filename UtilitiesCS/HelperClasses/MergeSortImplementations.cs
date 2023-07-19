using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS
{
    public static class MergeSortImplementations
    {
        private static IList<T> MergeSort<T>(IList<T> coll, Comparison<T> comparison)
        {
            // DONE: 2023-03-06 Move To UtilitiesCS. A generic merge sort(of T) doesn't belong in a form controller
            var Result = new List<T>();
            var Left = new Queue<T>();
            var Right = new Queue<T>();
            if (coll.Count <= 1)
                return coll;
            int midpoint = (int)Math.Round(coll.Count / 2d);

            for (int i = 0, loopTo = midpoint - 1; i <= loopTo; i++)
                Left.Enqueue(coll[i]);

            for (int i = midpoint, loopTo1 = coll.Count - 1; i <= loopTo1; i++)
                Right.Enqueue(coll[i]);


            Left = new Queue<T>(MergeSort(Left.ToList(), comparison));
            Right = new Queue<T>(MergeSort(Right.ToList(), comparison));
            Result = Merge(Left, Right, comparison);
            return Result;
        }


        private static List<T> Merge<T>(Queue<T> Left, Queue<T> Right, Comparison<T> comparison)
        {
            // DONE: 2023-03-06 Move To UtilitiesCS. A generic merge sort(of T) doesn't belong in a form controller

            var Result = new List<T>();

            while (Left.Count > 0 && Right.Count > 0)
            {
                int cmp = comparison(Left.Peek(), Right.Peek());
                if (cmp < 0)
                {
                    Result.Add(Left.Dequeue());
                }
                else
                {
                    Result.Add(Right.Dequeue());
                }
            }

            while (Left.Count > 0)
                Result.Add(Left.Dequeue());

            while (Right.Count > 0)
                Result.Add(Right.Dequeue());

            return Result;
        }
    }
}