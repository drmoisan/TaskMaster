namespace UtilitiesCS
{
    public interface IRecentsList<T>: ISerializableList<T>
    {
        void Add(T item);
    }
}