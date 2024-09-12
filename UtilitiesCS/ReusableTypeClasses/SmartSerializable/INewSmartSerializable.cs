using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.ReusableTypeClasses.SmartSerializable
{
    public interface INewSmartSerializable<T> where T : class, ISmartSerializable<T>, new()
    {
        T Deserialize(string fileName, string folderPath);
        T Deserialize(string fileName, string folderPath, bool askUserOnError);
        T Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings);

        void Serialize();
        void Serialize(string filePath);
        void SerializeThreadSafe(string filePath);

        NewSmartSerializableConfig Config { get; set; }
    }
}
