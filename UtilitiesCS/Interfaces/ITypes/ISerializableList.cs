using System.Collections.Generic;
using System.Collections;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public delegate IList<T> CSVLoader<T>(string filepath);

    public interface ISerializableList<T>: INotifyPropertyChanged, IList<T>
    {
        string Filename { get; set; }
        string Filepath { get; set; }
        string Folderpath { get; set; }
        void Deserialize();
        void Deserialize(bool askUserOnError);
        void Deserialize(string filepath, bool askUserOnError);
        void Deserialize(string filepath, CSVLoader<T> backupLoader, bool askUserOnError);
        int FindIndex(int startIndex, int count, Predicate<T> match);
        int FindIndex(int startIndex, Predicate<T> match);
        int FindIndex(Predicate<T> match);
        void Serialize();
        void Serialize(string filepath);
        Task SerializeAsync();
        Task SerializeAsync(string filepath);
        List<T> ToList();
        void FromList(IList<T> value);
    }
}