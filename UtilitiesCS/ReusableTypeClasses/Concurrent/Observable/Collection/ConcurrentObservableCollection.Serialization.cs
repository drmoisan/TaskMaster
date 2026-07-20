#nullable enable

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection
{
    /// <summary>
    /// Serialization partial for <see cref="ConcurrentObservableCollection{T}"/>. Carries the disk
    /// path accessors, file constructors, the <see cref="AltListLoader"/> backup delegate, the
    /// JSON serialize/deserialize members, and the injectable filesystem/prompt seams. Serialization
    /// uses <see cref="TypeNameHandling.Auto"/> and the inherited collection (bare JSON array)
    /// contract — the type carries no <c>[JsonObject]</c> attribute and no root <c>$type</c> wrapper.
    /// This mirrors the member set the former <c>ScoCollection&lt;T&gt;</c> subclasses/consumers rely on.
    /// </summary>
    public partial class ConcurrentObservableCollection<T>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            // GetCurrentMethod() is non-null for a running static initializer, and its DeclaringType
            // is this generic type; both ! reflect the guaranteed non-null log4net logger identity.
            System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!
        );

        internal static IConcurrentObservableCollectionFileSystem FileSystem { get; set; } =
            new ConcurrentObservableCollectionFileSystem();
        internal static IConcurrentObservableCollectionPrompt Prompt { get; set; } =
            new ConcurrentObservableCollectionPrompt();

        #region File Constructors

        public ConcurrentObservableCollection(byte[] file)
            : base()
        {
            DeserializeJson(file);
        }

        public ConcurrentObservableCollection(string fileName, string folderPath)
            : base()
        {
            FileName = fileName;
            FolderPath = folderPath;
            Deserialize();
        }

        public ConcurrentObservableCollection(
            string fileName,
            string folderPath,
            bool askUserOnError
        )
            : base()
        {
            FileName = fileName;
            FolderPath = folderPath;
            Deserialize(askUserOnError);
        }

        public ConcurrentObservableCollection(
            string fileName,
            string folderPath,
            AltListLoader backupLoader,
            string backupFilepath,
            bool askUserOnError
        )
            : base()
        {
            FileName = fileName;
            FolderPath = folderPath;
            _backupFilepath = backupFilepath;
            Deserialize(_disk, backupLoader, backupFilepath, askUserOnError);
        }

        /// <summary>Delegate used to load a backup list when primary deserialization fails.</summary>
        public delegate System.Collections.Generic.IList<T> AltListLoader(string filePath);

        private string? _backupFilepath;

        #endregion File Constructors

        #region Deserialize helpers

        private ConcurrentObservableCollection<T>? DeserializeJson(FilePathHelper disk)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            return JsonConvert.DeserializeObject<ConcurrentObservableCollection<T>>(
                FileSystem.ReadAllText(disk.FilePath),
                settings
            );
        }

        private ConcurrentObservableCollection<T>? DeserializeJson(byte[] file)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            var fileString = Encoding.UTF8.GetString(file);
            return JsonConvert.DeserializeObject<ConcurrentObservableCollection<T>>(
                fileString,
                settings
            );
        }

        private ConcurrentObservableCollection<T> LoadFromBackup(
            AltListLoader backupLoader,
            string backupFilepath,
            FilePathHelper disk
        )
        {
            var collection = new ConcurrentObservableCollection<T>(backupLoader(backupFilepath));
            collection.Serialize(disk.FilePath);
            return collection;
        }

        private ConcurrentObservableCollection<T> CreateEmpty(
            DialogResult response,
            FilePathHelper disk
        )
        {
            if (response == DialogResult.Yes)
            {
                var collection = new ConcurrentObservableCollection<T> { };
                collection.Serialize(disk.FilePath);
                return collection;
            }
            else
            {
                throw new ArgumentNullException(
                    "Must have a list or create one to continue executing"
                );
            }
        }

        private DialogResult AskUser(bool askUserOnError, string messageText)
        {
            return askUserOnError ? Prompt.ShowError(messageText) : DialogResult.Yes;
        }

        #endregion Deserialize helpers

        #region Disk path accessors

        private FilePathHelper _disk = new FilePathHelper();

        public string FilePath
        {
            get => _disk.FilePath;
            set => _disk.FilePath = value;
        }

        public string FolderPath
        {
            get => _disk.FolderPath;
            set => _disk.FolderPath = value;
        }

        public string FileName
        {
            get => _disk.FileName;
            set => _disk.FileName = value;
        }

        #endregion Disk path accessors

        #region Serialize

        public void Serialize()
        {
            if (FilePath != "")
                Serialize(FilePath);
        }

        public void Serialize(string filePath)
        {
            this.FilePath = filePath;
            RequestSerialization(filePath);
        }

        public async Task SerializeAsync()
        {
            if (FilePath != "")
            {
                await SerializeAsync(FilePath);
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public async Task SerializeAsync(string filePath)
        {
            this.FilePath = filePath;
            RequestSerialization(filePath);
            await Task.CompletedTask;
        }

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public void SerializeThreadSafe(string filePath)
        {
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    using (StreamWriter sw = FileSystem.CreateText(filePath))
                    {
                        var settings = new JsonSerializerSettings();
                        settings.TypeNameHandling = TypeNameHandling.Auto;
                        settings.Formatting = Formatting.Indented;

                        var serializer = JsonSerializer.Create(settings);
                        serializer.Serialize(sw, this);
                        sw.Close();
                        _serializationRequested =
                            new UtilitiesCS.Threading.ThreadSafeSingleShotGuard();
                    }
                }
                catch (System.Exception e)
                {
                    logger.Error($"Error serializing to {filePath}", e);
                }
                finally
                {
                    _readWriteLock.ExitWriteLock();
                }
            }
        }

        private UtilitiesCS.Threading.ThreadSafeSingleShotGuard _serializationRequested = new();
        private TimerWrapper? _timer;

        private void RequestSerialization(string filePath)
        {
            if (_serializationRequested.CheckAndSetFirstCall)
            {
                _timer = new TimerWrapper(TimeSpan.FromSeconds(3));
                _timer.Elapsed += (sender, e) => SerializeThreadSafe(filePath);
                _timer.AutoReset = false;
                _timer.StartTimer();
            }
        }

        #endregion Serialize

        #region Deserialize

        public void Deserialize()
        {
            if (FilePath != "")
                Deserialize(_disk, true);
        }

        public void Deserialize(bool askUserOnError)
        {
            if (FilePath != "")
                Deserialize(_disk, askUserOnError);
        }

        public void Deserialize(string fileName, string folderPath, bool askUserOnError)
        {
            _disk = new FilePathHelper(fileName, folderPath);
            Deserialize(_disk, askUserOnError);
        }

        internal void Deserialize(FilePathHelper disk, bool askUserOnError)
        {
            ConcurrentObservableCollection<T>? collection = null;
            bool writeCollection = false;
            DialogResult response = DialogResult.Ignore;

            try
            {
                collection = DeserializeJson(disk);
                if (collection is null)
                {
                    throw new InvalidOperationException($"{_disk.FilePath} deserialized to null.");
                }
            }
            catch (FileNotFoundException e)
            {
                logger.Error(e.Message);
                response = AskUser(
                    askUserOnError,
                    $"{disk.FilePath} not found. Need a list to "
                        + $"continue. Create a new list or abort execution?"
                );
                collection = CreateEmpty(response, disk);
                writeCollection = true;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error! {e.Message}");
                response = AskUser(
                    askUserOnError,
                    $"{disk.FilePath} encountered a problem. \n{e.Message}\n"
                        + $"Need a list to continue. Create a new list or abort execution?"
                );
                collection = CreateEmpty(response, disk);
                writeCollection = true;
            }

            FromList(collection?.ToList());
            if (writeCollection)
            {
                Serialize();
            }
        }

        public void Deserialize(
            string fileName,
            string folderPath,
            AltListLoader backupLoader,
            string backupFilepath,
            bool askUserOnError
        )
        {
            _disk = new FilePathHelper(fileName, folderPath);
            Deserialize(_disk, backupLoader, backupFilepath, askUserOnError);
        }

        internal void Deserialize(
            FilePathHelper disk,
            AltListLoader backupLoader,
            string backupFilepath,
            bool askUserOnError
        )
        {
            ConcurrentObservableCollection<T>? collection = null;
            bool writeCollection = false;
            DialogResult response = DialogResult.Ignore;

            try
            {
                collection = DeserializeJson(disk);
            }
            catch (FileNotFoundException e)
            {
                logger.Error(e.Message);
                response = AskUser(askUserOnError, $"{disk.FilePath} not found. Load from backup?");
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message);
                response = AskUser(
                    askUserOnError,
                    $"{disk.FilePath} encountered a problem. Load from backup?"
                );
            }
            finally
            {
                if (response == DialogResult.Yes && !backupFilepath.IsNullOrEmpty())
                {
                    try
                    {
                        if (FileSystem.Exists(backupFilepath))
                        {
                            collection = LoadFromBackup(backupLoader, backupFilepath, disk);
                            writeCollection = true;
                        }
                        else
                        {
                            logger.Error($"Backup file not found: {backupFilepath}");
                            var response2 = AskUser(
                                askUserOnError,
                                $"Backup file not found: {backupFilepath}\nNeed a list to continue. Create a new list or abort execution?"
                            );
                            collection = CreateEmpty(response2, disk);
                            writeCollection = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        logger.Error($"Error loading backup file {backupFilepath}.", ex);
                        var response2 = AskUser(
                            askUserOnError,
                            $"Backup file {backupFilepath} encountered a problem.\nNeed a list to continue. Create a new list or abort execution?"
                        );
                        collection = CreateEmpty(response2, disk);
                        writeCollection = true;
                    }
                }
                else if (response != DialogResult.Ignore)
                {
                    var response2 = AskUser(
                        askUserOnError,
                        $"Need a list to continue. Create a new list or abort execution?"
                    );
                    collection = CreateEmpty(response2, disk);
                    writeCollection = true;
                }
            }

            FromList(collection?.ToList());
            if (writeCollection)
            {
                Serialize();
            }
        }

        #endregion Deserialize
    }
}
