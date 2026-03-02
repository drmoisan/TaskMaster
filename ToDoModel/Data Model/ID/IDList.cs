using Deedle;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Fields;
using Outlook = Microsoft.Office.Interop.Outlook;

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
                                                removeColumns: null, 
                                                addColumns: new string[]
                                                {
                                                    MAPIFields.Schemas.ToDoID,
                                                    "Categories",
                                                    MAPIFields.Schemas.MessageStore
                                                });

            var toDoColumn = ResolveColumnKey(df, "ToDoID", MAPIFields.Schemas.ToDoID);
            if (string.IsNullOrEmpty(toDoColumn))
            {
                this.Clear();
                _maxIDLength = 0;
                this.Serialize();
                return;
            }

            df = df.FillMissing("ERROR");
            df = df.Where(x =>
            {
                try
                {
                    return ((string)x.Value[toDoColumn]) != "ERROR";
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
            });
            var idList = df.GetColumn<string>(toDoColumn).Values.ToList();
            this.FromList(idList);
            _maxIDLength = this.Count == 0 ? 0 : this.Select(x => x.Length).Max();
            this.Serialize();
        }

        public async Task<string> SubstituteIdRootAsync(string oldId, string newRoot, string oldRoot) 
        {
            return await Task.Run(() => 
            { 
                var newId = oldId.Replace(oldRoot, newRoot);
                this.Remove(oldId);
                this.Add(newId);
                this.Serialize();
                return newId;
            });
                
            
        }

        public IAsyncEnumerable<IToDoItem> GetItemsWithRootIdAsync(string rootId) 
        {
            var strFilter = $"@SQL={MAPIFields.Schemas.ToDoID} like '{rootId}%'";
            var items = _olApp.Session.Stores
                ?.Cast<Store>()
                ?.ToAsyncEnumerable()
                ?.Select(TryGetDefaultToDoFolder)
                ?.Where(store => store is not null)
                ?.SelectMany(folder => 
                    folder?
                    .Items?
                    .Restrict(strFilter)?
                    .Cast<object>()?
                    .ToAsyncEnumerable()?
                    .Select(x => new ToDoItem(new OutlookItem(x))));
            return items;
        }

        internal MAPIFolder TryGetDefaultToDoFolder(Store store)
        {
            try
            {
                return store.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
            }
            catch (System.Exception)
            {
                return null;
            }
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
                                                    removeColumns: null, 
                                                    addColumns:
                                                    [
                                                        MAPIFields.Schemas.ToDoID,
                                                        "Categories",
                                                        MAPIFields.Schemas.MessageStore
                                                    ]);

                var toDoColumn = ResolveColumnKey(df, "ToDoID", MAPIFields.Schemas.ToDoID);
                var storeColumn = ResolveColumnKey(df, "Store", MAPIFields.Schemas.MessageStore);
                if (string.IsNullOrEmpty(toDoColumn) || string.IsNullOrEmpty(storeColumn))
                {
                    return;
                }

                df = df.FillMissing("");
                var df2 = df.Where(x =>
                {
                    try
                    {
                        return ((string)x.Value[toDoColumn]).Contains(oldPrefix);
                    }
                    catch (KeyNotFoundException)
                    {
                        return false;
                    }
                });

                foreach (var row in df2.Rows.Values)
                {
                    string entryID = row["EntryID"].ToString();
                    string storeID = row[storeColumn].ToString();
                    string todoOld = row[toDoColumn].ToString();
                    string todoNew = todoOld.Replace(oldPrefix, newPrefix);
                    var item = new OutlookItem(_olApp.Session.GetItemFromID(entryID, storeID));
                    item.TrySetUdf("ToDoID", todoNew);
                    this.Remove(todoOld);
                    this.Add(todoNew);
                }

                this.Serialize();
            }
        }

        private static string ResolveColumnKey(Frame<int, string> df, params string[] candidates)
        {
            if (df is null) { return null; }

            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrEmpty(candidate) && df.ColumnKeys.Contains(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Function Invokes the DataModel_ToDoTree.ReNumberIDs() method at the root level which 
        /// recursively calls DataModel_ToDoTree.ReNumberChildrenIDs() and then invokes the
        /// ListOfIDsLegacy.Save() Method
        /// </summary>
        /// <param name="appGlobals">Pointer to Outlook Application</param>
        public void CompressToDoIDs(IApplicationGlobals appGlobals)
        {
            var _dataModel = new TreeOfToDoItems();
            _dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadAll, appGlobals);
            var flat = _dataModel.TryFlatten()?.Select(x => x.ToDoID).ToList();
            if (flat is not null)
            {
                this.FromList(flat);
            }
            
            _dataModel.ReNumberIDs(this);
            this.Sort();
            this.Serialize();
        }
               
        public void SetOlApp(Outlook.Application olApp) { _olApp = olApp; }
    }
}
