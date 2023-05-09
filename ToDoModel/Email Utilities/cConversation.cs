using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace ToDoModel
{

    public class cConversation
    {
        private object _item;
        private Conversation pConversation;
        private Table pTable;
        private Collection pCollection;
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
                pConversation = (Conversation)value.GetConversation;
                if (pConversation is not null)
                {
                    pTable = pConversation.GetTable();
                    pTable.Columns.Add("http://schemas.microsoft.com/mapi/proptag/0x0e05001f");
                    _item = value;
                }
            }
        }

        public void Enumerate()
        {
            Row oRow;
            while (!pTable.EndOfTable)
            {
                oRow = pTable.GetNextRow();
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
                    pCollection = get_ToCollection(OnlySameFolder);
                    CountRet = pCollection.Count;
                }
                else
                {
                    CountRet = pTable.GetRowCount();
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
                pTable.Sort("[ReceivedTime]", true);

                while (!pTable.EndOfTable)
                {
                    oRow = pTable.GetNextRow();
                    // Use EntryID and StoreID to open the item.
                    objItem = _olApp.Session.GetItemFromID(Conversions.ToString(oRow["EntryID"]));
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
                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(objItem.Parent.Name, _item.Parent.Name, false)))
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
                if (OnlySameFolder)
                {
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(objItem.Parent.Name, _item.Parent.Name, false)))
                    {
                        listEmails.Add((MailItem)objItem);
                    }
                }
                else
                {
                    listEmails.Add((MailItem)objItem);
                }
            }
        }

        public Collection get_ToCollection(bool OnlySameFolder = false)
        {
            if (_item is not null)
            {
                Row oRow;
                object objItem;
                pCollection = new Collection();
                pTable.Sort("[ReceivedTime]", true);

                while (!pTable.EndOfTable)
                {
                    oRow = pTable.GetNextRow();
                    // Use EntryID and StoreID to open the item.
                    objItem = _olApp.Session.GetItemFromID(Conversions.ToString(oRow["EntryID"]));
                    if (OnlySameFolder)
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(objItem.Parent.Name, _item.Parent.Name, false)))
                        {
                            pCollection.Add(objItem);
                        }
                    }
                    else
                    {
                        pCollection.Add(objItem);
                    }
                }
                return pCollection;
            }
            else
            {
                return null;
            }
        }



        private void DemoConversationTable()
        {
            Conversation oConv;
            Table oTable;
            Row oRow;
            MailItem oMail;
            MailItem oItem;
            const string PR_STORE_ENTRYID = "https://schemas.microsoft.com/mapi/proptag/0x0FFB0102";
            ;
            // Obtain the current item for the active inspector.
#error Cannot convert OnErrorResumeNextStatementSyntax - see comment for details
            /* Cannot convert OnErrorResumeNextStatementSyntax, CONVERSION ERROR: Conversion for OnErrorResumeNextStatement not implemented, please report this issue in 'On Error Resume Next' at character 6141


                        Input:

                                On Error Resume Next

                         */
            oMail = (MailItem)_olApp.ActiveInspector().CurrentItem;

            if (oMail is not null)
            {
                // Obtain the Conversation object.
                oConv = oMail.GetConversation();
                if (oConv is not null)
                {
                    oTable = oConv.GetTable();
                    var unused = oTable.Columns.Add(PR_STORE_ENTRYID);
                    while (!oTable.EndOfTable)
                    {
                        oRow = oTable.GetNextRow();
                        // Use EntryID and StoreID to open the item.
                        oItem = (MailItem)_olApp.Session.GetItemFromID(Conversions.ToString(oRow["EntryID"]), oRow.BinaryToString(PR_STORE_ENTRYID));
                        Debug.Print(oItem.Subject, "Attachments.Count=" + oItem.Attachments.Count);
                    }
                }
            }
        }


    }
}