using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilitiesCS.ReusableTypeClasses;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class CorpusInherit: ConcurrentDictionary<string, int>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public CorpusInherit() : base() { }
        public CorpusInherit(IEnumerable<KeyValuePair<string, int>> collection) : base(collection) { }
        public CorpusInherit(IEqualityComparer<string> comparer) : base(comparer) { }
        public CorpusInherit(IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) : base(collection, comparer) { }
        public CorpusInherit(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity) { }
        public CorpusInherit(int concurrencyLevel, IEnumerable<KeyValuePair<string, int>> collection, IEqualityComparer<string> comparer) : base(concurrencyLevel, collection, comparer) { }
        public CorpusInherit(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer) : base(concurrencyLevel, capacity, comparer) { }

        #endregion Constructors

        #region Public Properties and Methods

        private string _id;
        public string Id { get => _id; set => _id = value; }

        public Enums.Corpus Indicator { get => _indicator; set => _indicator = value; }
        private Enums.Corpus _indicator;

        public void AddOrIncrementToken(string token) => this.AddOrUpdate(token, 1, (key, count) => count++);

        public void AddOrIncrementTokens(IEnumerable<string> tokens) => tokens.ForEach(AddOrIncrementToken);

        public void DecrementOrRemoveToken(string token)
        {
            lock (this)
            {
                if (this.TryGetValue(token, out int count))
                {
                    if (--count == 0)
                    {
                        this.TryRemove(token, out _);
                    }
                    else
                    {
                        this[token] = count;
                    }
                }
            }
        }

        #endregion Public Properties and Methods

        #region Static Deserialization

        protected static CorpusInherit CreateEmpty(DialogResult response, FilePathHelper disk)
        {
            if (response == DialogResult.Yes)
            {
                var dictionary = new CorpusInherit();
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
        
        public static CorpusInherit Deserialize(string fileName, string folderPath) 
        {
            return Deserialize(fileName, folderPath, false);
        }

        public static CorpusInherit Deserialize(string fileName, string folderPath, bool askUserOnError)
        {
            var disk = new FilePathHelper(fileName, folderPath);
            Deserialize(disk, askUserOnError);
            return new CorpusInherit();
        }

        internal static CorpusInherit Deserialize(FilePathHelper disk, bool askUserOnError)
        {
            CorpusInherit dictionary = null;
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

        protected static CorpusInherit DeserializeJson(FilePathHelper disk)
        {
            CorpusInherit collection;
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            collection = JsonConvert.DeserializeObject<CorpusInherit>(
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
