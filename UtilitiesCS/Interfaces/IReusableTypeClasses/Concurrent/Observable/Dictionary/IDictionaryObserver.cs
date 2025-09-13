namespace UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary
{
    public interface IDictionaryObserver<TKey, TValue>
    {
        void OnEventOccur(DictionaryChangedEventArgs<TKey, TValue> args);
    }
}