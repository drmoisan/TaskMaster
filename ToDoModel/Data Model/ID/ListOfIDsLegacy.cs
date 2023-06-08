using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using UtilitiesCS;


using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using System.Runtime.CompilerServices;
using Deedle;

namespace ToDoModel
{

    [Serializable()]
    public class ListOfIDsLegacy : IListOfIDsLegacy
    {
        public ListOfIDsLegacy()
        {
            _usedIDList = new List<string>();
        }

        public ListOfIDsLegacy(List<string> listUsedID)
        {
            UsedIDList = listUsedID;
        }

        public ListOfIDsLegacy(string FilePath, Outlook.Application OlApp)
        {
            _olApp = OlApp;
            var tmpIDList = LoadFromFile(filepath: FilePath, olApp: OlApp);
            _usedIDList = tmpIDList.UsedIDList;
            Filepath = tmpIDList.Filepath;
        }

        public static ListOfIDsLegacy LoadFromFile(string filepath, Outlook.Application olApp)
        {
            var tmpIDList = new ListOfIDsLegacy();

            if (File.Exists(filepath))
            {
                var deserializer = new BinaryFormatter();
                try
                {
                    using (Stream TestFileStream = File.OpenRead(filepath))
                    {
                        tmpIDList = (ListOfIDsLegacy)deserializer.Deserialize(TestFileStream);
                    }
                }

                catch (UnauthorizedAccessException ex)
                {
                    tmpIDList = ProcessFileError(olApp, "Unexpected File Access Error. Recreate the list?");
                }

                catch (IOException ex)
                {
                    tmpIDList = ProcessFileError(olApp, "Unexpected IO Error. Is IDList File Corrupt?");
                }

                catch (InvalidCastException ex)
                {
                    tmpIDList = ProcessFileError(olApp, "File exists but cannot cast to ListOfIDsLegacy. Recreate the list?");
                }
            }

            else
            {
                tmpIDList = ProcessFileError(olApp, "File " + filepath + " does not exist. Recreate the List?");
            }

            tmpIDList.Filepath = filepath;
            return tmpIDList;
        }

        private List<string> _usedIDList;
        private long _maxIDLength;
        private string _filepath = "";
        private Outlook.Application _olApp;

        private static ListOfIDsLegacy ProcessFileError(Outlook.Application olApp, string msg)
        {
            var tmpIDList = new ListOfIDsLegacy();
            var result = MessageBox.Show(msg, "Error",MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                tmpIDList.RefreshIDList(olApp);
            }
            else
            {
                MessageBox.Show("Returning an empty list of ToDoIDs");
            }
            return tmpIDList;
        }
        
        public void SetOlApp(Outlook.Application olApp) { _olApp = olApp; }
        
        public void RefreshIDList(Outlook.Application Application)
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
            var df2 = df.Where(x => ((string)x.Value["ToDoID"]) != "ERROR");
            var idList = df.GetColumn<string>("ToDoID").Values.ToList();
            _usedIDList = idList;
            _maxIDLength = _usedIDList.Select(x => x.Length).Max();
            //var _dataModel = new TreeOfToDoItems();
            //List<object> _toDoList;
            //UsedIDList = new List<string>();

            //_toDoList = _dataModel.GetToDoList(TreeOfToDoItems.LoadOptions.vbLoadAll, Application);
            //_toDoList = _toDoList.Where(x => x.NoConflicts()).ToList();

            //foreach (object _objItem in _toDoList)
            //{

            //    string strID = _objItem.GetUdfString("ToDoID");

            //    if (UsedIDList.Contains(strID) == false & strID.Length != 0)
            //    {
            //        UsedIDList.Add(strID);
            //        if (strID.Length > _maxIDLength)
            //            _maxIDLength = strID.Length;
            //    }
            //}
            Save();

        }

        internal Frame<int, string> GetDfToDo(Store store)
        {
            var table = store.GetToDoTable();

            (var data, var columnInfo) = table.ExtractData();

            Frame<int, string> df = DfDeedle.FromArray2D(data: data, columnInfo);

            df = df.FillMissing("");

            return df;
        }

        public void SubstituteIdRoot(string oldPrefix, string newPrefix)
        {
            if (_olApp is null)
            {
                MessageBox.Show($"Coding Error. Cannot substitute id root without a handle to "+
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
                
                //df2.Print();
                //var firstRow = df2.GetRowAt<ObjectSeries<string>>(0);
                //var storeByte = firstRow["Store"];
                foreach (var row in df2.Rows.Values) 
                {
                    string entryID = row["EntryID"].ToString();
                    string storeID = row["Store"].ToString();
                    string todoOld = row["ToDoID"].ToString();
                    string todoNew = todoOld.Replace(oldPrefix, newPrefix);
                    object item = _olApp.Session.GetItemFromID(entryID, storeID);
                    item.SetUdf("ToDoID", todoNew);
                    _usedIDList.Remove(todoOld);
                    _usedIDList.Add(todoNew);
                }

                Save();

                //var mystore = (Store)storeByte;
                //var mystoreID = mystore.StoreID;
                //var addr = df.RowIndex.Locate(maxSentOn);
                //var idx = (int)dfDateIdx.RowIndex.AddressOperations.OffsetOf(addr);
                //var row = dfConversation.Rows.GetAt(idx);
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

        public long MaxIDLength
        {
            get
            {
                if (_maxIDLength == 0L)
                {
                    long maxLen = 0L;
                    foreach (string strID in UsedIDList)
                    {
                        if (strID.Length > maxLen)
                        {
                            maxLen = strID.Length;
                        }
                    }
                    _maxIDLength = maxLen;
                }
                return _maxIDLength;

            }
        }

        public List<string> UsedIDList
        {
            get
            {
                return _usedIDList;
            }
            set
            {
                _usedIDList = value;
            }
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
            }
        }

        public string GetNextAvailableToDoID(string strSeed)
        {
            int encoderBase = 36; // 125;

            bool blContinue = true;
            var lngMaxID = ConvertToDecimal(encoderBase, strSeed);
            string strMaxID = "";

            while (blContinue)
            {
                lngMaxID += 1;
                strMaxID = ConvertToBase(encoderBase, lngMaxID);
                if (UsedIDList.Contains(strMaxID) == false)
                {
                    blContinue = false;
                }
            }
            UsedIDList.Add(strMaxID);
            if (strMaxID.Length > _maxIDLength)
            {
                _maxIDLength = strMaxID.Length;
                Properties.Settings.Default.MaxIDLength = (int)_maxIDLength;
                Properties.Settings.Default.Save();
            }
            return strMaxID;
        }

        public string GetMaxToDoID()
        {
            int encoderBase = 36; // 125;

            string strMaxID = UsedIDList.Max();
            var lngMaxID = ConvertToDecimal(encoderBase, strMaxID);
            lngMaxID += 1;
            strMaxID = ConvertToBase(encoderBase, lngMaxID);
            UsedIDList.Add(strMaxID);
            if (strMaxID.Length > _maxIDLength)
            {
                _maxIDLength = strMaxID.Length;
                Properties.Settings.Default.MaxIDLength = (int)_maxIDLength;
                Properties.Settings.Default.Save();
            }

            return strMaxID;
        }

        public void Save(string Filepath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(Filepath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Filepath));
            }

            var serializer = new BinaryFormatter();
            using (Stream TestFileStream = File.Create(Filepath))
            {
                serializer.Serialize(TestFileStream, this);
            }

            this.Filepath = Filepath;
        }

        public void Save()
        {
            if (Filepath.Length > 0)
            {
                var serializer = new BinaryFormatter();
                using (Stream TestFileStream = File.Create(Filepath))
                {
                    serializer.Serialize(TestFileStream, this);
                }
            }
            else
            {
                MessageBox.Show("Can't save. IDList FileName not set yet");
            }
        }

        public string ConvertToBase(int nbase, BigInteger num, int intMinDigits = 2)
        {
            string ConvertToBaseRet = default;
            string chars;
            BigInteger r;
            string newNumber;
            int maxBase;
            int i;

            // chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
            // chars = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ";
            chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            maxBase = (chars.Length);

            // check if we can convert to this base
            if (nbase > maxBase)
            {
                ConvertToBaseRet = "";
            }
            else
            {

                // in r we have the offset of the char that was converted to the new base
                newNumber = "";
                while (num >= nbase)
                {
                    r = num % nbase;
                    newNumber = chars.Substring((int)(r + 1), 1) + newNumber;
                    num /= nbase;
                }

                newNumber = chars.Substring((int)(num + 1), 1) + newNumber;

                var loopTo = (newNumber.Length) % intMinDigits;
                for (i = 1; i <= loopTo; i++)
                    newNumber = 0 + newNumber;

                ConvertToBaseRet = newNumber;
            }

            return ConvertToBaseRet;
        }

        public BigInteger ConvertToDecimal(int nbase, string strBase)
        {
            BigInteger ConvertToDecimalRet = default;
            string chars;
            int i;
            int intLoc;
            BigInteger lngTmp;

            // chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ"
            // chars = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ";
            chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            lngTmp = 0;

            var loopTo = (strBase.Length -1);
            for (i = 1; i <= loopTo; i++)
            {
                lngTmp *= nbase;
                intLoc = chars.IndexOf(strBase.Substring(i, 1));
                lngTmp += intLoc - 1;
            }

            ConvertToDecimalRet = lngTmp;
            return ConvertToDecimalRet;
        }
        
    }
}