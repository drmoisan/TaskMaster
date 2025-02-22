namespace UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Bag
{
    internal interface ISimpleActionBagObserver<T>
    {
        void OnEventOccur(BagChangedEventArgs<T> args);
    }
}