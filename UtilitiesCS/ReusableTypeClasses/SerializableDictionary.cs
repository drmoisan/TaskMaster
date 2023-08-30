//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.IO;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using Swordfish.NET.Collections;

//namespace UtilitiesCS.ReusableTypeClasses
//{
//    [Serializable()]
//    public class SerializableDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, ISerializableDictionary<TKey, TValue>
//    {
//        public SerializableDictionary() : base() { }
//        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
//        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
//        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
//        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

//        public SerializableDictionary(string filename, string folderpath) : base() 
//        { 
//            Filename = filename; 
//            Folderpath = folderpath;
//        }

//        public SerializableDictionary(IDictionary<TKey, TValue> dictionary,
//                                      string filename,
//                                      string folderpath) : base(dictionary) 
//        { 
//            Filename = filename; 
//            Folderpath = folderpath; 
//        }

//        public SerializableDictionary(string filename,
//                                      string folderpath,
//                                      AltLoader<TKey, TValue> backupLoader,
//                                      string backupFilepath,
//                                      bool askUserOnError) : base()
//        {
//            Filename = filename;
//            Folderpath = folderpath;
//            _backupFilepath = backupFilepath;
//            Deserialize(_filepath, backupLoader, askUserOnError);
//        }

//        #region Serialization
//        public delegate Dictionary<TKey, TValue> AltLoader<TKey, TValue>(string filepath);

//        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//        private string _filepath = "";
//        private string _filename = "";
//        private string _folderpath = "";
//        private string _backupFilepath = "";

//        public string Filepath
//        {
//            get
//            {
//                return _filepath;
//            }
//            set
//            {
//                _filepath = value;
//                var fileExtension = Path.GetExtension(value);
//                _folderpath = Path.GetDirectoryName(_filepath);
//                _filename = Path.GetFileName(_filepath);
//                if ((value != "") && (fileExtension == "") && Directory.Exists(value))
//                {
//                    throw new ArgumentException(
//                        $"{value} is a Folder Path and was passed to the field named 'Filepath'. " +
//                        "Either pass this to the 'FileName' field or include a folderpath.");
//                }
//            }
//        }

//        public string Folderpath
//        {
//            get
//            {
//                return _folderpath;
//            }
//            set
//            {
//                _folderpath = value;
//                if (_filename != "")
//                    _filepath = Path.Combine(_folderpath, _filename);
//            }
//        }

//        public string Filename
//        {
//            get
//            {
//                return _filename;
//            }
//            set
//            {
//                _filename = value;
//                if (_folderpath != "")
//                    _filepath = Path.Combine(_folderpath, _filename);
//            }
//        }

//        public void Serialize()
//        {
//            if (Filepath != "")
//                Serialize(Filepath);
//        }

//        public void Serialize(string filepath)
//        {
//            this.Filepath = filepath;

//            string output = JsonConvert.SerializeObject(this, Formatting.Indented);
//            File.WriteAllText(filepath, output);

//        }

//        public void Deserialize()
//        {
//            if (Filepath != "") Deserialize(Filepath, true);
//        }

//        public void Deserialize(bool askUserOnError)
//        {
//            if (Filepath != "") Deserialize(Filepath, askUserOnError);
//        }

//        public void Deserialize(string filepath, AltLoader<TKey, TValue> backupLoader, bool askUserOnError)
//        {
//            if (_filepath != filepath) this.Filepath = filepath;

//            DialogResult response = DialogResult.Ignore;

//            try
//            {
//                var innerDictionary = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(File.ReadAllText(filepath));
//                foreach (var kvp in innerDictionary) { base.Add(kvp.Key, kvp.Value); }
//            }
//            catch (FileNotFoundException e)
//            {
//                log.Error(e.Message);
//                if (askUserOnError)
//                {
//                    response = MessageBox.Show($"{filepath} not found. Load from CSV?",
//                                               "File Not Found",
//                                               MessageBoxButtons.YesNo,
//                                               MessageBoxIcon.Error);
//                }
//                else
//                {
//                    response = DialogResult.Yes;
//                }
//            }
//            catch (System.Exception e)
//            {
//                log.Error(e.Message);
//                if (askUserOnError)
//                {
//                    response = MessageBox.Show($"{filepath} encountered a problem. {e.Message} " +
//                                               " Load from CSV?",
//                                               "Error!",
//                                               MessageBoxButtons.YesNo,
//                                               MessageBoxIcon.Error);
//                }
//                else
//                {
//                    response = DialogResult.Yes;
//                }
//            }
//            finally
//            {
//                if (response == DialogResult.Yes)
//                {
//                    if (_backupFilepath != "")
//                    {
//                        var innerDictionary = backupLoader(_backupFilepath);
//                        foreach (var kvp in innerDictionary) { base.Add(kvp.Key, kvp.Value); }
//                    }
//                    else
//                    {
//                        log.Debug($"Attempting to load {Path.GetFileName(filepath)} from CSV");

//                        var folder = Path.GetDirectoryName(filepath);
//                        var filename = Path.GetFileNameWithoutExtension(filepath) + ".csv";
//                        var innerDictionary = backupLoader(Path.Combine(folder, filename));
//                        foreach (var kvp in innerDictionary) { base.Add(kvp.Key, kvp.Value); }
//                    }
//                    Serialize();
//                }
//                else if (response == DialogResult.No)
//                {
//                    if (askUserOnError)
//                    {
//                        response = MessageBox.Show("Need a list to continue. " +
//                                                   "Create a new List Or Stop Execution?",
//                                                   "Error",
//                                                   MessageBoxButtons.YesNo,
//                                                   MessageBoxIcon.Error);
//                    }
//                    else { response = DialogResult.Yes; }

//                    if (response == DialogResult.Yes)
//                    {

//                    }
//                    else { throw new ArgumentNullException("Must have a list or create one to continue executing"); }
//                }
//            }

//        }

//        public void Deserialize(string filepath, bool askUserOnError)
//        {
//            if (_filepath != filepath) this.Filepath = filepath;

//            DialogResult response = DialogResult.Ignore;

//            try
//            {
//                var innerDictionary = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(File.ReadAllText(filepath));
//                foreach (var kvp in innerDictionary) { base.Add(kvp.Key, kvp.Value); }
//            }
//            catch (FileNotFoundException)
//            {
//                log.Error($"File {filepath} does not exist.");
//                if (askUserOnError)
//                {
//                    response = MessageBox.Show($"{filepath} not found. Create a new list? Excecution will stop if answer is no.",
//                                               "File Not Found",
//                                               MessageBoxButtons.YesNo,
//                                               MessageBoxIcon.Error);
//                }
//                else { response = DialogResult.Yes; }
//            }
//            catch (System.Exception e)
//            {
//                log.Error($"Error! {e.Message}");
//                if (askUserOnError)
//                {
//                    response = MessageBox.Show(filepath + " encountered a problem. " +
//                                               e.Message + " Create a new list? Excecution will stop if answer is no.",
//                                               "Error",
//                                               MessageBoxButtons.YesNo,
//                                               MessageBoxIcon.Error);
//                }
//                else { response = DialogResult.Yes; }
//            }
//        }

//        #endregion

//    }
//}
