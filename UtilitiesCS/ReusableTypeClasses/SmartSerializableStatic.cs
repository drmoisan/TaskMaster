using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class SmartSerializableStatic<T> where T : class, ISmartSerializable<T>, new()
    {
        private static SmartSerializable<T> GetInstance() => new();
        public static T Deserialize(string fileName, string folderPath) => 
            GetInstance().Deserialize(fileName, folderPath);
        
        public static T Deserialize(string fileName, string folderPath, bool askUserOnError) => 
            GetInstance().Deserialize(fileName, folderPath, askUserOnError);
        
        public static T Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) => 
            GetInstance().Deserialize(fileName, folderPath, askUserOnError, settings);

        public static JsonSerializerSettings GetDefaultSettings() => 
            GetInstance().GetDefaultSettings();
    }
}
