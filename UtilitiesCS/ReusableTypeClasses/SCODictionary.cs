using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swordfish.NET.Collections;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace UtilitiesCS.ReusableTypeClasses
{
    /// <summary>
    /// Class that implements a serializable concurrent observable dictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable()]
    public class SCODictionary<TKey, TValue>: ConcurrentObservableDictionary<TKey, TValue>, ISCODictionary<TKey, TValue>
    {
        #region constructors
        public SCODictionary() : base() { }
        public SCODictionary(IDictionary<TKey, TValue> source) : base(source) { }
        public SCODictionary(IEqualityComparer<TKey> equalityComparer) : base(equalityComparer) { }    
        public SCODictionary(int capactity) : base(capactity) { }
        public SCODictionary(IDictionary<TKey, TValue> source, IEqualityComparer<TKey> equalityComparer) : base(source, equalityComparer) { }
        public SCODictionary(int capacity, IEqualityComparer<TKey> equalityComparer) : base(capacity, equalityComparer) { }

        public SCODictionary(string filename,
                             string folderpath) : base()
        {
            Filename = filename;
            Folderpath = folderpath;
            Deserialize();
        }

        /// <summary>
        /// Creates a new serializable concurrent observable dictionary from an existing dictionary and filepath
        /// </summary>
        /// <param name="dictionary">Existing dictionary</param>
        /// <param name="filename">Name of json file to house the SCODictionary</param>
        /// <param name="folderpath">Location of json file</param>
        public SCODictionary(IDictionary<TKey, TValue> dictionary,
                             string filename,
                             string folderpath) : base(dictionary)
        {
            Filename = filename;
            Folderpath = folderpath;
            Serialize();
        }

        public SCODictionary(string filename,
                             string folderpath,
                             ISCODictionary<TKey, TValue>.AltLoader backupLoader,
                             string backupFilepath,
                             bool askUserOnError) : base()
        {
            Filename = filename;
            Folderpath = folderpath;
            _backupFilepath = backupFilepath;
            Deserialize(_filepath, backupLoader, askUserOnError);
        }
        #endregion

        #region Serialization
        

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _filepath = "";
        private string _filename = "";
        private string _folderpath = "";
        private string _backupFilepath = "";

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

        public async void Serialize(string filepath)
        {
            this.Filepath = filepath;
            _ = Task.Run(() => SerializeThreadSafe(filepath));
            await Task.Delay(1);
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
                        //settings.TypeNameHandling = TypeNameHandling.Auto;
                        settings.Formatting = Formatting.Indented;

                        var serializer = JsonSerializer.Create(settings);
                        serializer.Serialize(sw, this);
                        sw.Close();
                    }
                }
                catch (Exception e)
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

        private async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        public void Deserialize()
        {
            if (Filepath != "") Deserialize(Filepath, true);
        }

        public void Deserialize(bool askUserOnError)
        {
            if (Filepath != "") Deserialize(Filepath, askUserOnError);
        }

        public void Deserialize(string filepath, ISCODictionary<TKey, TValue>.AltLoader backupLoader, bool askUserOnError)
        {
            if (_filepath != filepath) this.Filepath = filepath;

            DialogResult response = DialogResult.Ignore;

            try
            {
                string strObject = File.ReadAllText(filepath, Encoding.UTF8);
                //var innerDictionary = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(File.ReadAllText(filepath));
                var innerDictionary = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(strObject);
                foreach (var kvp in innerDictionary) { this.Add(kvp.Key, kvp.Value); }
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
                        var innerDictionary = backupLoader(_backupFilepath);
                        foreach (var kvp in innerDictionary) { base.Add(kvp.Key, kvp.Value); }
                    }
                    else
                    {
                        log.Debug($"Attempting to load {Path.GetFileName(filepath)} from CSV");

                        var folder = Path.GetDirectoryName(filepath);
                        var filename = Path.GetFileNameWithoutExtension(filepath) + ".csv";
                        var innerDictionary = backupLoader(Path.Combine(folder, filename));
                        foreach (var kvp in innerDictionary) { base.Add(kvp.Key, kvp.Value); }
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
                string strObject = File.ReadAllText(filepath, Encoding.UTF8);
                //var innerDictionary = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(File.ReadAllText(filepath));
                var innerDictionary = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(strObject);
                foreach (var kvp in innerDictionary) { this.Add(kvp.Key, kvp.Value); }
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
        }

        Dictionary<TKey, TValue> ISCODictionary<TKey, TValue>.ToDictionary()
        {
            return new Dictionary<TKey, TValue>(this);
        }

        #endregion


    }
}
