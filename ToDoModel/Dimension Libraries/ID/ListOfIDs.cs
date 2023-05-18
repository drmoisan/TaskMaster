using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;


using UtilitiesVB;

namespace ToDoModel
{

    [Serializable()]
    public class ListOfIDs : IListOfIDs
    {

        private List<string> _usedIDList;
        private long _maxIDLength;
        private string _filepath = "";

        public ListOfIDs(List<string> listUsedID)
        {
            UsedIDList = listUsedID;
        }

        public ListOfIDs(string FilePath, Outlook.Application OlApp)
        {
            LoadFromFile(FilePath: FilePath, OlApp: OlApp);
        }

        public ListOfIDs()
        {
            _usedIDList = new List<string>();
        }

        public static ListOfIDs LoadFromFile(string FilePath, Outlook.Application OlApp)
        {
            var tmpIDList = new ListOfIDs();

            if (File.Exists(FilePath))
            {
                var deserializer = new BinaryFormatter();
                try
                {
                    using (Stream TestFileStream = File.OpenRead(FilePath))
                    {
                        tmpIDList = (ListOfIDs)deserializer.Deserialize(TestFileStream);
                    }
                }

                catch (UnauthorizedAccessException ex)
                {
                    tmpIDList = ProcessFileError(OlApp, "Unexpected File Access Error. Recreate the list?");
                }

                catch (IOException ex)
                {
                    tmpIDList = ProcessFileError(OlApp, "Unexpected IO Error. Is IDList File Corrupt?");
                }

                catch (InvalidCastException ex)
                {
                    tmpIDList = ProcessFileError(OlApp, "File exists but cannot cast to ListOfIDs. Recreate the list?");
                }
            }

            else
            {
                tmpIDList = ProcessFileError(OlApp, "File " + FilePath + " does not exist. Recreate the List?");
            }

            tmpIDList.Filepath = FilePath;
            return tmpIDList;
        }

        private static ListOfIDs ProcessFileError(Outlook.Application OlApp, string msg)
        {
            var tmpIDList = new ListOfIDs();
            var result = MessageBox.Show(msg, "Error",MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                tmpIDList.RefreshIDList(OlApp);
            }
            else
            {
                MessageBox.Show("Returning an empty list of ToDoIDs");
            }
            return tmpIDList;
        }

        public void RefreshIDList(Outlook.Application Application)
        {
            var unused = new object();
            var _dataModel = new TreeOfToDoItems();
            List<object> _toDoList;
            UsedIDList = new List<string>();

            _toDoList = _dataModel.GetToDoList(TreeOfToDoItems.LoadOptions.vbLoadAll, Application);

            foreach (object _objItem in _toDoList)
            {
                string strID = CustomFieldID_GetValue(_objItem, "ToDoID");
                if (UsedIDList.Contains(strID) == false & strID.Length != 0)
                {
                    UsedIDList.Add(strID);
                    if (strID.Length > _maxIDLength)
                        _maxIDLength = strID.Length;
                }
            }
        }

        /// <summary>
    /// Function Invokes the DataModel_ToDoTree.ReNumberIDs() method at the root level which 
    /// recursively calls DataModel_ToDoTree.ReNumberChildrenIDs() and then invokes the
    /// ListOfIDs.Save() Method
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
            bool blContinue = true;
            var lngMaxID = ConvertToDecimal(125, strSeed);
            string strMaxID = "";

            while (blContinue)
            {
                lngMaxID += 1;
                strMaxID = ConvertToBase(125, lngMaxID);
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
            string strMaxID = UsedIDList.Max();
            var lngMaxID = ConvertToDecimal(125, strMaxID);
            lngMaxID += 1;
            strMaxID = ConvertToBase(125, lngMaxID);
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
            chars = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ";
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
            chars = "0123456789aAáÁàÀâÂäÄãÃåÅæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHIIíÍìÌîÎïÏjJkKlLmMnNñÑoOóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtTþÞuUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽ";
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

        private string CustomFieldID_GetValue(object objItem, string UserDefinedFieldName)
        {
            MailItem OlMail;
            TaskItem OlTask;
            AppointmentItem OlAppt;
            UserProperty objProperty;


            if (objItem is null)
            {
                return "";
            }
            else if (objItem is MailItem)
            {
                OlMail = (MailItem)objItem;
                objProperty = OlMail.UserProperties.Find(UserDefinedFieldName);
            }

            else if (objItem is TaskItem)
            {
                OlTask = (TaskItem)objItem;
                objProperty = OlTask.UserProperties.Find(UserDefinedFieldName);
            }
            else if (objItem is AppointmentItem)
            {
                OlAppt = (AppointmentItem)objItem;
                objProperty = OlAppt.UserProperties.Find(UserDefinedFieldName);
            }
            else
            {
                objProperty = null;
                MessageBox.Show("Unsupported object type");
            }

            return objProperty is null ? "" : objProperty is Array ? FlattenArry((object[])objProperty.Value) : (string)objProperty.Value;

            OlMail = null;
            OlTask = null;
            OlAppt = null;
            objProperty = null;

        }

        public string FlattenArry(object[] varBranch)
        {
            string FlattenArryRet = default;
            int i;
            string strTemp;

            strTemp = "";

            var loopTo = (varBranch.Length - 1);
            for (i = 0; i <= loopTo; i++)
                strTemp = varBranch[i] is Array ? strTemp + ", " + FlattenArry((object[])varBranch[i]) : strTemp + ", " + varBranch[i];
            if (strTemp.Length != 0)
                strTemp = strTemp.Substring(2);
            FlattenArryRet = strTemp;
            return FlattenArryRet;
        }
    }
}