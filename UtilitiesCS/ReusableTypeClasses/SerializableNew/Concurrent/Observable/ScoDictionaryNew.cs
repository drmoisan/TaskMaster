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
using System.Runtime.CompilerServices;
using UtilitiesCS.NewtonsoftHelpers.Sco;

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
        public string SerializeToString() => ism.SerializeToString();
        public void SerializeToStream(StreamWriter sw) => ism.SerializeToStream(sw);
        public ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath) => ism.Deserialize(fileName, folderPath);
        public ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError) => ism.Deserialize(fileName, folderPath, askUserOnError);
        public ScoDictionaryNew<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) => ism.Deserialize(fileName, folderPath, askUserOnError, settings);
        public async Task<ScoDictionaryNew<TKey, TValue>> DeserializeAsync<U>(SmartSerializable<U> config) where U : class, ISmartSerializable<U>, new() => await ism.DeserializeAsync(config);
        public async Task<ScoDictionaryNew<TKey, TValue>> DeserializeAsync<U>(SmartSerializable<U> config, bool askUserOnError) where U : class, ISmartSerializable<U>, new() => await ism.DeserializeAsync(config, askUserOnError);
        
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

        #region Static Deserialization

        public static class Static
        {
            private static SmartSerializable<T> GetInstance<T>() where T : ScoDictionaryNew<TKey, TValue>, ISmartSerializable<T>, new() => new();

            public static JsonSerializerSettings GetSettingsJson<T>(IApplicationGlobals globals) where T : ScoDictionaryNew<TKey, TValue>, ISmartSerializable<T>, new()
            {
                var settings = new JsonSerializerSettings()
                {
                    //TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    TraceWriter = new NLogTraceWriter()
                };
                settings.Converters.Add(new AppGlobalsConverter(globals));
                settings.Converters.Add(new FilePathHelperConverter(globals.FS));
                settings.Converters.Add(new ScoDictionaryConverter<T, TKey, TValue>());
                return settings;
            }

            public static T Deserialize<T>(string fileName, string folderPath) where T : ScoDictionaryNew<TKey, TValue>, ISmartSerializable<T>, new() =>
                GetInstance<T>().Deserialize(fileName, folderPath);
                        
            public static T Deserialize<T>(string fileName, string folderPath, bool askUserOnError) where T : ScoDictionaryNew<TKey, TValue>, ISmartSerializable<T>, new() =>
                GetInstance<T>().Deserialize(fileName, folderPath, askUserOnError);

            public static T Deserialize<T>(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) 
                where T : ScoDictionaryNew<TKey, TValue>, ISmartSerializable<T>, new() =>
                GetInstance<T>().Deserialize(fileName, folderPath, askUserOnError, settings);

            public static T DeserializeObject<T>(string json, JsonSerializerSettings settings) where T : ScoDictionaryNew<TKey, TValue>, ISmartSerializable<T>, new()
            {
                return GetInstance<T>().DeserializeObject(json, settings);
            }

            public static T DeserializeObject<T>(string json, IApplicationGlobals globals) where T : ScoDictionaryNew<TKey, TValue>, ISmartSerializable<T>, new()
            {
                var settings = GetSettingsJson<T>(globals);
                return GetInstance<T>().DeserializeObject(json, settings);
            }

            public static async Task<T> DeserializeAsync<T, U>
                (SmartSerializable<U> config, bool askUserOnError, Func<T> altLoader)
                where T : ScoDictionaryNew<TKey, TValue>, ISmartSerializable<T>, new()
                where U : class, ISmartSerializable<U>, new() =>
                await GetInstance<T>().DeserializeAsync<U>(config, askUserOnError, altLoader);
        }

        #endregion Static Deserialization
    
    }
}
