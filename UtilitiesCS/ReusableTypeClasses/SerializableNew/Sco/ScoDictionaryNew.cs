using Microsoft.Office.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class ScoDictionaryNew<TKey, TValue>: ConcurrentObservableDictionary<TKey, TValue>, ISmartSerializable<ScoDictionaryNew<TKey, TValue>>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public ScoDictionaryNew() : base() { ism = new SmartSerializable<ScoDictionaryNew<TKey, TValue>>(this); }
        public ScoDictionaryNew(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { ism = new SmartSerializable<ScoDictionaryNew<TKey, TValue>>(this); }
        public ScoDictionaryNew(IEqualityComparer<TKey> comparer) : base(comparer) { ism = new SmartSerializable<ScoDictionaryNew<TKey, TValue>>(this); }
        public ScoDictionaryNew(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) { ism = new SmartSerializable<ScoDictionaryNew<TKey, TValue>>(this); }
        public ScoDictionaryNew(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity) { ism = new SmartSerializable<ScoDictionaryNew<TKey, TValue>>(this); }
        public ScoDictionaryNew(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, collection, comparer) { ism = new SmartSerializable<ScoDictionaryNew<TKey, TValue>>(this); }
        public ScoDictionaryNew(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer) { ism = new SmartSerializable<ScoDictionaryNew<TKey, TValue>>(this); }
        public ScoDictionaryNew(ScoDictionaryNew<TKey, TValue> dictionary) : base(dictionary) { ism = dictionary.ism; }

        #endregion Constructors

        #region ISmartSerializable

        public NewSmartSerializableConfig Config { get => ism.Config; set => ism.Config = value; }
        protected SmartSerializable<ScoDictionaryNew<TKey, TValue>> ism;

        public void Serialize() => ism.Serialize();
        public void Serialize(string filePath) => ism.Serialize(filePath);
        public void SerializeThreadSafe(string filePath) => ism.SerializeThreadSafe(filePath);
        public ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath) => ism.Deserialize(fileName, folderPath);
        public ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError) => ism.Deserialize(fileName, folderPath, askUserOnError);
        public ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) => ism.Deserialize(fileName, folderPath, askUserOnError, settings);

        #endregion ISmartSerializable

        public string Name { get; set; }

        #region INotifyPropertyChanged

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public void Notify([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        public static class Static
        {
            private static SmartSerializable<ScoDictionaryNew<TKey, TValue>> GetInstance() => new();

            public static ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath) =>
                GetInstance().Deserialize(fileName, folderPath);

            public static ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError);

            public static ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError, settings);
        }
              
    }
}
