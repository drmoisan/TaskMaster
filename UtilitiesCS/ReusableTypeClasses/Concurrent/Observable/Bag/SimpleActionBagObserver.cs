using System;

namespace UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Bag
{
    internal class SimpleActionBagObserver<T> : ISimpleActionBagObserver<T>
    {
        private readonly Action<BagChangedEventArgs<T>> _action;

        public SimpleActionBagObserver(Action<BagChangedEventArgs<T>> action)
        {
            _action = action;
        }

        public void OnEventOccur(BagChangedEventArgs<T> args)
        {
            _action.Invoke(args);
        }
    }
}
