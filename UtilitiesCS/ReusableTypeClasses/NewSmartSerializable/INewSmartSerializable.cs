using Newtonsoft.Json;
using System;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface INewSmartSerializable<T> where T: class, INewSmartSerializable<T>, new()
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