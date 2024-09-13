using Newtonsoft.Json;
using System;

namespace UtilitiesCS.ReusableTypeClasses
{
    public interface INewSmartSerializable<T> where T: class, INewSmartSerializable<T>, new()
    {
        //FilePathHelper Disk { get; set; }
        //JsonSerializerSettings JsonSettings { get; set; }
        //FilePathHelper LocalDisk { get; set; }
        //JsonSerializerSettings LocalJsonSettings { get; set; }
        //FilePathHelper NetDisk { get; set; }
        //JsonSerializerSettings NetJsonSettings { get; set; }

        //void ActivateLocalDisk();
        //void ActivateNetDisk();
        
        T Deserialize(string fileName, string folderPath);
        T Deserialize(string fileName, string folderPath, bool askUserOnError);
        T Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings);
        
        void Serialize();
        void Serialize(string filePath);
        void SerializeThreadSafe(string filePath);

        INewSmartSerializableConfig Config { get; set; }
    }
}