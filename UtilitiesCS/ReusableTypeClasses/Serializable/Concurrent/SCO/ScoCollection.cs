using Newtonsoft.Json;
using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS
{
    public class ScoCollection<T> : ConcurrentObservableCollection<T>, IList<T>, IList
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public ScoCollection() : base() { }

        public ScoCollection(IEnumerable<T> enumerable) : base(enumerable) { }

        public ScoCollection(byte[] file) : base() 
        {
            DeserializeJson(file);
        }
        
        public ScoCollection(
            string fileName,
            string folderPath) : base()
        {
            FileName = fileName;
            FolderPath = folderPath;
            Deserialize();
        }

        public ScoCollection(
            string fileName,
            string folderPath,
            bool askUserOnError) : base()
        {
            FileName = fileName;
            FolderPath = folderPath;
            Deserialize(askUserOnError);
        }

        public ScoCollection(
            string fileName,
            string folderPath,
            AltListLoader backupLoader,
            string backupFilepath,
            bool askUserOnError) : base()
        {
            FileName = fileName;
            FolderPath = folderPath;
            _backupFilepath = backupFilepath;
            Deserialize(_disk, backupLoader, backupFilepath, askUserOnError);
        }

        public delegate IList<T> AltListLoader(string filePath);
        private string _backupFilepath;

        #endregion Constructors

        #region Helper Methods

        private ScoCollection<T> DeserializeJson(FilePathHelper disk)
        {
            ScoCollection<T> collection;
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            collection = JsonConvert.DeserializeObject<ScoCollection<T>>(
                File.ReadAllText(disk.FilePath), settings);
            return collection;
        }

        private ScoCollection<T> DeserializeJson(byte[] file)
        {
            ScoCollection<T> collection;
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            var fileString = Encoding.UTF8.GetString(file);
            collection = JsonConvert.DeserializeObject<ScoCollection<T>>(fileString);
            return collection;
        }

        private ScoCollection<T> LoadFromBackup(AltListLoader backupLoader, string backupFilepath, FilePathHelper disk)
        {
            ScoCollection<T> collection;
            //logger.Debug($"Attempting to load {Path.GetFileName(backupFilepath)} from backup");
            collection = new ScoCollection<T>(backupLoader(backupFilepath));

            //NotifyPropertyChanged("BackupLoader");
            collection.Serialize(disk.FilePath);
            return collection;
        }

        private ScoCollection<T> CreateEmpty(DialogResult response, FilePathHelper disk)
        {
            if (response == DialogResult.Yes)
            {
                var collection = new ScoCollection<T> { };
                collection.Serialize(disk.FilePath);
                return collection;
            }
            else { throw new ArgumentNullException(
                "Must have a list or create one to continue executing"); }
        }

        private DialogResult AskUser(bool askUserOnError, string messageText)
        {
            DialogResult response;
            if (askUserOnError)
            {
                response = MyBox.ShowDialog(
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

        public List<T> ToList() { return new List<T>(this); }

        public void FromList(IList<T> value) 
        { 
            var collection = new ScoCollection<T>(value);
            this.Clear();
            DoBaseWrite(() => WriteCollection = collection?.DoBaseRead(() => ReadCollection));
        }

        #endregion

        #region Serialization

        private FilePathHelper _disk = new FilePathHelper();
        
        public string FilePath { get => _disk.FilePath; set => _disk.FilePath = value; }
        
        public string FolderPath { get => _disk.FolderPath; set => _disk.FolderPath = value;}

        public string FileName { get => _disk.FileName; set => _disk.FileName = value;}
        
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

        async public Task SerializeAsync()
        {
            if (FilePath != "")
            {
                await SerializeAsync(FilePath);
            }
            else { await Task.CompletedTask; }

        }

        public async Task SerializeAsync(string filePath)
        {
            this.FilePath = filePath;
            //await Task.Run(() => SerializeThreadSafe(filePath));
            RequestSerialization(filePath);
            // Hack for now since i just made the async function sync. Need to remove
            await Task.CompletedTask;
        }

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

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

        public void Deserialize()
        {
            if (FilePath != "") Deserialize(_disk, true);
        }

        public void Deserialize(bool askUserOnError)
        {
            if (FilePath != "") Deserialize(_disk, askUserOnError);
        }

        public void Deserialize(string fileName, string folderPath, bool askUserOnError)
        {
            _disk = new FilePathHelper(fileName, folderPath);
            Deserialize(_disk, askUserOnError);
        }

        internal void Deserialize(FilePathHelper disk, bool askUserOnError)
        {
            ScoCollection<T> collection = null;
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
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} not found. Need a list to " +
                    $"continue. Create a new list or abort execution?");
                collection = CreateEmpty(response, disk);
                writeCollection = true;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error! {e.Message}");
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} encountered a problem. \n{e.Message}\n" +
                    $"Need a list to continue. Create a new list or abort execution?");
                collection = CreateEmpty(response, disk);
                writeCollection = true;
            }

            DoBaseWrite(() => WriteCollection = collection?.DoBaseRead(() => collection?.ReadCollection));
            if (writeCollection)
            {
                Serialize();
            }
        }

        public void Deserialize(string fileName, string folderPath, AltListLoader backupLoader, string backupFilepath, bool askUserOnError)
        {
            _disk = new FilePathHelper(fileName, folderPath);
            Deserialize(_disk, backupLoader, backupFilepath, askUserOnError);
        }

        internal void Deserialize(FilePathHelper disk, AltListLoader backupLoader, string backupFilepath, bool askUserOnError)
        {
            ScoCollection<T> collection = null;
            bool writeCollection = false;
            DialogResult response = DialogResult.Ignore;

            try
            {
                collection = DeserializeJson(disk);
            }
            catch (FileNotFoundException e)
            {
                logger.Error(e.Message);
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} not found. Load from backup?");
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message);
                response = AskUser(askUserOnError,
                    $"{disk.FilePath} encountered a problem. Load from backup?");
            }
            finally
            {
                if (response == DialogResult.Yes && !backupFilepath.IsNullOrEmpty())
                {
                    collection = LoadFromBackup(backupLoader, backupFilepath, disk);
                    writeCollection = true;
                }
                else if (response != DialogResult.Ignore)
                {
                    var response2 = AskUser(askUserOnError,
                        $"Need a list to continue. Create a new list or abort execution?");
                    collection = CreateEmpty(response2, disk);
                    writeCollection = true;
                }
            }
            
            DoBaseWrite(() => WriteCollection = collection?.DoBaseRead(() => collection?.ReadCollection));
            if (writeCollection)
            {
                Serialize();
            }
        }

        #endregion Serialization

        #region Dead Code


        //public static ScoCollection<T> Deserialize(string fileName, string folderPath, bool askUserOnError)
        //{
        //    var disk = new FilePathHelper(fileName, folderPath);
        //    ScoCollection<T> collection = null;

        //    DialogResult response = DialogResult.Ignore;

        //    try
        //    {
        //        collection = DeserializeJson(disk);
        //        return collection;
        //    }
        //    catch (FileNotFoundException e)
        //    {
        //        logger.Error(e.Message);
        //        response = AskUser(askUserOnError,
        //            $"{disk.FilePath} not found. Need a list to " +
        //            $"continue. Create a new list or abort execution?");
        //        collection = CreateEmpty(response, disk);                
        //    }
        //    catch (System.Exception e)
        //    {
        //        logger.Error($"Error! {e.Message}");
        //        response = AskUser(askUserOnError,
        //            $"{disk.FilePath} encountered a problem. \n{e.Message}\n" +
        //            $"Need a list to continue. Create a new list or abort execution?");
        //        collection = CreateEmpty(response, disk);
        //    }

        //    return collection;
        //}

        //public static ScoCollection<T> Deserialize(string fileName, string folderPath, AltListLoader<T> backupLoader, string backupFilepath, bool askUserOnError)
        //{
        //    var disk = new FilePathHelper(fileName, folderPath);
        //    ScoCollection<T> collection = null;
        //    DialogResult response = DialogResult.Ignore;

        //    try
        //    {
        //        collection = DeserializeJson(disk);
        //    }
        //    catch (FileNotFoundException e)
        //    {
        //        logger.Error(e.Message);
        //        response = AskUser(askUserOnError,  
        //            $"{disk.FilePath} not found. Load from backup?");
        //    }
        //    catch (System.Exception e)
        //    {
        //        logger.Error(e.Message);
        //        response = AskUser(askUserOnError, 
        //            $"{disk.FilePath} encountered a problem. Load from backup?");
        //    }
        //    finally
        //    {
        //        if (response == DialogResult.Yes && !backupFilepath.IsNullOrEmpty())
        //        {
        //            collection = LoadFromBackup(backupLoader, backupFilepath, disk);
        //        }
        //        else
        //        {
        //            var response2 = AskUser(askUserOnError,
        //                $"Need a list to continue. Create a new list or abort execution?");
        //            collection = CreateEmpty(response2, disk);
        //        }
        //    }

        //    return collection;

        //}

        #endregion
    }
}
