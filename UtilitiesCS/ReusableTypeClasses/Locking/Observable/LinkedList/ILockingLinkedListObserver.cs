using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;

namespace ConcurrentObservableCollection.ConcurrentObservableDictionary
{
    public interface ILockingLinkedListObserver<T>
    {
        void OnEventOccur(LockingObservableLinkedListChangedEventArgs<T> args);
    }
}