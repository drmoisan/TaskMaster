#nullable enable

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Input;

namespace UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList
{
    public class LockingObservableLinkedListChangedEventArgs<T>(
        NotifyCollectionChangedAction action
    )
    {
        public LockingObservableLinkedListChangedEventArgs(
            NotifyCollectionChangedAction action,
            LockingObservableLinkedListNode<T>? newNode,
            LockingObservableLinkedListNode<T>? oldNode
        )
            : this(action)
        {
            NewNode = newNode;
            OldNode = oldNode;
        }

        public NotifyCollectionChangedAction Action { get; } = action;

        // Nullable: the action-only primary constructor leaves these unset (null); only the
        // add/replace constructor populates them.
        public LockingObservableLinkedListNode<T>? NewNode { get; }
        public LockingObservableLinkedListNode<T>? OldNode { get; }
    }
}
