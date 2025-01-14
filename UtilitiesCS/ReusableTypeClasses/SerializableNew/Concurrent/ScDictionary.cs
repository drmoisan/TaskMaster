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

namespace UtilitiesCS.ReusableTypeClasses
{
    public class ScDictionary<TKey, TValue>: ConcurrentDictionary<TKey, TValue>, ISmartSerializable<ScDictionary<TKey, TValue>>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public ScDictionary() : base() { ism = new SmartSerializable<ScDictionary<TKey, TValue>>(this); }
        public ScDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { ism = new SmartSerializable<ScDictionary<TKey, TValue>>(this); }
        public ScDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { ism = new SmartSerializable<ScDictionary<TKey, TValue>>(this); }
        public ScDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) { ism = new SmartSerializable<ScDictionary<TKey, TValue>>(this); }
        public ScDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity) { ism = new SmartSerializable<ScDictionary<TKey, TValue>>(this); }
        public ScDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, collection, comparer) { ism = new SmartSerializable<ScDictionary<TKey, TValue>>(this); }
        public ScDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer) { ism = new SmartSerializable<ScDictionary<TKey, TValue>>(this); }
        public ScDictionary(ScDictionary<TKey, TValue> dictionary) : base(dictionary) { ism = dictionary.ism; }

        #endregion Constructors

        #region ISmartSerializable

        public NewSmartSerializableConfig Config { get => ism.Config; set => ism.Config = value; }
        protected SmartSerializable<ScDictionary<TKey, TValue>> ism;

        public void Serialize() => ism.Serialize();
        public void Serialize(string filePath) => ism.Serialize(filePath);
        public void SerializeThreadSafe(string filePath) => ism.SerializeThreadSafe(filePath);
        public ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath) => ism.Deserialize(fileName, folderPath);
        public ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError) => ism.Deserialize(fileName, folderPath, askUserOnError);
        public ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) => ism.Deserialize(fileName, folderPath, askUserOnError, settings);

        public static JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
        }

        ScDictionary<TKey, TValue> ISmartSerializable<ScDictionary<TKey, TValue>>.Deserialize<U>(SmartSerializable<U> loader)
        {
            throw new NotImplementedException();
        }

        ScDictionary<TKey, TValue> ISmartSerializable<ScDictionary<TKey, TValue>>.Deserialize<U>(SmartSerializable<U> loader, bool askUserOnError, Func<ScDictionary<TKey, TValue>> altLoader)
        {
            throw new NotImplementedException();
        }

        Task<ScDictionary<TKey, TValue>> ISmartSerializable<ScDictionary<TKey, TValue>>.DeserializeAsync<U>(SmartSerializable<U> config)
        {
            throw new NotImplementedException();
        }

        Task<ScDictionary<TKey, TValue>> ISmartSerializable<ScDictionary<TKey, TValue>>.DeserializeAsync<U>(SmartSerializable<U> config, bool askUserOnError)
        {
            throw new NotImplementedException();
        }

        Task<ScDictionary<TKey, TValue>> ISmartSerializable<ScDictionary<TKey, TValue>>.DeserializeAsync<U>(SmartSerializable<U> config, bool askUserOnError, Func<ScDictionary<TKey, TValue>> altLoader)
        {
            throw new NotImplementedException();
        }

        public ScDictionary<TKey, TValue> DeserializeObject(string json, JsonSerializerSettings settings)
        {
            throw new NotImplementedException();
        }

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
            private static SmartSerializable<ScDictionary<TKey, TValue>> GetInstance() => new();

            public static ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath) =>
                GetInstance().Deserialize(fileName, folderPath);

            public static ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError);

            public static ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError, settings);
        }




        #region Deactivated
        //#region Static Deserialization

        //protected static ScDictionary<TKey, TValue> CreateEmpty(DialogResult response, FilePathHelper disk)
        //{
        //    if (response == DialogResult.Yes)
        //    {
        //        var dictionary = new ScDictionary<TKey, TValue>();
        //        dictionary.Serialize(disk.FilePath);
        //        return dictionary;
        //    }
        //    else
        //    {
        //        throw new ArgumentNullException(
        //        "Must have a dictionary or create one to continue executing");
        //    }
        //}

        //protected static ScDictionary<TKey, TValue> CreateEmpty(DialogResult response, FilePathHelper disk, JsonSerializerSettings settings)
        //{
        //    if (response == DialogResult.Yes)
        //    {
        //        var dictionary = new ScDictionary<TKey, TValue>();
        //        dictionary.JsonSettings = settings;
        //        dictionary.Serialize(disk.FilePath);
        //        return dictionary;
        //    }
        //    else
        //    {
        //        throw new ArgumentNullException(
        //        "Must have a dictionary or create one to continue executing");
        //    }
        //}

        //protected static DialogResult AskUser(bool askUserOnError, string messageText)
        //{
        //    DialogResult response;
        //    if (askUserOnError)
        //    {
        //        response = MessageBox.Show(
        //            messageText,
        //            "Error",
        //            MessageBoxButtons.YesNo,
        //            MessageBoxIcon.Error);
        //    }
        //    else
        //    {
        //        response = DialogResult.Yes;
        //    }

        //    return response;
        //}

        //public static ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath)
        //{
        //    return Deserialize(fileName, folderPath, false);
        //}

        //public static ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError)
        //{
        //    var disk = new FilePathHelper(fileName, folderPath);            
        //    var settings = GetDefaultSettings();
        //    return Deserialize(disk, askUserOnError, settings);
        //}

        //public static ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) 
        //{
        //    var disk = new FilePathHelper(fileName, folderPath);
        //    return Deserialize(disk, askUserOnError, settings);
        //}

        //internal static ScDictionary<TKey, TValue> Deserialize(FilePathHelper disk, bool askUserOnError, JsonSerializerSettings settings)
        //{
        //    bool writeDictionary = false;
        //    ScDictionary<TKey, TValue> dictionary;
        //    DialogResult response;

        //    try
        //    {
        //        dictionary = DeserializeJson(disk, settings);
        //        if (dictionary is null)
        //        {
        //            throw new InvalidOperationException($"{disk.FilePath} deserialized to null.");
        //        }

        //    }
        //    catch (FileNotFoundException e)
        //    {
        //        logger.Error(e.Message);
        //        response = AskUser(askUserOnError,
        //            $"{disk.FilePath} not found. Need a dictionary to " +
        //            $"continue. Create a new dictionary or abort execution?");
        //        dictionary = CreateEmpty(response, disk, settings);
        //        writeDictionary = true;
        //    }
        //    catch (System.Exception e)
        //    {
        //        logger.Error($"Error! {e.Message}");
        //        response = AskUser(askUserOnError,
        //            $"{disk.FilePath} encountered a problem. \n{e.Message}\n" +
        //            $"Need a dictionary to continue. Create a new dictionary or abort execution?");
        //        dictionary = CreateEmpty(response, disk, settings);
        //        writeDictionary = true;
        //    }

        //    dictionary.FilePath = disk.FilePath;
        //    if (writeDictionary)
        //    {
        //        dictionary.Serialize();
        //    }
        //    return dictionary;
        //}

        //protected static ScDictionary<TKey, TValue> DeserializeJson(FilePathHelper disk, JsonSerializerSettings settings) 
        //{
        //    var collection = JsonConvert.DeserializeObject<ScDictionary<TKey, TValue>>(
        //        File.ReadAllText(disk.FilePath), settings);
        //    collection.JsonSettings = settings;
        //    return collection;
        //}

        //protected static ScDictionary<TKey, TValue> DeserializeJson(FilePathHelper disk)
        //{
        //    var settings = GetDefaultSettings();
        //    return DeserializeJson(disk, settings);
        //}

        //#endregion Static Deserialization

        //#region Serialization

        //protected FilePathHelper _disk = new FilePathHelper();

        //public string FilePath { get => _disk.FilePath; set => _disk.FilePath = value; }

        //public string FolderPath { get => _disk.FolderPath; set => _disk.FolderPath = value; }

        //public string FileName { get => _disk.FileName; set => _disk.FileName = value; }

        //public FilePathHelper LocalDisk { get => _localDisk; set => _localDisk = value; }
        //private FilePathHelper _localDisk = new FilePathHelper();

        //public FilePathHelper NetDisk { get => _netDisk; set => _netDisk = value; }
        //private FilePathHelper _netDisk = new();

        //public void ActivateLocalDisk()
        //{
        //    _disk = _localDisk;
        //    _jsonSettings = _localJsonSettings;
        //}

        //public void ActivateNetDisk()
        //{
        //    _disk = _netDisk;
        //    _jsonSettings = _netJsonSettings;
        //}

        //public void Serialize()
        //{
        //    if (FilePath != "")
        //    {
        //        Serialize(FilePath);
        //    }
        //}

        //public void Serialize(string filePath)
        //{
        //    this.FilePath = filePath;
        //    RequestSerialization(filePath);
        //}

        //protected static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        //[JsonIgnore]
        //public JsonSerializerSettings JsonSettings { get => _jsonSettings; set => _jsonSettings = value; }
        //private JsonSerializerSettings _jsonSettings = GetDefaultSettings();

        //[JsonIgnore]
        //public JsonSerializerSettings NetJsonSettings { get => _netJsonSettings; set => _netJsonSettings = value; }
        //private JsonSerializerSettings _netJsonSettings;

        //[JsonIgnore]
        //public JsonSerializerSettings LocalJsonSettings { get => _localJsonSettings; set => _localJsonSettings = value; }
        //private JsonSerializerSettings _localJsonSettings;
                
        //public void SerializeThreadSafe(string filePath)
        //{
        //    // Set Status to Locked
        //    if (_readWriteLock.TryEnterWriteLock(-1))
        //    {
        //        try
        //        {
        //            using (StreamWriter sw = File.CreateText(filePath))
        //            {
        //                var serializer = JsonSerializer.Create(JsonSettings);
        //                serializer.Serialize(sw, this);
        //                sw.Close();
        //                _serializationRequested = new ThreadSafeSingleShotGuard();
        //            }
        //        }
        //        catch (System.Exception e)
        //        {
        //            logger.Error($"Error serializing to {filePath}", e);
        //        }
        //        finally
        //        {
        //            // Release lock
        //            _readWriteLock.ExitWriteLock();
        //        }
        //    }

        //}

        //private ThreadSafeSingleShotGuard _serializationRequested = new();
        //private TimerWrapper _timer;
        //protected void RequestSerialization(string filePath)
        //{
        //    if (_serializationRequested.CheckAndSetFirstCall)
        //    {
        //        _timer = new TimerWrapper(TimeSpan.FromSeconds(3));
        //        _timer.Elapsed += (sender, e) => SerializeThreadSafe(filePath);
        //        _timer.AutoReset = false;
        //        _timer.StartTimer();
        //    }
        //}

        //#endregion Serialization
        #endregion Deactivated

    }
}
