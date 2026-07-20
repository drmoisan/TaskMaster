#nullable enable

using System.Collections.Specialized;

namespace UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Bag
{
    public class BagChangedEventArgs<T>
    {
        public BagChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Action = action;
        }

        public BagChangedEventArgs(NotifyCollectionChangedAction action, T newValue, T oldValue)
            : this(action)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }

        public NotifyCollectionChangedAction Action { get; }

        // Nullable: the action-only constructor leaves these at default(T) (null for reference T).
        public T? NewValue { get; }
        public T? OldValue { get; }
    }
}
