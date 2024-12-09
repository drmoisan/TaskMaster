using System;
using System.Collections.Generic;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface ILockingLinkedList<T>
    {
        int Count { get; }
        LockingLinkedListNode<T> First { get; }
        LockingLinkedListNode<T> Last { get; }

        void AddAfter(LinkedListNode<T> node, T item);
        void AddAfter(LockingLinkedListNode<T> node, T item);
        void AddBefore(LinkedListNode<T> node, T item);
        void AddBefore(LockingLinkedListNode<T> node, T item);
        void AddFirst(T item);
        void AddLast(T item);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        LockingLinkedListNode<T> Find(Predicate<T> predicate);
        LockingLinkedListNode<T> Find(T value);
        LockingLinkedListNode<T> FindLast(T value);
        void Remove(LinkedListNode<T> node);
        void Remove(LockingLinkedListNode<T> node);
        void Remove(Predicate<T> match);
        void Remove(T item);
        void RemoveFirst();
        void RemoveLast();
        T TakeFirst();
        T[] TakeFirst(int n);
        T TakeLast();
        T[] TakeLast(int n);
        T[] TryTakeFirst(int n);
    }
}