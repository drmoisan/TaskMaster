namespace UtilitiesCS
{
    public interface IRecentsList<T>: ISerializableList<T>
    {
        void AddRecent(T item);
    }
}