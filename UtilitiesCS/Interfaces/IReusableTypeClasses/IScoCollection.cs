using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UtilitiesCS
{
    public delegate IList<T> AltListLoader<T>(string filePath);

    public interface IScoCollection<T> : IConcurrentObservableBase<T>, IList<T>, IList
    {
        #region List<T> Implementation by Dan Moisan
        bool Exists(Predicate<T> match);
        T Find(Predicate<T> match);
        int[] FindIndices(Predicate<T> match);
        int[] FindIndices(int startIndex, Predicate<T> match);
        int[] FindIndices(int startIndex, int count, Predicate<T> match);
        int FindIndex(Predicate<T> match);
        int FindIndex(int startIndex, Predicate<T> match);
        int FindIndex(int startIndex, int count, Predicate<T> match);
        #endregion

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
