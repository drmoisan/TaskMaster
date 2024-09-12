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
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class SmartSerializable<T> : ISmartSerializable<T> where T : class, ISmartSerializable<T>, new()
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SmartSerializable()
        {
            _parent = null;
            Config = new SmartSerializableConfig();
        }

        public SmartSerializable(T parent)
        {
            _parent = parent;            
            Config = new SmartSerializableConfig();
        }

        protected T _parent;

        #region SerializationConfig

        private SmartSerializableConfig _config = new();
        public SmartSerializableConfig Config { get => _config; set => _config = value; }

        #endregion SerializationConfig

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
                instance.Config.JsonSettings = settings;
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

        public T TryDeserialize<U>(SmartSerializable<U> loader)
            where U : class, ISmartSerializable<U>, new()
        {
            try
            {
                return Deserialize(loader);
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.Message);
                return default;
            }
        }

        public T Deserialize<U>(SmartSerializable<U> loader)
            where U : class, ISmartSerializable<U>, new()
        {
            try
            {
                var disk = loader.ThrowIfNull().Config.ThrowIfNull().Disk.ThrowIfNull();
                var settings = loader.Config.JsonSettings.ThrowIfNull();
                return DeserializeJson(loader.Config.Disk, loader.Config.JsonSettings);
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.Message);
                throw;
            }
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

            instance.Config.Disk.FilePath = disk.FilePath;
            if (writeInstance)
            {
                instance.Serialize();
            }
            return instance;
        }

        public async Task<T> DeserializeAsync<U>(SmartSerializable<U> config) where U : class, ISmartSerializable<U>, new()
        {
            return await Task.Run(() => Deserialize(config));
        }

        protected T DeserializeJson(FilePathHelper disk, JsonSerializerSettings settings)
        {
            var instance = JsonConvert.DeserializeObject<T>(
                File.ReadAllText(disk.FilePath), settings);
            instance.Config.JsonSettings = settings;
            return instance;
        }

        protected T DeserializeJson(FilePathHelper disk)
        {
            var settings = GetDefaultSettings();
            return DeserializeJson(disk, settings);
        }

        #endregion Deserialization

        #region Serialization
                
        public void Serialize()
        {
            if (Config.Disk.FilePath != "")
            {
                RequestSerialization(Config.Disk.FilePath);
            }
        }

        public void Serialize(string filePath)
        {
            this.Config.Disk.FilePath = filePath;
            RequestSerialization(filePath);
        }

        protected ReaderWriterLockSlim _readWriteLock = new();

        public static JsonSerializerSettings GetDefaultSettings()
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
            _parent.ThrowIfNull($"{nameof(SmartSerializable<T>)}.{nameof(_parent)} is null. It must be linked to the instance it is serializing.");
            // Set Status to Locked
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    using (StreamWriter sw = CreateStreamWriter(filePath))
                    {
                        var serializer = JsonSerializer.Create(Config.JsonSettings);
                        serializer.Serialize(sw, _parent);
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

        #region Static
        
        public static class Static
        {
            private static SmartSerializable<T> GetInstance() => new();

            public static T Deserialize(string fileName, string folderPath) =>
                GetInstance().Deserialize(fileName, folderPath);

            public static T Deserialize(string fileName, string folderPath, bool askUserOnError) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError);

            public static T Deserialize(string fileName, string folderPath, bool askUserOnError, JsonSerializerSettings settings) =>
                GetInstance().Deserialize(fileName, folderPath, askUserOnError, settings);

            public static T Deserialize<U>(SmartSerializable<U> config) where U : class, ISmartSerializable<U>, new() =>
                GetInstance().Deserialize(config);

            public static async Task<T> DeserializeAsync<U>(SmartSerializable<U> config) where U : class, ISmartSerializable<U>, new() =>
                await GetInstance().DeserializeAsync(config);

            internal static JsonSerializerSettings GetDefaultSettings() =>
                SmartSerializable<T>.GetDefaultSettings();
        }
        
        #endregion Static
    }
}
