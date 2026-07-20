#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using UtilitiesCS.Extensions;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Interfaces;
using UtilitiesCS.Properties;
using UtilitiesCS.Threading;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class SmartSerializableBase
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

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

        public SmartSerializableBase() { }

        #region Deserialization

        protected T CreateEmpty<T>(DialogResult response, FilePathHelper disk)
            where T : class, new()
        {
            if (response == DialogResult.Yes)
            {
                var instance = new T();
                Serialize(instance, disk.FilePath);
                //var serialize = typeof(T).GetMethod("Serialize", [typeof(FilePathHelper)]);
                //serialize?.Invoke(instance, [disk.FilePath]);
                return instance;
            }
            else
            {
                throw new ArgumentNullException(
                    $"Must have an instance of {typeof(T)} or create one to continue executing"
                );
            }
        }

        protected T CreateEmpty<T>(
            DialogResult response,
            FilePathHelper disk,
            JsonSerializerSettings settings,
            Func<T>? altLoader
        )
            where T : class, new()
        {
            if (response == DialogResult.Yes)
            {
                var instance = altLoader is null ? new T() : altLoader();

                SetConfig(instance, settings.DeepCopy());

                Serialize(instance, disk.FilePath);

                return instance;
            }
            else
            {
                throw new ArgumentNullException(
                    $"Must have an instance of {typeof(T)} or create one to continue executing"
                );
            }
        }

        protected T CreateEmpty<T>(
            DialogResult response,
            FilePathHelper disk,
            JsonSerializerSettings settings
        )
            where T : class, new()
        {
            return CreateEmpty<T>(response, disk, settings, null);
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

        public T Deserialize<T>(string fileName, string folderPath)
            where T : class, new()
        {
            return Deserialize<T>(fileName, folderPath, false);
        }

        public T Deserialize<T>(string fileName, string folderPath, bool askUserOnError)
            where T : class, new()
        {
            var disk = new FilePathHelper(fileName, folderPath);
            var settings = GetDefaultSettings();
            return Deserialize<T>(disk, askUserOnError, settings);
        }

        public T Deserialize<T>(
            string fileName,
            string folderPath,
            bool askUserOnError,
            JsonSerializerSettings settings
        )
            where T : class, new()
        {
            var disk = new FilePathHelper(fileName, folderPath);
            return Deserialize<T>(disk, askUserOnError, settings);
        }

        public T? TryDeserialize<T, U>(SmartSerializable<U> loader)
            where T : class, new()
            where U : class, ISmartSerializable<U>, new()
        {
            try
            {
                return Deserialize<T, U>(loader);
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.Message);
                return default;
            }
        }

        public T? Deserialize<T, U>(SmartSerializable<U> loader)
            where T : class, new()
            where U : class, ISmartSerializable<U>, new()
        {
            try
            {
                var disk = loader.ThrowIfNull().Config.ThrowIfNull().Disk.ThrowIfNull();
                var settings = loader.Config.JsonSettings.ThrowIfNull();
                T? instance = DeserializeJson<T>(loader.Config.Disk, loader.Config.JsonSettings);
                if (instance is not null)
                {
                    var config = GetConfig(instance);
                    config?.CopyFrom(loader.Config, true);
                }
                return instance;
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.Message);
                throw;
            }
        }

        public T Deserialize<T, U>(
            SmartSerializable<U> loader,
            bool askUserOnError,
            Func<T>? altLoader
        )
            where T : class, new()
            where U : class, ISmartSerializable<U>, new()
        {
            //Func<T> altLoader = null;
            var disk = loader.ThrowIfNull().Config.ThrowIfNull().Disk.ThrowIfNull();
            var settings = loader.Config.JsonSettings.ThrowIfNull();
            bool writeInstance = false;
            T? instance = default;

            try
            {
                instance = DeserializeJson<T>(loader.Config.Disk, loader.Config.JsonSettings);
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
                instance = CreateEmpty<T>(response, disk, settings, altLoader);
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
            GetConfig(instance!)?.CopyFrom(loader.Config, true);
            //instance.Config.CopyFrom(loader.Config, true);

            if (writeInstance)
            {
                Serialize(instance!);
            }

            return instance!;
        }

        protected T Deserialize<T>(
            FilePathHelper disk,
            bool askUserOnError,
            JsonSerializerSettings settings
        )
            where T : class, new()
        {
            bool writeInstance = false;
            T? instance = default;
            DialogResult response;

            try
            {
                instance = DeserializeJson<T>(disk, settings);
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
                instance = CreateEmpty<T>(response, disk, settings);
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
                instance = CreateEmpty<T>(response, disk, settings);
                writeInstance = true;
            }

            // Non-null by construction: the try assigns a non-null instance or throws, and both
            // catch paths assign a non-null CreateEmpty result, so instance is set before use.
            var config = GetConfig(instance!);
            if (config is not null)
            {
                config.JsonSettings = settings.DeepCopy();
                config.Disk.FilePath = disk.FilePath;
            }

            if (writeInstance)
            {
                Serialize(instance!);
            }
            return instance!;
        }

        public async Task<T> DeserializeAsync<T, U>(SmartSerializable<U> config)
            where T : class, new()
            where U : class, ISmartSerializable<U>, new()
        {
            // Deserialize can yield null, but this overload's contract is non-null (callers rely on
            // it); the ! conforms to the contract without changing runtime behavior.
            return (await Task.Run(() => Deserialize<T, U>(config)))!;
        }

        public async Task<T> DeserializeAsync<T, U>(
            SmartSerializable<U> config,
            bool askUserOnError
        )
            where T : class, new()
            where U : class, ISmartSerializable<U>, new()
        {
            return await Task.Run(() => Deserialize<T, U>(config, askUserOnError, null));
        }

        public async Task<T> DeserializeAsync<T, U>(
            SmartSerializable<U> config,
            bool askUserOnError,
            Func<T> altLoader
        )
            where T : class, new()
            where U : class, ISmartSerializable<U>, new()
        {
            return await Task.Run(() => Deserialize(config, askUserOnError, altLoader));
        }

        protected T? DeserializeJson<T>(FilePathHelper disk, JsonSerializerSettings settings)
            where T : class, new()
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
                var config = GetConfig(instance);
                if (config is not null)
                {
                    config.JsonSettings = settings.DeepCopy();
                }
            }
            return instance;
        }

        public T? DeserializeObject<T>(string json, JsonSerializerSettings settings)
            where T : class, new()
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
                SetConfig(instance, settings.DeepCopy());
                //instance.Config.JsonSettings = settings.DeepCopy();
            }
            return instance;
        }

        protected T? DeserializeJson<T>(FilePathHelper disk)
            where T : class, new()
        {
            var settings = GetDefaultSettings();
            return DeserializeJson<T>(disk, settings);
        }

        public JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
        }

        #endregion Deserialization

        #region Config

        protected NewSmartSerializableConfig? GetConfig<T>(T instance)
        {
            return typeof(T).GetProperty("Config")?.GetValue(instance)
                as NewSmartSerializableConfig;
        }

        protected void SetConfig<T>(T instance, JsonSerializerSettings settings)
        {
            var config =
                typeof(T).GetProperty("Config")?.GetValue(instance) as NewSmartSerializableConfig;
            if (config is not null)
            {
                config.JsonSettings = settings;
            }
        }

        #endregion Config

        #region Serialization

        public void Serialize<T>(T instance)
        {
            var filePath = GetConfig(instance)?.Disk?.FilePath;
            if (!filePath.IsNullOrEmpty())
            {
                // filePath is non-null here: IsNullOrEmpty returned false, guaranteeing a value.
                RequestSerialization(instance, filePath!);
            }
        }

        public void Serialize<T>(T instance, string filePath)
        {
            var disk = GetConfig(instance)?.Disk;
            if (disk is not null)
            {
                disk.FilePath = filePath;
                RequestSerialization(instance, filePath);
            }
        }

        protected ReaderWriterLockSlim _readWriteLock = new();

        private Func<string, StreamWriter> _createStreamWriter = File.CreateText;
        protected Func<string, StreamWriter> CreateStreamWriter
        {
            get => _createStreamWriter;
            set => _createStreamWriter = value;
        }

        public void SerializeThreadSafe<T>(T instance, string filePath)
        {
            // Cast to object? so the notnull-constrained ThrowIfNull applies to the unconstrained
            // T; behavior is unchanged (throws ArgumentNullException when the instance is null).
            ((object?)instance).ThrowIfNull(
                $"{nameof(SmartSerializableBase)}.{nameof(instance)} is null. It must be linked to the instance it is serializing."
            );
            // Set Status to Locked
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    using (StreamWriter sw = CreateStreamWriter(filePath))
                    {
                        SerializeToStream(instance, sw);
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

        public string SerializeToString<T>(T instance)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            try
            {
                SerializeToStream(instance, streamWriter);
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

        public void SerializeToStream<T>(T instance, StreamWriter sw)
        {
            sw.ThrowIfNull();
            var config = GetConfig(instance);
            if (config is not null)
            {
                var serializer = JsonSerializer.Create(config.JsonSettings);
                if (config.JsonSettings.TypeNameHandling == TypeNameHandling.Auto)
                {
                    // A caller serializing an instance passes a non-null value; GetType requires it.
                    serializer.Serialize(sw, instance, instance!.GetType());
                }
                else
                {
                    serializer.Serialize(sw, instance);
                }
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

        protected void RequestSerialization<T>(T instance, string filePath)
        {
            if (_serializationRequested.CheckAndSetFirstCall)
            {
                _timer = TimerFactory(TimeSpan.FromSeconds(3));
                _timer.Elapsed += (sender, e) => SerializeThreadSafe(instance, filePath);
                _timer.AutoReset = false;
                _timer.StartTimer();
            }
        }

        #endregion Serialization
    }
}
