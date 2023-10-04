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
using Swordfish.NET.General.Collections;

namespace UtilitiesCS
{
    public class ScoCollection<T> : IConcurrentObservableBase<T>, IList<T>, ICollection<T>, IList, ICollection, IScoCollection<T>
    {
        #region Constructors

        public ScoCollection() 
        {
            _collection = new();
        }
        
        public ScoCollection(IEnumerable<T> enumerable) 
        { 
            _collection = new(enumerable);
        }

        public ScoCollection(string filename, string folderpath)
        {
            Filename = filename;
            Folderpath = folderpath;
            Deserialize();
        }

        public ScoCollection(string filename, string folderpath, bool askUserOnError)
        {
            Filename = filename;
            Folderpath = folderpath;
            Deserialize(askUserOnError);
        }

        public ScoCollection(string filename, string folderpath, AltListLoader<T> backupLoader, string backupFilepath, bool askUserOnError)
        {
            Filename = filename;
            Folderpath = folderpath;
            _backupFilepath = backupFilepath;
            Deserialize(_filepath, backupLoader, askUserOnError);
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

        // { add { } remove { } } suggested to avoid compiler warning on interface implementation
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

        #region Serialization
        private string _filepath = "";
        private string _filename = "";
        private string _folderpath = "";

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Filepath
        {
            get
            {
                return _filepath;
            }
            set
            {
                _filepath = value;
                var fileExtension = Path.GetExtension(value);
                _folderpath = Path.GetDirectoryName(_filepath);
                _filename = Path.GetFileName(_filepath);
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
                return _folderpath;
            }
            set
            {
                _folderpath = value;
                if (_filename != "")
                    _filepath = Path.Combine(_folderpath, _filename);
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                if (_folderpath != "")
                    _filepath = Path.Combine(_folderpath, _filename);
            }
        }

        public void Serialize()
        {
            if (Filepath != "")
                Serialize(Filepath);
        }

        public void Serialize(string filepath)
        {
            this.Filepath = filepath;
            _ = Task.Run(() => SerializeThreadSafe(filepath));
            //var settings = new JsonSerializerSettings();
            //settings.TypeNameHandling = TypeNameHandling.Auto;
            //settings.Formatting = Formatting.Indented;
            //using (TextWriter writer = File.CreateText(filepath))
            //{
            //    var serializer = JsonSerializer.Create(settings);
            //    serializer.Serialize(writer, this);
            //}
        }

        async public Task SerializeAsync()
        {
            if (Filepath != "")
            {
                await SerializeAsync(Filepath);
            }
            else { await Task.CompletedTask; }

        }

        public async Task SerializeAsync(string filepath)
        {
            this.Filepath = filepath;
            await Task.Run(() => SerializeThreadSafe(filepath));
        }

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public void SerializeThreadSafe(string filepath)
        {

            // Set Status to Locked
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    // Append text to the file
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        var settings = new JsonSerializerSettings();
                        settings.TypeNameHandling = TypeNameHandling.Auto;
                        settings.Formatting = Formatting.Indented;

                        var serializer = JsonSerializer.Create(settings);
                        serializer.Serialize(sw, this);
                        sw.Close();
                    }
                }
                catch (System.Exception e)
                {
                    log.Error($"Error serializing to {filepath}", e);
                }
                finally
                {
                    // Release lock
                    _readWriteLock.ExitWriteLock();
                }
            }

        }

        public void Deserialize()
        {
            if (Filepath != "") Deserialize(Filepath, true);
        }

        public void Deserialize(bool askUserOnError)
        {
            if (Filepath != "") Deserialize(Filepath, askUserOnError);
        }

        public void Deserialize(string filepath, AltListLoader<T> backupLoader, bool askUserOnError)
        {
            if (_filepath != filepath) this.Filepath = filepath;

            DialogResult response = DialogResult.Ignore;

            try
            {
                _collection = JsonConvert.DeserializeObject<ConcurrentObservableCollection<T>>(File.ReadAllText(filepath));
            }
            catch (FileNotFoundException e)
            {
                log.Error(e.Message);
                if (askUserOnError)
                {
                    response = MessageBox.Show($"{filepath} not found. Load from backup?",
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
                    response = MessageBox.Show($"{filepath} encountered a problem. {e.Message} " +
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
                        log.Debug($"Attempting to load {Path.GetFileName(filepath)} from backup");

                        var folder = Path.GetDirectoryName(filepath);
                        var filename = Path.GetFileNameWithoutExtension(filepath) + ".csv";
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

        public void Deserialize(string filepath, bool askUserOnError)
        {
            if (_filepath != filepath) this.Filepath = filepath;

            DialogResult response = DialogResult.Ignore;

            try
            {
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                settings.Formatting = Formatting.Indented;
                _collection = JsonConvert.DeserializeObject<ConcurrentObservableCollection<T>>(File.ReadAllText(filepath), settings);

                //_collection = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filepath));
            }
            catch (FileNotFoundException)
            {
                log.Error($"File {filepath} does not exist.");
                if (askUserOnError)
                {
                    response = MessageBox.Show($"{filepath} not found. Create a new list? Excecution will stop if answer is no.",
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
                    response = MessageBox.Show(filepath + " encountered a problem. " +
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
