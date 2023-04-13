using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Windows.Forms;
using Newtonsoft;
using Newtonsoft.Json;
using Microsoft.Office.Interop.Outlook;


namespace UtilitiesCS
{
    [Serializable()]
    public class SerializableList<T> : IList<T>, ISerializableList<T>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<T> _innerList;
        private IEnumerable<T> _lazyLoader;
        private string _backupFilepath = "";
        

        public SerializableList()
        {
            _innerList = new List<T>();
        }

        public SerializableList(List<T> listOfT)
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

        public SerializableList(string filename, string folderpath, CSVLoader<T> backupLoader, string backupFilepath, bool askUserOnError)
        {
            Filename = filename;
            Folderpath = folderpath;
            _backupFilepath = backupFilepath;
            Deserialize(_filepath, backupLoader, askUserOnError);
        }
        
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

        #region ICollection<T> Members
        public void Add(T item)
        {
            ensureList();
            _innerList.Add(item);
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
            get { ensureList(); return _innerList.Count; }
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

            string output = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filepath, output);

        }

        public void Deserialize()
        {
            if (Filepath != "") Deserialize(Filepath, true);
        }

        public void Deserialize(bool askUserOnError)
        {
            if (Filepath != "") Deserialize(Filepath, askUserOnError);
        }

        public void Deserialize(string filepath, CSVLoader<T> backupLoader, bool askUserOnError)
        {
            if (_filepath != filepath) this.Filepath = filepath;

            DialogResult response = DialogResult.Ignore;

            try
            {
                _innerList = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filepath));
            }
            catch (FileNotFoundException e)
            {
                log.Error(e.Message);
                if (askUserOnError)
                {
                    response = MessageBox.Show($"{filepath} not found. Load from CSV?",
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
                                           " Load from CSV?",
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
                        _innerList = backupLoader(_backupFilepath);
                    }
                    else
                    {
                        log.Debug($"Attempting to load {Path.GetFileName(filepath)} from CSV");

                        var folder = Path.GetDirectoryName(filepath);
                        var filename = Path.GetFileNameWithoutExtension(filepath) + ".csv";
                        _innerList = backupLoader(Path.Combine(folder, filename));
                    }
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
                        _innerList = new List<T> { };
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
                _innerList = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filepath));
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
                    _innerList = new List<T> { };
                }
                else if (_innerList == null)
                {
                    throw new ArgumentNullException("Must have a list or create one to continue executing");
                }
            }
        }

        public List<T> ToList() { return _innerList; }
        #endregion
    }
}
