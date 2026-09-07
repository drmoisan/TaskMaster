#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Azure;
using Newtonsoft.Json;
using UtilitiesCS.Extensions;
using UtilitiesCS.Extensions.Lazy;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Interfaces;
using UtilitiesCS.Threading;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class SmartSerializable<T> : ISmartSerializable<T>
        where T : class, ISmartSerializable<T>, new()
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public SmartSerializable()
        {
            _parent = null;
            Config = new NewSmartSerializableConfig();
        }

        public SmartSerializable(T parent)
        {
            _parent = parent;
            Config = new NewSmartSerializableConfig();
        }

        protected T? _parent;
        private Func<string, string> _readAllText = File.ReadAllText;
        protected Func<string, string> ReadAllText
        {
            get => _readAllText;
            set => _readAllText = value;
        }

        private Func<FilePathHelper, bool> _diskExists = disk => disk.Exists();
        protected Func<FilePathHelper, bool> DiskExists
        {
            get => _diskExists;
            set => _diskExists = value;
        }

        private Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> _showDialog =
            (messageText, caption, buttons, icon) =>
                MyBox.ShowDialog(messageText, caption, buttons, icon);
        protected Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> ShowDialog
        {
            get => _showDialog;
            set => _showDialog = value;
        }

        #region SerializationConfig

        [JsonProperty]
        public NewSmartSerializableConfig Config
        {
            get => _config;
            set
            {
                if (_config is not null)
                    _config.PropertyChanged -= Config_PropertyChanged;
                _config = value;
                if (_config is not null)
                    _config.PropertyChanged += Config_PropertyChanged;
            }
        }

        private NewSmartSerializableConfig _config = new NewSmartSerializableConfig();

        #endregion SerializationConfig

        public string? Name { get; set; }

        #region INotifyPropertyChanged

        private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //var properties = string.Join(",",e.PropertyName.Split(',').Select(name => $"{typeof(T).Name}.{name}"));
            //var properties = $"{typeof(T).Name},{e.PropertyName}";
            //Notify(properties);
            Notify(e.PropertyName);
        }

        public void Notify(
            [System.Runtime.CompilerServices.CallerMemberName] string propertyName = ""
        )
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion INotifyPropertyChanged

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
                    $"Must have an instance of {typeof(T)} or create one to continue executing"
                );
            }
        }

        protected T CreateEmpty(
            DialogResult response,
            FilePathHelper disk,
            JsonSerializerSettings settings,
            Func<T>? altLoader
        )
        {
            if (response == DialogResult.Yes)
            {
                var instance = altLoader is null ? new T() : altLoader();
                instance.Config.JsonSettings = settings;
                instance.Serialize(disk.FilePath);
                return instance;
            }
            else
            {
                throw new ArgumentNullException(
                    $"Must have an instance of {typeof(T)} or create one to continue executing"
                );
            }
        }

        protected T CreateEmpty(
            DialogResult response,
            FilePathHelper disk,
            JsonSerializerSettings settings
        )
        {
            return CreateEmpty(response, disk, settings, null);
        }

        protected DialogResult AskUser(bool askUserOnError, string messageText)
        {
            DialogResult response;
            if (askUserOnError)
            {
                response = ShowDialog(
                    messageText,
                    "Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                );
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

        public T Deserialize(
            string fileName,
            string folderPath,
            bool askUserOnError,
            JsonSerializerSettings settings
        )
        {
            var disk = new FilePathHelper(fileName, folderPath);
            return Deserialize(disk, askUserOnError, settings);
        }

        public T? TryDeserialize<U>(SmartSerializable<U> loader)
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
                T? instance = DeserializeJson(loader.Config.Disk, loader.Config.JsonSettings);
                if (instance is not null)
                {
                    instance.Config.CopyFrom(loader.Config, true);
                }
                // Interface contract is non-null; conform while preserving runtime behavior.
                return instance!;
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.Message);
                throw;
            }
        }

        public T? Deserialize<U>(ISmartSerializable<U> loader)
            where U : class, ISmartSerializable<U>, new()
        {
            try
            {
                var disk = loader.ThrowIfNull().Config.ThrowIfNull().Disk.ThrowIfNull();
                var settings = loader.Config.JsonSettings.ThrowIfNull();
                T? instance = DeserializeJson(loader.Config.Disk, loader.Config.JsonSettings);
                if (instance is not null)
                {
                    instance.Config.CopyFrom(loader.Config, true);
                }
                return instance;
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.Message);
                throw;
            }
        }

        public T Deserialize<U>(
            SmartSerializable<U> loader,
            bool askUserOnError,
            Func<T>? altLoader
        )
            where U : class, ISmartSerializable<U>, new()
        {
            //Func<T> altLoader = null;
            var disk = loader.ThrowIfNull().Config.ThrowIfNull().Disk.ThrowIfNull();
            var settings = loader.Config.JsonSettings.ThrowIfNull();
            bool writeInstance = false;
            T? instance = default;

            try
            {
                instance = DeserializeJson(loader.Config.Disk, loader.Config.JsonSettings);
                if (instance is null)
                {
                    throw new InvalidOperationException($"{disk.FilePath} deserialized to null.");
                }
            }
            catch (FileNotFoundException e)
            {
                logger.Error(e.Message);
                var response = AskUser(
                    askUserOnError,
                    $"{disk.FilePath} not found. Need an instance of {typeof(T)} to "
                        + $"continue. Create a new dictionary or abort execution?"
                );
                instance = CreateEmpty(response, disk, settings, altLoader);
                writeInstance = true;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error! {e.Message}");
                var response = AskUser(
                    askUserOnError,
                    $"{disk.FilePath} encountered a problem. \n{e.Message}\n"
                        + $"Need a dictionary to continue. Create a new dictionary or abort execution?"
                );
                instance = CreateEmpty(response, disk, settings, altLoader);
                writeInstance = true;
            }
            // Non-null by construction: the try assigns a non-null instance or throws, and both
            // catch paths assign a non-null CreateEmpty result, so instance is set before use.
            instance!.Config.CopyFrom(loader.Config, true);

            if (writeInstance)
            {
                instance!.Serialize();
            }

            return instance!;
        }

        protected T Deserialize(
            FilePathHelper disk,
            bool askUserOnError,
            JsonSerializerSettings settings
        )
        {
            bool writeInstance = false;
            T? instance = default;
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
                response = AskUser(
                    askUserOnError,
                    $"{disk.FilePath} not found. Need an instance of {typeof(T)} to "
                        + $"continue. Create a new dictionary or abort execution?"
                );
                instance = CreateEmpty(response, disk, settings);
                writeInstance = true;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error! {e.Message}");
                response = AskUser(
                    askUserOnError,
                    $"{disk.FilePath} encountered a problem. \n{e.Message}\n"
                        + $"Need a dictionary to continue. Create a new dictionary or abort execution?"
                );
                instance = CreateEmpty(response, disk, settings);
                writeInstance = true;
            }

            // Non-null by construction: the try assigns a non-null instance or throws, and both
            // catch paths assign a non-null CreateEmpty result, so instance is set before use.
            instance!.Config.Disk.FilePath = disk.FilePath;

            if (writeInstance)
            {
                instance!.Serialize();
            }
            return instance!;
        }

        public async Task<T> DeserializeAsync<U>(SmartSerializable<U> config)
            where U : class, ISmartSerializable<U>, new()
        {
            // Deserialize can yield null, but the ISmartSerializable<T> contract is non-null; the !
            // conforms to the interface signature without changing runtime behavior.
            return (await Task.Run(() => Deserialize(config)))!;
        }

        public async Task<T> DeserializeAsync<U>(SmartSerializable<U> config, bool askUserOnError)
            where U : class, ISmartSerializable<U>, new()
        {
            return await Task.Run(() => Deserialize(config, askUserOnError, null));
        }

        public async Task<T> DeserializeAsync<U>(
            SmartSerializable<U> config,
            bool askUserOnError,
            Func<T> altLoader
        )
            where U : class, ISmartSerializable<U>, new()
        {
            return await Task.Run(() => Deserialize(config, askUserOnError, altLoader));
        }

        protected T? DeserializeJson(FilePathHelper disk, JsonSerializerSettings settings)
        {
            T? instance = null;
            if (!DiskExists(disk))
            {
                return instance;
            }
            try
            {
                instance = JsonConvert.DeserializeObject<T>(ReadAllText(disk.FilePath), settings);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            if (instance is not null)
            {
                instance.Config.JsonSettings = settings;
            }
            return instance;
        }

        public T DeserializeObject(string json, JsonSerializerSettings settings)
        {
            T? instance = null;
            try
            {
                instance = JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            if (instance is not null)
            {
                instance.Config.JsonSettings = settings.DeepCopy();
            }
            // why: ISmartSerializable<T>.DeserializeObject (null-oblivious, out of this child's
            // scope) declares a T return; matching that oblivious contract preserves the existing
            // public signature (AC5). The value can be null on deserialization failure, which the
            // oblivious interface already permits callers to handle.
            return instance!;
        }

        protected T? DeserializeJson(FilePathHelper disk)
        {
            var settings = GetDefaultSettings();
            return DeserializeJson(disk, settings);
        }

        #endregion Deserialization

        #region Serialization

        /// <summary>
        /// Reports whether a write may proceed, and logs the reason at error level when it may not
        /// (issue #797, AC2). The previous guard compared only against the empty string, so a null
        /// file path passed it and reached the write path, and an empty path returned silently with
        /// no diagnostic at all. Shared by the deferred and the explicit-save entry points so both
        /// report the same diagnostic and neither fails silently.
        /// </summary>
        /// <param name="filePath">Receives the configured file path.</param>
        /// <returns>True when the configured path is neither null nor empty.</returns>
        private bool TryGetSerializationPath(out string filePath)
        {
            filePath = Config.Disk.FilePath;
            if (!string.IsNullOrEmpty(filePath))
            {
                return true;
            }

            logger.Error(
                $"Cannot serialize {typeof(T)}: Config.Disk.FilePath is null or empty "
                    + $"(value: '{filePath}'), so the instance was not written to disk."
            );
            return false;
        }

        public void Serialize()
        {
            if (TryGetSerializationPath(out var filePath))
            {
                RequestSerialization(filePath);
            }
        }

        public void Serialize(string filePath)
        {
            this.Config.Disk.FilePath = filePath;
            RequestSerialization(filePath);
        }

        /// <summary>
        /// Explicit-save entry point (issue #797, AC4). Callers that must not lose a write when the
        /// host process exits inside the deferred three-second window call this instead of
        /// <see cref="Serialize()"/>.
        /// </summary>
        public void SerializeNow()
        {
            // why: issue #797 AC4. The deferred write is raised on a ThreadPool background thread
            // three seconds after the request, and a background thread is not joined at process
            // exit, so a save issued inside that window is lost when the host tears down the
            // AppDomain, with no log entry. An explicit save therefore writes inline through the
            // existing thread-safe write method, which takes the write lock, writes through the
            // injectable stream-writer seam, and re-arms the single-shot guard in its finally block.
            // The AC2 guard is evaluated first so this fix does not substitute one silent failure
            // for another. Every other caller keeps the unchanged deferred behaviour.
            if (TryGetSerializationPath(out var filePath))
            {
                SerializeThreadSafe(filePath);
            }
        }

        protected ReaderWriterLockSlim _readWriteLock = new();

        public static JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
        }

        private Func<string, StreamWriter> _createStreamWriter = File.CreateText;
        protected Func<string, StreamWriter> CreateStreamWriter
        {
            get => _createStreamWriter;
            set => _createStreamWriter = value;
        }

        public void SerializeThreadSafe(string filePath)
        {
            _parent.ThrowIfNull(
                $"{nameof(SmartSerializable<T>)}.{nameof(_parent)} is null. It must be linked to the instance it is serializing."
            );
            // Set Status to Locked
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    using (StreamWriter sw = CreateStreamWriter(filePath))
                    {
                        SerializeToStream(sw);
                        sw.Close();
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
                    _serializationRequested = new ThreadSafeSingleShotGuard();
                }
            }
        }

        public string SerializeToString()
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            try
            {
                SerializeToStream(streamWriter);
                streamWriter.Flush();
                memoryStream.Position = 0;
            }
            catch (Exception e)
            {
                logger.Error($"Error serializing to string", e);
                return "";
            }
            using var streamReader = new StreamReader(memoryStream);
            return streamReader.ReadToEnd();
        }

        public void SerializeToStream(StreamWriter sw)
        {
            sw.ThrowIfNull();
            var serializer = JsonSerializer.Create(Config.JsonSettings);

            if (Config.JsonSettings.TypeNameHandling == TypeNameHandling.Auto)
            {
                // Serialization requires a linked parent; public entry points guard via ThrowIfNull.
                serializer.Serialize(sw, _parent, _parent!.GetType());
            }
            else
            {
                serializer.Serialize(sw, _parent);
            }
        }

        private ThreadSafeSingleShotGuard _serializationRequested = new();
        private ITimerWrapper? _timer;

        /// <summary>
        /// Factory used to create the deferred-serialization timer. Defaults to a real
        /// <see cref="TimerWrapper"/>, preserving production behavior. Tests override this to
        /// inject a deterministic, manually-fired timer so the deferred write can be triggered
        /// without a wall-clock wait.
        /// </summary>
        protected Func<TimeSpan, ITimerWrapper> TimerFactory { get; set; } =
            interval => new TimerWrapper(interval);

        protected void RequestSerialization(string filePath)
        {
            if (_serializationRequested.CheckAndSetFirstCall)
            {
                _timer = TimerFactory(TimeSpan.FromSeconds(3));
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

            public static T Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError,
                JsonSerializerSettings settings
            ) => GetInstance().Deserialize(fileName, folderPath, askUserOnError, settings);

            public static T? Deserialize<U>(SmartSerializable<U> config)
                where U : class, ISmartSerializable<U>, new() => GetInstance().Deserialize(config);

            public static T? DeseriealizeObject(string json, JsonSerializerSettings settings) =>
                GetInstance().DeserializeObject(json, settings);

            public static async Task<T?> DeserializeAsync<U>(SmartSerializable<U> config)
                where U : class, ISmartSerializable<U>, new() =>
                await GetInstance().DeserializeAsync(config);

            public static async Task<T> DeserializeAsync<U>(
                SmartSerializable<U> config,
                bool askUserOnError
            )
                where U : class, ISmartSerializable<U>, new() =>
                await GetInstance().DeserializeAsync(config, askUserOnError);

            public static async Task<T> DeserializeAsync<U>(
                SmartSerializable<U> config,
                bool askUserOnError,
                Func<T> altLoader
            )
                where U : class, ISmartSerializable<U>, new() =>
                await GetInstance().DeserializeAsync(config, askUserOnError, altLoader);

            internal static JsonSerializerSettings GetDefaultSettings() =>
                SmartSerializable<T>.GetDefaultSettings();
        }

        #endregion Static
    }
}
