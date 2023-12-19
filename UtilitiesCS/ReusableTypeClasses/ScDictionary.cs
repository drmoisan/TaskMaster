using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class ScDictionary<TKey, TValue>: ConcurrentDictionary<TKey, TValue>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public ScDictionary() : base() { }
        public ScDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { }
        public ScDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public ScDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) { }
        public ScDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity) { }
        public ScDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, collection, comparer) { }
        public ScDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer) { }

        #endregion Constructors
            
        #region Static Deserialization

        protected static ScDictionary<TKey, TValue> CreateEmpty(DialogResult response, FilePathHelper disk)
        {
            if (response == DialogResult.Yes)
            {
                var dictionary = new ScDictionary<TKey, TValue>();
                dictionary.Serialize(disk.FilePath);
                return dictionary;
            }
            else
            {
                throw new ArgumentNullException(
                "Must have a dictionary or create one to continue executing");
            }
        }

        protected static DialogResult AskUser(bool askUserOnError, string messageText)
        {
            DialogResult response;
            if (askUserOnError)
            {
                response = MessageBox.Show(
                    messageText,
                    "Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
            }
            else
            {
                response = DialogResult.Yes;
            }

            return response;
        }

        public static ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath)
        {
            return Deserialize(fileName, folderPath, false);
        }

        public static ScDictionary<TKey, TValue> Deserialize(string fileName, string folderPath, bool askUserOnError)
        {
            var disk = new FilePathHelper(fileName, folderPath);
            Deserialize(disk, askUserOnError);
            return new ScDictionary<TKey, TValue>();
        }

        internal static ScDictionary<TKey, TValue> Deserialize(FilePathHelper disk, bool askUserOnError)
        {
            ScDictionary<TKey, TValue> dictionary = null;
            bool writeDictionary = false;
            DialogResult response = DialogResult.Ignore;

            try
            {
                dictionary = DeserializeJson(disk);
                if (dictionary is null)
                {
                    throw new InvalidOperationException($"{disk.FilePath} deserialized to null.");
                }
            }
            catch (FileNotFoundException e)
            {
                logger.Error(e.Message);
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} not found. Need a dictionary to " +
                    $"continue. Create a new dictionary or abort execution?");
                dictionary = CreateEmpty(response, disk);
                writeDictionary = true;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error! {e.Message}");
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} encountered a problem. \n{e.Message}\n" +
                    $"Need a dictionary to continue. Create a new dictionary or abort execution?");
                dictionary = CreateEmpty(response, disk);
                writeDictionary = true;
            }

            if (writeDictionary)
            {
                dictionary.Serialize();
            }
            return dictionary;
        }

        protected static ScDictionary<TKey, TValue> DeserializeJson(FilePathHelper disk)
        {
            ScDictionary<TKey, TValue> collection;
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            collection = JsonConvert.DeserializeObject<ScDictionary<TKey, TValue>>(
                File.ReadAllText(disk.FilePath), settings);
            return collection;
        }

        #endregion Static Deserialization

        #region Serialization

        protected FilePathHelper _disk = new FilePathHelper();

        public string FilePath { get => _disk.FilePath; set => _disk.FilePath = value; }

        public string FolderPath { get => _disk.FolderPath; set => _disk.FolderPath = value; }

        public string FileName { get => _disk.FileName; set => _disk.FileName = value; }

        public void Serialize()
        {
            if (FilePath != "")
            {
                Serialize(FilePath);
            }
        }

        public void Serialize(string filePath)
        {
            this.FilePath = filePath;
            RequestSerialization(filePath);
        }

        protected static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public void SerializeThreadSafe(string filePath)
        {
            // Set Status to Locked
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        var settings = new JsonSerializerSettings();
                        settings.TypeNameHandling = TypeNameHandling.Auto;
                        settings.Formatting = Formatting.Indented;

                        var serializer = JsonSerializer.Create(settings);
                        serializer.Serialize(sw, this);
                        sw.Close();
                        _serializationRequested = new ThreadSafeSingleShotGuard();
                    }
                }
                catch (System.Exception e)
                {
                    logger.Error($"Error serializing to {filePath}", e);
                }
                finally
                {
                    // Release lock
                    _readWriteLock.ExitWriteLock();
                }
            }

        }

        private ThreadSafeSingleShotGuard _serializationRequested = new();
        private TimerWrapper _timer;
        protected void RequestSerialization(string filePath)
        {
            if (_serializationRequested.CheckAndSetFirstCall)
            {
                _timer = new TimerWrapper(TimeSpan.FromSeconds(3));
                _timer.Elapsed += (sender, e) => SerializeThreadSafe(filePath);
                _timer.AutoReset = false;
                _timer.StartTimer();
            }
        }

        #endregion Serialization

    }
}
