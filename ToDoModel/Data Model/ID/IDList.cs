using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace ToDoModel.Data_Model.ID
{
    public class IDList: SerializableList<string>
    {
        #region constructors

        public IDList() : base() { }
        public IDList(IList<string> list) : base(list) { }
        public IDList(IEnumerable<string> IEnumerableOfString) : base(IEnumerableOfString) { }
        public IDList(string filename, string folderpath) : base(filename, folderpath) { }
        public IDList(string filename,
                      string folderpath,
                      Outlook.Application olApp) : base(filename, folderpath) 
        { 
            _olApp = olApp;
        }
        
        public IDList(string filename,
                      string folderpath,
                      CSVLoader<string> backupLoader,
                      string backupFilepath,
                      bool askUserOnError) : base(filename,
                                                  folderpath,
                                                  backupLoader,
                                                  backupFilepath,
                                                  askUserOnError) { }

        public IDList(string filename,
                      string folderpath,
                      CSVLoader<string> backupLoader,
                      string backupFilepath,
                      bool askUserOnError,
                      Outlook.Application olApp) : base(filename,
                                                        folderpath,
                                                        backupLoader,
                                                        backupFilepath,
                                                        askUserOnError)
        {
            _olApp = olApp;
        }

        #endregion

        private Outlook.Application _olApp;
        private int _maxIDLength = 0;

        public int MaxLengthOfID
        {
            get
            {
                if (_maxIDLength == 0)
                {
                    _maxIDLength = this.Select(x => x.Length).Max();
                }
                return _maxIDLength;
            }
        }

        public void SetOlApp(Outlook.Application olApp) { _olApp = olApp; }
    }
}
