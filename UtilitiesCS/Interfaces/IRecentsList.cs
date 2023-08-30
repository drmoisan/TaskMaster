namespace UtilitiesCS
{
    public interface IRecentsList<T>: ISerializableList<T>
    {
        new void Add(T item);
    }
}