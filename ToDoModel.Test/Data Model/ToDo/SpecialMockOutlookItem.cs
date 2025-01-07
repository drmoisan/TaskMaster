using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace ToDoModel.Test.Data_Model.ToDo
{
    internal class SpecialMockOutlookItem : IOutlookItem
    {
        public Actions Actions => throw new NotImplementedException();

        public Application Application => throw new NotImplementedException();

        public object[] Args => throw new NotImplementedException();

        public Attachments Attachments => throw new NotImplementedException();

        public string BillingInformation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Body { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Categories { get; set; }

        public OlObjectClass Class => throw new NotImplementedException();

        public string Companies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string ConversationIndex => throw new NotImplementedException();

        public string ConversationTopic => throw new NotImplementedException();

        public DateTime CreationTime { get; set; }

        public OlDownloadState DownloadState => throw new NotImplementedException();

        public string EntryID => throw new NotImplementedException();

        public FormDescription FormDescription => throw new NotImplementedException();

        public Inspector Inspector => throw new NotImplementedException();

        public OlImportance Importance { get; set; }

        public object InnerObject { get; set; }

        public bool IsConflict => throw new NotImplementedException();

        public ItemProperties ItemProperties => throw new NotImplementedException();

        public DateTime LastModificationTime => throw new NotImplementedException();

        public Links Links => throw new NotImplementedException();

        public OlRemoteStatus MarkForDownload { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string MessageClass { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Mileage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool NoAging { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public long OutlookInternalVersion => throw new NotImplementedException();

        public string OutlookVersion => throw new NotImplementedException();

        public Folder Parent => throw new NotImplementedException();

        public PropertyAccessor PropertyAccessor => throw new NotImplementedException();

        public DateTime ReminderTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Saved => throw new NotImplementedException();

        public OlSensitivity Sensitivity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public NameSpace Session => throw new NotImplementedException();

        public long Size => throw new NotImplementedException();

        public string Subject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DateTime TaskStartDate { get; set; }

        public bool UnRead { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public UserProperties UserProperties => throw new NotImplementedException();

        public void Close(OlInspectorClose SaveMode)
        {
            throw new NotImplementedException();
        }

        public object Copy()
        {
            throw new NotImplementedException();
        }

        public void Display()
        {
            throw new NotImplementedException();
        }

        public object Move(Folder DestinationFolder)
        {
            throw new NotImplementedException();
        }

        public void PrintOut()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void SaveAs(string path, OlSaveAsType type)
        {
            throw new NotImplementedException();
        }

        public void ShowCategoriesDialog()
        {
            throw new NotImplementedException();
        }
    }
}
