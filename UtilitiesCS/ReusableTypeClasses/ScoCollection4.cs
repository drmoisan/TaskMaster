using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Deedle;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using Swordfish.NET.Collections;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS
{
    public class ScoCollection4<T> : IScoCollection<T>
    //IConcurrentObservableBase<T>, IList<T>, ICollection<T>, IList, ICollection, IScoCollection<T>
    {
        #region Constructors

        public ScoCollection4() 
        {
            _collection = new();
        }
        
        public ScoCollection4(IEnumerable<T> enumerable) 
        { 
            _collection = new(enumerable);
        }

        public ScoCollection4(string filename, string folderpath)
        {
            FileName = filename;
            Folderpath = folderpath;
            Deserialize();
        }

        public ScoCollection4(string filename, string folderpath, bool askUserOnError)
        {
            FileName = filename;
            Folderpath = folderpath;
            Deserialize(askUserOnError);
        }

        public ScoCollection4(string filename, string folderpath, AltListLoader<T> backupLoader, string backupFilepath, bool askUserOnError)
        {
            FileName = filename;
            Folderpath = folderpath;
            _backupFilepath = backupFilepath;
            Deserialize(_filePath, backupLoader, askUserOnError);
        }

        #endregion

        #region Private Variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ConcurrentObservableCollection<T> _collection;
        private string _backupFilepath = "";

        #endregion

        #region IConcurrentObservableBase Implementation

        public T this[int index] { get => _collection[index]; set => _collection[index] = value; }
        
        public ImmutableCollectionBase<T> Snapshot => _collection.Snapshot;

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        public event NotifyCollectionChangedEventHandler CollectionChanged { add { } remove { } }

        public void Add(T item) => _collection.Add(item);
        
        public void Clear() => _collection.Clear();
        
        public bool Contains(T item) => _collection.Contains(item);
        
        public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);
        
        public void Dispose() => _collection.Dispose();
        
        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
        
        public int IndexOf(T item) => _collection.IndexOf(item);
        
        public void Insert(int index, T item) => _collection.Insert(index, item);
        
        public bool Remove(T item) => _collection.Remove(item);
        
        public void RemoveAt(int index) => _collection.RemoveAt(index);
        
        public IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs> observer) => _collection.Subscribe(observer);
                
        IEnumerator IEnumerable.GetEnumerator() => (_collection as IEnumerable).GetEnumerator();

        #endregion

        #region IList Implementation

        object IList.this[int index] { get => (_collection as IList)[index]; set => (_collection as IList)[index] = value; }

        bool IList.IsReadOnly => (_collection as IList).IsReadOnly;

        bool IList.IsFixedSize => (_collection as IList).IsFixedSize;

        int IList.Add(object value) => (_collection as IList).Add(value);
        
        void IList.Clear() => (_collection as IList).Clear();
        
        bool IList.Contains(object value) => (_collection as IList).Contains(value);
        
        int IList.IndexOf(object value) => (_collection as IList).IndexOf(value);
        
        void IList.Insert(int index, object value) => (_collection as IList).Insert(index, value);
        
        void IList.Remove(object value) => (_collection as IList).Remove(value);
        
        void IList.RemoveAt(int index) => (_collection as IList).RemoveAt(index);

        #endregion

        #region ICollection Implementation

        int ICollection.Count => (_collection as ICollection).Count;

        object ICollection.SyncRoot => (_collection as ICollection).SyncRoot;

        bool ICollection.IsSynchronized => (_collection as ICollection).IsSynchronized;

        void ICollection.CopyTo(Array array, int index) => (_collection as ICollection).CopyTo(array, index);

        #endregion

        #region List<T> Implementation by Dan Moisan

        public bool Exists(Predicate<T> match)
        {
            return this.FindIndex(match) != -1;
        }

        public T Find(Predicate<T> match)
        {
            var index = this.FindIndex(0, this.Count, match);
            if (index == -1)
            {
                return default(T);
            }
            else
            {
                return this[index];
            }
        }

        public int[] FindIndices(Predicate<T> match)
        {
            return this.FindIndices(0, this.Count, match);
        }

        public int[] FindIndices(int startIndex, Predicate<T> match)
        {
            return this.FindIndices(startIndex, this.Count, match);
        }

        public int[] FindIndices(int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)this.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startIndex), $"{nameof(startIndex)} " +
                    $"has a value of {startIndex} which is greater " +
                    $"than the list length of {this.Count}");
            }

            if (count < 0 || startIndex > this.Count - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var indices = new List<int>();

            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(this[i]))
                {
                    indices.Add(i);
                }
            }

            return indices.ToArray();
        }

        public int FindIndex(Predicate<T> match)
        {
            return this.FindIndex(0, this.Count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return this.FindIndex(startIndex, this.Count - startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)this.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startIndex), $"{nameof(startIndex)} has " +
                    $"a value of {startIndex} which is greater than " +
                    $"the list length of {this.Count}");
            }

            if (count < 0 || startIndex > this.Count - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(this[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion

        #region Serialization

        private string _filePath = "";
        private string _fileName = "";
        private string _folderPath = "";

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                var fileExtension = Path.GetExtension(value);
                _folderPath = Path.GetDirectoryName(_filePath);
                _fileName = Path.GetFileName(_filePath);
                if ((value != "") && (fileExtension == "") && Directory.Exists(value))
                {
                    throw new ArgumentException(
                        $"{value} is a Folder Path and was passed to the field named 'Filepath'. " +
                        "Either pass this to the 'FileName' field or include a folderpath.");
                }
            }
        }

        public string Folderpath
        {
            get
            {
                return _folderPath;
            }
            set
            {
                _folderPath = value;
                if (_fileName != "")
                    _filePath = Path.Combine(_folderPath, _fileName);
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                if (_folderPath != "")
                    _filePath = Path.Combine(_folderPath, _fileName);
            }
        }

        public void Serialize()
        {
            if (FilePath != "")
                Serialize(FilePath);
        }

        public void Serialize(string filePath)
        {
            this.FilePath = filePath;
            //_ = Task.Run(() => SerializeThreadSafe(filePath));
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
                    log.Error($"Error serializing to {filePath}", e);
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
            if (FilePath != "") Deserialize(FilePath, true);
        }

        public void Deserialize(bool askUserOnError)
        {
            if (FilePath != "") Deserialize(FilePath, askUserOnError);
        }

        public void Deserialize(string filePath, AltListLoader<T> backupLoader, bool askUserOnError)
        {
            if (_filePath != filePath) this.FilePath = filePath;

            DialogResult response = DialogResult.Ignore;

            try
            {
                _collection = JsonConvert.DeserializeObject<ConcurrentObservableCollection<T>>(File.ReadAllText(filePath));
            }
            catch (FileNotFoundException e)
            {
                log.Error(e.Message);
                if (askUserOnError)
                {
                    response = MessageBox.Show($"{filePath} not found. Load from backup?",
                                               "File Not Found",
                                               MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Error);
                }
                else
                {
                    response = DialogResult.Yes;
                }
            }
            catch (System.Exception e)
            {
                log.Error(e.Message);
                if (askUserOnError)
                {
                    response = MessageBox.Show($"{filePath} encountered a problem. {e.Message} " +
                                               " Load from backup?",
                                               "Error!",
                                               MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Error);
                }
                else
                {
                    response = DialogResult.Yes;
                }
            }
            finally
            {
                if (response == DialogResult.Yes)
                {
                    if (_backupFilepath != "")
                    {
                        _collection = new ConcurrentObservableCollection<T>(backupLoader(_backupFilepath));
                    }
                    else
                    {
                        log.Debug($"Attempting to load {Path.GetFileName(filePath)} from backup");

                        var folder = Path.GetDirectoryName(filePath);
                        var filename = Path.GetFileNameWithoutExtension(filePath) + ".csv";
                        _collection = new ConcurrentObservableCollection<T>(backupLoader(Path.Combine(folder, filename)));
                    }
                    NotifyPropertyChanged("BackupLoader");
                    Serialize();
                }
                else if (response == DialogResult.No)
                {
                    if (askUserOnError)
                    {
                        response = MessageBox.Show("Need a list to continue. " +
                                                   "Create a new List Or Stop Execution?",
                                                   "Error",
                                                   MessageBoxButtons.YesNo,
                                                   MessageBoxIcon.Error);
                    }
                    else { response = DialogResult.Yes; }

                    if (response == DialogResult.Yes)
                    {
                        _collection = new ConcurrentObservableCollection<T> { };
                    }
                    else { throw new ArgumentNullException("Must have a list or create one to continue executing"); }
                }
            }

        }

        public void Deserialize(string filePath, bool askUserOnError)
        {
            if (_filePath != filePath) this.FilePath = filePath;

            DialogResult response = DialogResult.Ignore;

            try
            {
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                settings.Formatting = Formatting.Indented;
                _collection = JsonConvert.DeserializeObject<ConcurrentObservableCollection<T>>(File.ReadAllText(filePath), settings);

                //_collection = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filepath));
            }
            catch (FileNotFoundException)
            {
                log.Error($"File {filePath} does not exist.");
                if (askUserOnError)
                {
                    response = MessageBox.Show($"{filePath} not found. Create a new list? Excecution will stop if answer is no.",
                                               "File Not Found",
                                               MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Error);
                }
                else { response = DialogResult.Yes; }
            }
            catch (System.Exception e)
            {
                log.Error($"Error! {e.Message}");
                if (askUserOnError)
                {
                    response = MessageBox.Show(filePath + " encountered a problem. " +
                                               e.Message + " Create a new list? Excecution will stop if answer is no.",
                                               "Error",
                                               MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Error);
                }
                else { response = DialogResult.Yes; }
            }
            finally
            {
                if (response == DialogResult.Yes)
                {
                    _collection = new ConcurrentObservableCollection<T> { };
                    this.Serialize();
                }
                else if (_collection == null)
                {
                    throw new ArgumentNullException("Must have a list or create one to continue executing");
                }
            }
        }

        public List<T> ToList() { return new List<T>(_collection); }

        public void FromList(IList<T> value) { _collection = new ConcurrentObservableCollection<T>(value); }

        #endregion

    }


}
