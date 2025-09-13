using ConcurrentObservableCollection.ConcurrentObservableDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList
{
    public class SimpleActionLockingLinkedListObserver<T>: ILockingLinkedListObserver<T>
    {
        private readonly Action<LockingObservableLinkedListChangedEventArgs<T>> _action;

        public SimpleActionLockingLinkedListObserver(Action<LockingObservableLinkedListChangedEventArgs<T>> action)
        {
            _action = action;
        }

        public void OnEventOccur(LockingObservableLinkedListChangedEventArgs<T> args)
        {
            _action.Invoke(args);
        }
    }
    
}
