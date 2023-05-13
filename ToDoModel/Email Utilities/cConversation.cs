using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Office.Interop.Outlook;



namespace ToDoModel
{

    public class cConversation
    {
        private object _item;
        private Conversation _conversation;
        private Table _table;
        private IList _pList;
        private Application _olApp;
        // Private Const PR_STORE_ENTRYID As String = "https://schemas.microsoft.com/mapi/proptag/0x0FFB0102"
        // Private Const FOLDERNAME As String = "http://schemas.microsoft.com/mapi/proptag/0x0e05001f"

        public cConversation(Application OlApp)
        {
            _olApp = OlApp;
        }

        public object item
        {
            set
            {
                _item = value;
                dynamic temp = _item;
                _conversation = (Conversation)temp.GetConversation();
                if (_conversation is not null)
                {
                    _table = _conversation.GetTable();
                    _table.Columns.Add("http://schemas.microsoft.com/mapi/proptag/0x0e05001f");
                    _item = value;
                }
            }
        }

        public void Enumerate()
        {
            Row oRow;
            while (!_table.EndOfTable)
            {
                oRow = _table.GetNextRow();
                // Use EntryID and StoreID to open the item.
                Debug.WriteLine(oRow["Subject"]);
                Debug.WriteLine(oRow["http://schemas.microsoft.com/mapi/proptag/0x0e05001f"]);
            }
        }

        public long get_Count(bool OnlySameFolder = false)
        {
            long CountRet = default;
            if (_item is not null)
            {
                if (OnlySameFolder)
                {
                    _pList = get_ToCollection(OnlySameFolder);
                    CountRet = _pList.Count;
                }
                else
                {
                    CountRet = _table.GetRowCount();
                }
            }
            else
            {
                CountRet = 0L;
            }

            return CountRet;
        }

        public IList get_ToList(bool OnlySameFolder = false, bool MailOnly = true)
        {
            if (_item is not null)
            {
                Row oRow;
                object objItem;
                var listObjects = new List<object>();
                var listEmail = new List<MailItem>();
                _table.Sort("[ReceivedTime]", true);

                while (!_table.EndOfTable)
                {
                    oRow = _table.GetNextRow();
                    // Use EntryID and StoreID to open the item.
                    objItem = _olApp.Session.GetItemFromID(oRow["EntryID"]);
                    if (MailOnly)
                    {
                        AddEmailToList(OnlySameFolder, objItem, ref listEmail);
                    }
                    else
                    {
                        AddObjectToList(OnlySameFolder, objItem, listObjects);
                    }
                }

                if (MailOnly)
                {
                    return listEmail;
                }
                else
                {
                    return listObjects;
                }
            }

            else
            {
                return null;
            }

        }

        private void AddObjectToList(bool OnlySameFolder, object objItem, List<object> listObjects)
        {
            if (OnlySameFolder)
            {
                if (((dynamic)objItem).Parent.Name == ((dynamic)_item).Parent.Name)
                {
                    listObjects.Add(objItem);
                }
            }
            else
            {
                listObjects.Add(objItem);
            }
        }

        private void AddEmailToList(bool OnlySameFolder, object objItem, ref List<MailItem> listEmails)
        {
            if (objItem is MailItem)
            {
                MailItem mailItem = (MailItem)objItem;
                if (OnlySameFolder)
                {
                    if (mailItem.Parent.Name == ((MailItem)_item).Parent.Name)
                    {
                        listEmails.Add(mailItem);
                    }
                }
                else
                {
                    listEmails.Add(mailItem);
                }
            }
        }

        public IList get_ToCollection(bool OnlySameFolder = false)
        {
            if (_item is not null)
            {
                Row oRow;
                object objItem;
                _pList = new List<object>();
                _table.Sort("[ReceivedTime]", true);

                while (!_table.EndOfTable)
                {
                    oRow = _table.GetNextRow();
                    // Use EntryID and StoreID to open the item.
                    objItem = _olApp.Session.GetItemFromID(oRow["EntryID"]);
                    if (OnlySameFolder)
                    {
                        if (((dynamic)objItem).Parent.Name == ((dynamic)_item).Parent.Name)
                        {
                            _pList.Add(objItem);
                        }
                    }
                    else
                    {
                        _pList.Add(objItem);
                    }
                }
                return _pList;
            }
            else
            {
                return null;
            }
        }

    }
}