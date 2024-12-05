using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface ISmartSerializable<T>:INotifyPropertyChanged where T: class, ISmartSerializable<T>, new()
    {
        T Deserialize(string fileName, string folderPath);
        T Deserialize(string fileName, string folderPath, bool askUserOnError);
        T Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings);
        
        void Serialize();
        void Serialize(string filePath);
        void SerializeThreadSafe(string filePath);

        NewSmartSerializableConfig Config { get; set; }

        string Name { get; set; }

    }
}