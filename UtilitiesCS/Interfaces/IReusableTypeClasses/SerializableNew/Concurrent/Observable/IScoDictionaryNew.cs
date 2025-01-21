using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface IScoDictionaryNew<TKey, TValue>: IConcurrentObservableDictionary<TKey, TValue>, ISmartSerializable<ScoDictionaryNew<TKey, TValue>>
    {
        void Notify([CallerMemberName] string propertyName = "");
        void SerializeToStream(StreamWriter sw);
        string SerializeToString();
    }
}