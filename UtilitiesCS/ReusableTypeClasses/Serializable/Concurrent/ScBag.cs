using log4net.Repository.Hierarchy;
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
using System.Windows.Input;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class ScBag<T>: ConcurrentBag<T>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public ScBag() :base() { }
        public ScBag(IEnumerable<T> collection) : base(collection) { }

        #endregion Constructors

        #region Static Deserialization

        protected static ScBag<T> CreateEmpty(DialogResult response, FilePathHelper disk)
        {
            if (response == DialogResult.Yes)
            {
                var bag = new ScBag<T>();
                bag.Serialize(disk.FilePath);
                return bag;
            }
            else
            {
                throw new ArgumentNullException(
                "Must have a collection or create one to continue executing");
            }
        }

        protected static ScBag<T> CreateEmpty(DialogResult response, FilePathHelper disk, JsonSerializerSettings settings)
        {
            if (response == DialogResult.Yes)
            {
                var bag = new ScBag<T>();
                bag.JsonSettings = settings;
                bag.Serialize(disk.FilePath);
                return bag;
            }
            else
            {
                throw new ArgumentNullException(
                "Must have a collection or create one to continue executing");
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

        public static ScBag<T> Deserialize(string fileName, string folderPath)
        {
            return Deserialize(fileName, folderPath, false);
        }

        public static ScBag<T> Deserialize(string fileName, string folderPath, bool askUserOnError)
        {
            var disk = new FilePathHelper(fileName, folderPath);
            var settings = GetDefaultSettings();
            return Deserialize(disk, askUserOnError, settings);
        }

        public static ScBag<T> Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings)
        {
            var disk = new FilePathHelper(fileName, folderPath);
            return Deserialize(disk, askUserOnError, settings);
        }

        internal static ScBag<T> Deserialize(FilePathHelper disk, bool askUserOnError, JsonSerializerSettings settings)
        {
            bool writeBag = false;
            ScBag<T> bag;
            DialogResult response;

            try
            {
                bag = DeserializeJson(disk, settings);
                if (bag is null)
                {
                    throw new InvalidOperationException($"{disk.FilePath} deserialized to null.");
                }

            }
            catch (FileNotFoundException e)
            {
                logger.Error(e.Message);
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} not found. Need a collection to " +
                    $"continue. Create a new collection or abort execution?");
                bag = CreateEmpty(response, disk, settings);
                writeBag = true;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error! {e.Message}");
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} encountered a problem. \n{e.Message}\n" +
                    $"Need a collection to continue. Create a new collection or abort execution?");
                bag = CreateEmpty(response, disk, settings);
                writeBag = true;
            }

            bag.FilePath = disk.FilePath;
            if (writeBag)
            {
                bag.Serialize();
            }
            return bag;
        }

        protected static ScBag<T> DeserializeJson(FilePathHelper disk, JsonSerializerSettings settings)
        {
            var collection = JsonConvert.DeserializeObject<ScBag<T>>(
                File.ReadAllText(disk.FilePath), settings);
            collection.JsonSettings = settings;
            return collection;
        }

        protected static ScBag<T> DeserializeJson(FilePathHelper disk)
        {
            var settings = GetDefaultSettings();
            return DeserializeJson(disk, settings);
        }

        #endregion Static Deserialization

        #region Serialization

        protected FilePathHelper _disk = new();

        public string FilePath { get => _disk.FilePath; set => _disk.FilePath = value; }

        public string FolderPath { get => _disk.FolderPath; set => _disk.FolderPath = value; }

        public string FileName { get => _disk.FileName; set => _disk.FileName = value; }

        public FilePathHelper LocalDisk { get => _localDisk; set => _localDisk = value; }
        private FilePathHelper _localDisk = new();

        public FilePathHelper NetDisk { get => _netDisk; set => _netDisk = value; }
        private FilePathHelper _netDisk = new();

        public void ActivateLocalDisk()
        {
            _disk = _localDisk;
            _jsonSettings = _localJsonSettings;
        }

        public void ActivateNetDisk()
        {
            _disk = _netDisk;
            _jsonSettings = _netJsonSettings;
        }

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

        [JsonIgnore]
        public JsonSerializerSettings JsonSettings { get => _jsonSettings; set => _jsonSettings = value; }
        private JsonSerializerSettings _jsonSettings = GetDefaultSettings();

        [JsonIgnore]
        public JsonSerializerSettings NetJsonSettings { get => _netJsonSettings; set => _netJsonSettings = value; }
        private JsonSerializerSettings _netJsonSettings;

        [JsonIgnore]
        public JsonSerializerSettings LocalJsonSettings { get => _localJsonSettings; set => _localJsonSettings = value; }
        private JsonSerializerSettings _localJsonSettings;

        public static JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
        }

        public void SerializeThreadSafe(string filePath)
        {
            // Set Status to Locked
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        var serializer = JsonSerializer.Create(JsonSettings);
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
