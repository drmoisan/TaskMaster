using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    internal interface ISerializableListFileSystem
    {
        string ReadAllText(string filePath);
        StreamWriter CreateText(string filePath);
    }

    internal interface ISerializableListPrompt
    {
        DialogResult Show(
            string messageText,
            string caption,
            MessageBoxButtons buttons,
            MessageBoxIcon icon
        );
    }

    internal sealed class SerializableListFileSystem : ISerializableListFileSystem
    {
        public string ReadAllText(string filePath) => File.ReadAllText(filePath);

        public StreamWriter CreateText(string filePath) => File.CreateText(filePath);
    }

    internal sealed class SerializableListPrompt : ISerializableListPrompt
    {
        public DialogResult Show(
            string messageText,
            string caption,
            MessageBoxButtons buttons,
            MessageBoxIcon icon
        ) => MessageBox.Show(messageText, caption, buttons, icon);
    }

    [Serializable()]
    public class SerializableList<T> : IList<T>, ISerializableList<T>
        where T : IComparable<T>
    {
        public SerializableList()
        {
            _innerList = new List<T>();
        }

        public SerializableList(IList<T> listOfT)
        {
            _innerList = listOfT;
        }

        public SerializableList(IEnumerable<T> IEnumerableOfT)
        {
            _lazyLoader = IEnumerableOfT;
        }

        public SerializableList(string filename, string folderpath)
        {
            Filename = filename;
            Folderpath = folderpath;
            Deserialize();
        }

        public SerializableList(
            string filename,
            string folderpath,
            CSVLoader<T> backupLoader,
            string backupFilepath,
            bool askUserOnError
        )
        {
            Filename = filename;
            Folderpath = folderpath;
            _backupFilepath = backupFilepath;
            Deserialize(_filepath, backupLoader, askUserOnError);
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );
        internal static ISerializableListFileSystem FileSystem { get; set; } =
            new SerializableListFileSystem();
        internal static ISerializableListPrompt Prompt { get; set; } = new SerializableListPrompt();
        private IList<T> _innerList;
        private IEnumerable<T> _lazyLoader;
        private string _backupFilepath = "";

        internal void ensureList()
        {
            if (_innerList == null)
                _innerList = new List<T>(_lazyLoader);
        }

        #region IList<T> Members
        public int IndexOf(T item)
        {
            ensureList();
            return _innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ensureList();
            _innerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ensureList();
            _innerList.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                ensureList();
                return _innerList[index];
            }
            set
            {
                ensureList();
                _innerList[index] = value;
            }
        }
        #endregion

        #region IList<T> Extensions
        public int FindIndex(Predicate<T> match) => _innerList.FindIndex(match);

        public int FindIndex(int startIndex, Predicate<T> match) =>
            _innerList.FindIndex(startIndex, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match) =>
            _innerList.FindIndex(startIndex, count, match);
        #endregion

        #region ICollection<T> Members
        public void Add(T item)
        {
            ensureList();
            _innerList.Add(item);
            NotifyPropertyChanged(nameof(Add));
        }

        public void Clear()
        {
            ensureList();
            _innerList.Clear();
        }

        public bool Contains(T item)
        {
            ensureList();
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ensureList();
            _innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                ensureList();
                return _innerList.Count;
            }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            ensureList();
            return _innerList.Remove(item);
        }
        #endregion

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            ensureList();
            return _innerList.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            ensureList();
            return _innerList.GetEnumerator();
        }
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
            get { return _filepath; }
            set
            {
                _filepath = value;
                var fileExtension = Path.GetExtension(value);
                _folderpath = Path.GetDirectoryName(_filepath);
                _filename = Path.GetFileName(_filepath);
                if ((value != "") && (fileExtension == "") && Directory.Exists(value))
                {
                    throw new ArgumentException(
                        $"{value} is a Folder Path and was passed to the field named 'Filepath'. "
                            + "Either pass this to the 'FileName' field or include a folderpath."
                    );
                }
            }
        }

        public string Folderpath
        {
            get { return _folderpath; }
            set
            {
                _folderpath = value;
                if (_filename != "")
                    _filepath = Path.Combine(_folderpath, _filename);
            }
        }

        public string Filename
        {
            get { return _filename; }
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
            var fileSystem = FileSystem;
            QueueSerialize(filepath, fileSystem);
        }

        public async Task SerializeAsync()
        {
            if (Filepath != "")
            {
                await SerializeAsync(Filepath);
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public async Task SerializeAsync(string filepath)
        {
            this.Filepath = filepath;
            var fileSystem = FileSystem;
            await Task.Run(() => SerializeCore(filepath, fileSystem));
        }

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        private void QueueSerialize(string filepath, ISerializableListFileSystem fileSystem)
        {
            _ = Task.Run(() => SerializeCore(filepath, fileSystem));
        }

        /// <summary>
        /// Serialize the list to <paramref name="filepath"/> on the calling thread, holding the
        /// write lock for the duration. Callers that schedule this on a background thread must
        /// pass the file-system dependency they captured at schedule time so the correct
        /// implementation is used even if the seam is swapped after the task is queued.
        /// </summary>
        private void SerializeCore(string filepath, ISerializableListFileSystem fileSystem)
        {
            if (_readWriteLock.TryEnterWriteLock(-1))
            {
                try
                {
                    using (StreamWriter sw = fileSystem.CreateText(filepath))
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
                    _readWriteLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Serialize the list to <paramref name="filepath"/> using the current
        /// file-system seam, holding the write lock for the duration.
        /// Intended for direct synchronous calls; for fire-and-forget or async use,
        /// prefer <see cref="Serialize(string)"/> or <see cref="SerializeAsync(string)"/>,
        /// which capture the dependency at call time.
        /// </summary>
        public void SerializeThreadSafe(string filepath)
        {
            var fileSystem = FileSystem;
            SerializeCore(filepath, fileSystem);
        }

        public void Sort()
        {
            _innerList = _innerList.OrderBy(x => x).ToList();
        }

        //public void Serialize(string filepath)
        //{
        //    this.Filepath = filepath;

        //    var settings = new JsonSerializerSettings();
        //    settings.TypeNameHandling = TypeNameHandling.Auto;
        //    settings.Formatting = Formatting.Indented;
        //    using (TextWriter writer = File.CreateText(filepath))
        //    {
        //        var serializer = JsonSerializer.Create(settings);
        //        serializer.Serialize(writer, this);
        //    }
        //    //string output = JsonConvert.SerializeObject(this, settings);
        //    //string output = JsonConvert.SerializeObject(this, Formatting.Indented);
        //    //File.WriteAllText(filepath, output);

        //}

        public void Deserialize()
        {
            if (Filepath != "")
                Deserialize(Filepath, true);
        }

        public void Deserialize(bool askUserOnError)
        {
            if (Filepath != "")
                Deserialize(Filepath, askUserOnError);
        }

        public void Deserialize(string filepath, CSVLoader<T> backupLoader, bool askUserOnError)
        {
            if (_filepath != filepath)
                this.Filepath = filepath;

            var fileSystem = FileSystem;
            var prompt = Prompt;
            DialogResult response = DialogResult.Ignore;

            try
            {
                _innerList = JsonConvert.DeserializeObject<List<T>>(
                    fileSystem.ReadAllText(filepath)
                );
            }
            catch (FileNotFoundException e)
            {
                log.Error(e.Message);
                if (askUserOnError)
                {
                    response = prompt.Show(
                        $"{filepath} not found. Load from backup?",
                        "File Not Found",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error
                    );
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
                    response = prompt.Show(
                        $"{filepath} encountered a problem. {e.Message} " + " Load from backup?",
                        "Error!",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error
                    );
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
                        _innerList = backupLoader(_backupFilepath);
                    }
                    else
                    {
                        log.Debug($"Attempting to load {Path.GetFileName(filepath)} from backup");

                        var folder = Path.GetDirectoryName(filepath);
                        var filename = Path.GetFileNameWithoutExtension(filepath) + ".csv";
                        _innerList = backupLoader(Path.Combine(folder, filename));
                    }
                    NotifyPropertyChanged("BackupLoader");
                    QueueSerialize(Filepath, fileSystem);
                }
                else if (response == DialogResult.No)
                {
                    if (askUserOnError)
                    {
                        response = prompt.Show(
                            "Need a list to continue. " + "Create a new List Or Stop Execution?",
                            "Error",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Error
                        );
                    }
                    else
                    {
                        response = DialogResult.Yes;
                    }

                    if (response == DialogResult.Yes)
                    {
                        _innerList = new List<T> { };
                    }
                    else
                    {
                        throw new ArgumentNullException(
                            "Must have a list or create one to continue executing"
                        );
                    }
                }
            }
        }

        public void Deserialize(string filepath, bool askUserOnError)
        {
            if (_filepath != filepath)
                this.Filepath = filepath;

            var fileSystem = FileSystem;
            var prompt = Prompt;
            DialogResult response = DialogResult.Ignore;

            try
            {
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                settings.Formatting = Formatting.Indented;
                _innerList = JsonConvert.DeserializeObject<List<T>>(
                    fileSystem.ReadAllText(filepath),
                    settings
                );
                if (_innerList is null)
                {
                    throw new FileFormatException("File could not be deserialized correctly");
                }
                //_innerList = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filepath));
            }
            catch (FileNotFoundException)
            {
                log.Error($"File {filepath} does not exist.");
                if (askUserOnError)
                {
                    response = prompt.Show(
                        $"{filepath} not found. Create a new list? Excecution will stop if answer is no.",
                        "File Not Found",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error
                    );
                }
                else
                {
                    response = DialogResult.Yes;
                }
            }
            catch (System.Exception e)
            {
                log.Error($"Error! {e.Message}");
                if (askUserOnError)
                {
                    response = prompt.Show(
                        filepath
                            + " encountered a problem. "
                            + e.Message
                            + " Create a new list? Excecution will stop if answer is no.",
                        "Error",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error
                    );
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
                    _innerList = new List<T> { };
                    QueueSerialize(Filepath, fileSystem);
                }
                else if (_innerList == null)
                {
                    throw new ArgumentNullException(
                        "Must have a list or create one to continue executing"
                    );
                }
            }
        }

        public List<T> ToList()
        {
            return new List<T>(_innerList);
        }

        public void FromList(IList<T> value)
        {
            _innerList = value;
        }

        #endregion
    }
}
