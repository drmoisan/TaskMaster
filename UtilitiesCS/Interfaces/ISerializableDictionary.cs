using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface ISerializableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>
    //IDictionary<TKey, TValue>, 
    //ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, 
    //System.Runtime.Serialization.IDeserializationCallback, 
    //System.Runtime.Serialization.ISerializable
    {
        // removed IDictionary, IReadOnlyDictionary<TKey, TValue>,
        string Filename { get; set; }
        string Filepath { get; set; }
        string Folderpath { get; set; }

        void Deserialize();
        void Deserialize(bool askUserOnError);
        void Deserialize(string filepath, SerializableDictionary<TKey, TValue>.AltLoader<TKey, TValue> backupLoader, bool askUserOnError);
        void Deserialize(string filepath, bool askUserOnError);
        void Serialize();
        void Serialize(string filepath);
        Dictionary<TKey, TValue> ToDictionary();
        
    }
}