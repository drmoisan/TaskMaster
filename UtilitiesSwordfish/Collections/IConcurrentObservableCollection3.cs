// Authored by: John Stewien
// Year: 2011
// Company: Swordfish Computing
// License: 
// The Code Project Open License http://www.codeproject.com/info/cpol10.aspx
// Originally published at:
// http://www.codeproject.com/Articles/208361/Concurrent-Observable-Collection-Dictionary-and-So
// Last Revised: September 2012

using System;

namespace Swordfish.NET.Collections
{
    public interface IConcurrentObservableCollection3<T>
    {
        T this[int index] { get; set; }

        int Count { get; }
        bool IsReadOnly { get; }

        void Add(T item);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        bool Exists(Predicate<T> match);
        T Find(Predicate<T> match);
        int FindIndex(int startIndex, int count, Predicate<T> match);
        int FindIndex(int startIndex, Predicate<T> match);
        int FindIndex(Predicate<T> match);
        int[] FindIndices(int startIndex, int count, Predicate<T> match);
        int[] FindIndices(int startIndex, Predicate<T> match);
        int[] FindIndices(Predicate<T> match);
        int IndexOf(T item);
        void Insert(int index, T item);
        bool Remove(T item);
        void RemoveAt(int index);
    }
}