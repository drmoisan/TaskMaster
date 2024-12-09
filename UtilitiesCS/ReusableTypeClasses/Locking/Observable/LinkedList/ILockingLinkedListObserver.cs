using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;

namespace ConcurrentObservableCollections.ConcurrentObservableDictionary
{
    public interface ILockingLinkedListObserver<T>
    {
        void OnEventOccur(LockingObservableLinkedListChangedEventArgs<T> args);
    }
}