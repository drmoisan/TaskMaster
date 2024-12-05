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
    public class ProgramData: ScDictionary<string, int>
    {
        #region Constructors

        public ProgramData() : base() { ism = new(this); }
        public ProgramData(IEnumerable<KeyValuePair<string, int>> collection) : base(collection) { ism = new(this); }
        public ProgramData(IEqualityComparer<string> comparer) : base(comparer) { ism = new(this); }
        public ProgramData(IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) : base(collection, comparer) { ism = new(this); }
        public ProgramData(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity) { ism = new(this); }
        public ProgramData(int concurrencyLevel, IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) : base(concurrencyLevel, collection, comparer) { ism = new(this); }
        public ProgramData(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer) : base(concurrencyLevel, capacity, comparer) { ism = new(this); }
        public ProgramData(ScDictionary<string, int> dictionary) : base(dictionary) { }

        #endregion Constructors

        #region Deserialization

        public new static class Static
        {         
            public static ProgramData Deserialize(string fileName, string folderPath) =>
                new ProgramData(ScDictionary<string, int>.Static.Deserialize(fileName, folderPath));

            public static ProgramData Deserialize(string fileName, string folderPath, bool askUserOnError) =>
                new ProgramData(ScDictionary<string, int>.Static.Deserialize(fileName, folderPath, askUserOnError));

            public static ProgramData Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) =>
                new ProgramData(ScDictionary<string, int>.Static.Deserialize(fileName, folderPath, askUserOnError, settings));
        }

        #endregion Deserialization
    }
}
