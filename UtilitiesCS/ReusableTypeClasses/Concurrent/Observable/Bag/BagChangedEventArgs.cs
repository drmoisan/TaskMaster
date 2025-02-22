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
        public T NewValue { get; }
        public T OldValue { get; }
    }
}
