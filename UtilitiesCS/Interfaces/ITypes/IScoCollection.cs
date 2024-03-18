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
    public delegate IList<T> AltListLoader<T>(string filepath);

    public interface IScoCollection<T> : IConcurrentObservableBase<T>, IList<T>, ICollection<T>, IList, ICollection
    {
        #region Serialization

        string Filename { get; set; }
        string Filepath { get; set; }
        string Folderpath { get; set; }

        void Deserialize();
        void Deserialize(bool askUserOnError);
        void Deserialize(string filepath, AltListLoader<T> backupLoader, bool askUserOnError);
        void Deserialize(string filepath, bool askUserOnError);
        void Serialize();
        void Serialize(string filepath);
        Task SerializeAsync();
        Task SerializeAsync(string filepath);

        #endregion

        List<T> ToList();
        void FromList(IList<T> value);
    }
}
