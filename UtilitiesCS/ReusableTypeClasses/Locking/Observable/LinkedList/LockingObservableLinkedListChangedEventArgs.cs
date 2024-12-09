using System.Collections.Specialized;
using System.Collections.Generic;
using System.Windows.Input;

namespace UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList
{
    public class LockingObservableLinkedListChangedEventArgs<T>(NotifyCollectionChangedAction action)
    {
        public LockingObservableLinkedListChangedEventArgs(
            NotifyCollectionChangedAction action, 
            LockingObservableLinkedListNode<T> newNode, 
            LockingObservableLinkedListNode<T> oldNode)
            : this(action)
        {
            NewNode = newNode;
            OldNode = oldNode;
        }

        public NotifyCollectionChangedAction Action { get; } = action;
        public LockingObservableLinkedListNode<T> NewNode { get; }
        public LockingObservableLinkedListNode<T> OldNode { get; }
    }
}
