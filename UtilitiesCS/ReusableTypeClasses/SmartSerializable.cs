using BrightIdeasSoftware;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using System;
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
    public class SmartSerializable<T>: ISmartSerializable<T> where T : class, ISmartSerializable<T>, new()
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SmartSerializable() { _jsonSettings = GetDefaultSettings(); }

        #region Deserialization

        protected T CreateEmpty(DialogResult response, FilePathHelper disk)
        {
            if (response == DialogResult.Yes)
            {
                var instance = new T();
                instance.Serialize(disk.FilePath);
                return instance;
            }
            else
            {
                throw new ArgumentNullException(
                $"Must have an instance of {typeof(T)} or create one to continue executing");
            }
        }

        protected T CreateEmpty(DialogResult response, FilePathHelper disk, JsonSerializerSettings settings)
        {
            if (response == DialogResult.Yes)
            {
                var instance = new T();
                instance.JsonSettings = settings;
                instance.Serialize(disk.FilePath);
                return instance;
            }
            else
            {
                throw new ArgumentNullException(
                $"Must have an instance of {typeof(T)} or create one to continue executing");
            }
        }

        protected DialogResult AskUser(bool askUserOnError, string messageText)
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

        public T Deserialize(string fileName, string folderPath)
        {
            return Deserialize(fileName, folderPath, false);
        }

        public T Deserialize(string fileName, string folderPath, bool askUserOnError)
        {
            var disk = new FilePathHelper(fileName, folderPath);
            var settings = GetDefaultSettings();
            return Deserialize(disk, askUserOnError, settings);
        }

        public T Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings)
        {
            var disk = new FilePathHelper(fileName, folderPath);
            return Deserialize(disk, askUserOnError, settings);
        }

        protected T Deserialize(FilePathHelper disk, bool askUserOnError, JsonSerializerSettings settings)
        {
            bool writeInstance = false;
            T instance;
            DialogResult response;

            try
            {
                instance = DeserializeJson(disk, settings);
                if (instance is null)
                {
                    throw new InvalidOperationException($"{disk.FilePath} deserialized to null.");
                }

            }
            catch (FileNotFoundException e)
            {
                logger.Error(e.Message);
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} not found. Need an instance of {typeof(T)} to " +
                    $"continue. Create a new dictionary or abort execution?");
                instance = CreateEmpty(response, disk, settings);
                writeInstance = true;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error! {e.Message}");
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} encountered a problem. \n{e.Message}\n" +
                    $"Need a dictionary to continue. Create a new dictionary or abort execution?");
                instance = CreateEmpty(response, disk, settings);
                writeInstance = true;
            }

            instance.FilePath = disk.FilePath;
            if (writeInstance)
            {
                instance.Serialize();
            }
            return instance;
        }

        protected T DeserializeJson(FilePathHelper disk, JsonSerializerSettings settings)
        {
            var instance = JsonConvert.DeserializeObject<T>(
                File.ReadAllText(disk.FilePath), settings);
            instance.JsonSettings = settings;
            return instance;
        }

        protected T DeserializeJson(FilePathHelper disk)
        {
            var settings = GetDefaultSettings();
            return DeserializeJson(disk, settings);
        }

        #endregion Deserialization

        #region Serialization

        protected FilePathHelper _disk = new FilePathHelper();

        public string FilePath { get => _disk.FilePath; set => _disk.FilePath = value; }

        public string FolderPath { get => _disk.FolderPath; set => _disk.FolderPath = value; }

        public string FileName { get => _disk.FileName; set => _disk.FileName = value; }

        public FilePathHelper LocalDisk { get => _localDisk; set => _localDisk = value; }
        private FilePathHelper _localDisk = new FilePathHelper();

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

        protected ReaderWriterLockSlim _readWriteLock = new();

        [JsonIgnore]
        public JsonSerializerSettings JsonSettings { get => _jsonSettings; set => _jsonSettings = value; }
        private JsonSerializerSettings _jsonSettings;

        [JsonIgnore]
        public JsonSerializerSettings NetJsonSettings { get => _netJsonSettings; set => _netJsonSettings = value; }
        private JsonSerializerSettings _netJsonSettings;

        [JsonIgnore]
        public JsonSerializerSettings LocalJsonSettings { get => _localJsonSettings; set => _localJsonSettings = value; }
        private JsonSerializerSettings _localJsonSettings;
        //= GetDefaultSettings()

        public JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
        }

        private Func<string, StreamWriter> _createStreamWriter = File.CreateText;
        protected Func<string, StreamWriter> CreateStreamWriter { get => _createStreamWriter; set => _createStreamWriter = value; }
        
        public void SerializeThreadSafe(string filePath)
        {
            // Set Status to Locked
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    using (StreamWriter sw = CreateStreamWriter(filePath))
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
