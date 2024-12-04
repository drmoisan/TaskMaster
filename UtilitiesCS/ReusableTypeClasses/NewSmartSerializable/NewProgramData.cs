using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilitiesCS.ReusableTypeClasses;

namespace ToDoModel
{
    public class NewProgramData: NewScDictionary<string, int>
    {
        #region Constructors

        public NewProgramData() : base() { ism = new NewSmartSerializable<NewScDictionary<string, int>>(this); }
        public NewProgramData(IEnumerable<KeyValuePair<string, int>> collection) : base(collection) { ism = new NewSmartSerializable<NewScDictionary<string, int>>(this); }
        public NewProgramData(IEqualityComparer<string> comparer) : base(comparer) { ism = new NewSmartSerializable<NewScDictionary<string, int>>(this); }
        public NewProgramData(IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) : base(collection, comparer) { ism = new NewSmartSerializable<NewScDictionary<string, int>>(this); }
        public NewProgramData(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity) { ism = new NewSmartSerializable<NewScDictionary<string, int>>(this); }
        public NewProgramData(int concurrencyLevel, IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) : base(concurrencyLevel, collection, comparer) { ism = new NewSmartSerializable<NewScDictionary<string, int>>(this); }
        public NewProgramData(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer) : base(concurrencyLevel, capacity, comparer) { ism = new NewSmartSerializable<NewScDictionary<string, int>>(this); }
        public NewProgramData(NewScDictionary<string, int> dictionary) : base(dictionary) { }

        #endregion Constructors

        #region Deserialization

        public new static class Static
        {         
            public static NewProgramData Deserialize(string fileName, string folderPath) =>
                new NewProgramData(NewScDictionary<string, int>.Static.Deserialize(fileName, folderPath));

            public static NewProgramData Deserialize(string fileName, string folderPath, bool askUserOnError) =>
                new NewProgramData(NewScDictionary<string, int>.Static.Deserialize(fileName, folderPath, askUserOnError));

            public static NewProgramData Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) =>
                new NewProgramData(NewScDictionary<string, int>.Static.Deserialize(fileName, folderPath, askUserOnError, settings));
        }

        #endregion Deserialization
    }
}
