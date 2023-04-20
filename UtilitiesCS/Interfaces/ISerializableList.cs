using System.Collections.Generic;
using System.Collections;
using System;

namespace UtilitiesCS
{
    public delegate IList<T> CSVLoader<T>(string filepath);

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
        void Deserialize(bool askUserOnError);
        void Deserialize(string filepath, CSVLoader<T> backupLoader, bool askUserOnError);
        void Deserialize(string filepath, bool askUserOnError);
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