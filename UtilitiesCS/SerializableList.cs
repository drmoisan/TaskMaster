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

namespace UtilitiesCS
{
    
    [Serializable()]
    public class SerializableList<T> : IList<T>, ISerializableList<T>
    {
        private List<T> _innerList;
        private IEnumerable<T> _lazyLoader;

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

        private void ensureList()
        {
            if (_innerList == null)
                _innerList = new List<T>(_lazyLoader);
        }

        #region IList<T> Members
        public int IndexOf(T item)
        {
            ensureList();
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
                _folderpath = Path.GetDirectoryName(_filepath);
                _filename = Path.GetFileName(_filepath);
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
            if (Filepath != "") Deserialize(Filepath);
        }

        public void Deserialize(string filepath)
        {
            if (_filepath != filepath) this.Filepath = filepath;

            DialogResult response = DialogResult.Ignore;

            try
            {

                _innerList = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filepath));
            }
            catch (FileNotFoundException)
            {
                response = MessageBox.Show("Not Found", filepath +
                    " not found. Load from CSV?", MessageBoxButtons.YesNo);
            }
            catch (Exception ex)
            {
                response = MessageBox.Show(
                    "Error", filepath + " encountered a problem. " + ex.Message +
                    " Load from CSV?", MessageBoxButtons.YesNo);
            }
            finally
            {
                if (response == DialogResult.Yes)
                {
                    //BUGFIX: Add CSV_Read function
                    throw new NotImplementedException();
                }
                else if (response == DialogResult.No)
                {
                    response = MessageBox.Show("Need a list to continue", "Create a new List?", MessageBoxButtons.YesNo);
                    if (response == DialogResult.Yes)
                    {
                        _innerList = new List<T> { };
                    }
                    else throw new ArgumentNullException("Must have a list or create one to continue executing");
                }
            }

        }

        public List<T> ToList() { return _innerList; }
        #endregion
    }
}
