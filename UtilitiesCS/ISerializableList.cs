using System.Collections.Generic;

namespace UtilitiesCS
{
    public interface ISerializableList<T>
    {
        T this[int index] { get; set; }

        int Count { get; }
        string Filename { get; set; }
        string Filepath { get; set; }
        string Folderpath { get; set; }
        bool IsReadOnly { get; }

        void Add(T item);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        void Deserialize();
        void Deserialize(string filepath);
        IEnumerator<T> GetEnumerator();
        int IndexOf(T item);
        void Insert(int index, T item);
        bool Remove(T item);
        void RemoveAt(int index);
        void Serialize();
        void Serialize(string filepath);
        List<T> ToList();
    }
}