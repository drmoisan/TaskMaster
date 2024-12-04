using Newtonsoft.Json;
using Swordfish.NET.Collections;
using System;
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
    public class ScoSortedDictionary<TKey, TValue>: ConcurrentObservableSortedDictionary<TKey, TValue>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScoSortedDictionary() :base(null) { }
        public ScoSortedDictionary(IComparer<TKey> comparer) : base(comparer) { }
        public ScoSortedDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer) : base(comparer)
        {
            foreach (var kvp in dictionary)
            {
                Add(kvp.Key, kvp.Value);
            }
        }
        public ScoSortedDictionary(IDictionary<TKey, TValue> dictionary): this(dictionary, null) { }
        
        public ScoSortedDictionary(string fileName, string folderPath) : base(null)
        {
            FileName = fileName;
            FolderPath = folderPath;
            Deserialize(fileName, folderPath, true);
        }

        #region Serialization

        protected FilePathHelper _disk = new FilePathHelper();

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

        protected ScoSortedDictionary<TKey,TValue> CreateEmpty(DialogResult response, FilePathHelper disk)
        {
            if (response == DialogResult.Yes)
            {
                var dictionary = new ScoSortedDictionary<TKey, TValue> { };
                dictionary.Serialize(disk.FilePath);
                return dictionary;
            }
            else
            {
                throw new ArgumentNullException(
                "Must have a dictionary or create one to continue executing");
            }
        }

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

        protected ThreadSafeSingleShotGuard _serializationRequested = new();
        protected TimerWrapper _timer;
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
            ScoSortedDictionary<TKey, TValue> dictionary = null;
            bool writeDictionary = false;
            DialogResult response = DialogResult.Ignore;

            try
            {
                dictionary = DeserializeJson(disk);
                if (dictionary is null)
                {
                    throw new InvalidOperationException($"{_disk.FilePath} deserialized to null.");
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

            DoBaseWrite(() => WriteCollection = dictionary?.DoBaseRead(() => dictionary?.ReadCollection));
            if (writeDictionary)
            {
                Serialize();
            }
        }

        private ScoSortedDictionary<TKey, TValue> DeserializeJson(FilePathHelper disk)
        {
            ScoSortedDictionary<TKey, TValue> collection;
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            collection = JsonConvert.DeserializeObject<ScoSortedDictionary<TKey,TValue>>(
                File.ReadAllText(disk.FilePath), settings);
            return collection;
        }

        #endregion Serialization




    }
}
