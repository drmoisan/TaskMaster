using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using Microsoft.Office.Interop.Outlook;
using Deedle;
using System.Windows.Forms;
using UtilitiesCS.OutlookExtensions;

namespace ToDoModel
{
    public class IDList : SerializableList<string>, IIDList
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
                                                  askUserOnError)
        { }

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

        public string GetNextToDoID(string strSeed)
        {
            int encoderBase = 36; // 125;

            bool blContinue = true;
            var lngMaxID = strSeed.ToBase10(encoderBase);
            string strMaxID = "";

            while (blContinue)
            {
                lngMaxID += 1;
                strMaxID = lngMaxID.ToBase(encoderBase);
                if (!this.Contains(strMaxID))
                {
                    blContinue = false;
                }
            }
            this.Add(strMaxID);
            if (strMaxID.Length > _maxIDLength)
            {
                _maxIDLength = strMaxID.Length;
                Properties.Settings.Default.MaxLengthOfID = (int)_maxIDLength;
                Properties.Settings.Default.Save();
            }
            if (this.Filepath is not null) { this.Serialize(); }
            return strMaxID;
        }

        public string GetNextToDoID()
        {
            string strSeed = this.Max();
            return GetNextToDoID(strSeed);
        }

        public void RefreshIDList(Outlook.Application olApp)
        {
            _olApp = olApp;
            RefreshIDList();
        }

        public void RefreshIDList()
        {
            var df = DfDeedle.FromDefaultFolder(stores: _olApp.Session.Stores,
                                                    folderEnum: OlDefaultFolders.olFolderToDo,
                                                    removeColumns: null, //new string[] {"RemoveAll"},
                                                    addColumns: new string[]
                                                    {
                                                        OlTableExtensions.SchemaToDoID,
                                                        "Categories",
                                                        OlTableExtensions.SchemaMessageStore
                                                    });

            df = df.FillMissing("ERROR");
            df = df.Where(x => ((string)x.Value["ToDoID"]) != "ERROR");
            var idList = df.GetColumn<string>("ToDoID").Values.ToList();
            this.FromList(idList);
            _maxIDLength = this.Select(x => x.Length).Max();
            this.Serialize();
        }

        public void SubstituteIdRoot(string oldPrefix, string newPrefix)
        {
            if (_olApp is null)
            {
                MessageBox.Show($"Coding Error. Cannot substitute id root without a handle to " +
                    $"the Outlook Application. Please use the {nameof(SetOlApp)} method.");
            }
            else
            {
                var df = DfDeedle.FromDefaultFolder(stores: _olApp.Session.Stores,
                                                    folderEnum: OlDefaultFolders.olFolderToDo,
                                                    removeColumns: null, //new string[] {"RemoveAll"},
                                                    addColumns: new string[]
                                                    {
                                                        OlTableExtensions.SchemaToDoID,
                                                        "Categories",
                                                        OlTableExtensions.SchemaMessageStore
                                                    });

                df = df.FillMissing("");
                var df2 = df.Where(x => ((string)x.Value["ToDoID"]).Contains(oldPrefix));

                foreach (var row in df2.Rows.Values)
                {
                    string entryID = row["EntryID"].ToString();
                    string storeID = row["Store"].ToString();
                    string todoOld = row["ToDoID"].ToString();
                    string todoNew = todoOld.Replace(oldPrefix, newPrefix);
                    object item = _olApp.Session.GetItemFromID(entryID, storeID);
                    item.SetUdf("ToDoID", todoNew);
                    this.Remove(todoOld);
                    this.Add(todoNew);
                }

                this.Serialize();
            }
        }

        /// <summary>
        /// Function Invokes the DataModel_ToDoTree.ReNumberIDs() method at the root level which 
        /// recursively calls DataModel_ToDoTree.ReNumberChildrenIDs() and then invokes the
        /// ListOfIDsLegacy.Save() Method
        /// </summary>
        /// <param name="OlApp">Pointer to Outlook Application</param>
        public void CompressToDoIDs(Outlook.Application OlApp)
        {
            var _dataModel = new TreeOfToDoItems();
            _dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadAll, OlApp);
            _dataModel.ReNumberIDs(this);
        }

        public void SetOlApp(Outlook.Application olApp) { _olApp = olApp; }
    }
}
