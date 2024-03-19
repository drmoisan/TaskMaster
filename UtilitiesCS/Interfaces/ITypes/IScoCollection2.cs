using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swordfish.NET.General.Collections;

namespace UtilitiesCS
{
    internal interface IScoCollection2<T>: IConcurrentObservableCollection<T>
    {
        #region Serialization

        string FileName { get; set; }
        string FilePath { get; set; }
        string Folderpath { get; set; }

        void Deserialize();
        void Deserialize(bool askUserOnError);
        void Deserialize(string filePath, AltListLoader<T> backupLoader, bool askUserOnError);
        void Deserialize(string filePath, bool askUserOnError);
        void Serialize();
        void Serialize(string filePath);
        Task SerializeAsync();
        Task SerializeAsync(string filePath);

        #endregion

        List<T> ToList();
        void FromList(IList<T> value);
    }
}
