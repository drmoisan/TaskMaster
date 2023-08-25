using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface IScoDictionary<TKey, TValue>: IDictionary<TKey,TValue>, ICollection<KeyValuePair<TKey, TValue>>, ICollection, INotifyCollectionChanged, IDisposable
    {
        #region ConcurrentObservableDictionary  
        
        int IndexOfKey(TKey key);
        TValue RetrieveOrAdd(TKey key, Func<TValue> getValue);
        bool TryAdd(TKey key, TValue value);
        bool TryGetIndexOf(TKey key, out int index);
        new int Count { get; }
        
        #endregion

        #region Serialization

        public delegate Dictionary<TKey, TValue> AltLoader(string filepath);

        string Filename { get; set; }
        string Filepath { get; set; }
        string Folderpath { get; set; }

        void Deserialize();
        void Deserialize(bool askUserOnError);
        void Deserialize(string filepath, AltLoader backupLoader, bool askUserOnError);
        void Deserialize(string filepath, bool askUserOnError);
        void Serialize();
        void Serialize(string filepath);
        Task SerializeAsync();
        Task SerializeAsync(string filepath);
        
        #endregion  
        
        Dictionary<TKey, TValue> ToDictionary();
    
    }
}
