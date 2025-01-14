using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;


namespace UtilitiesCS.OutlookExtensions
{
    public class OutlookItemTry: IOutlookItem
    {
        protected IOutlookItem _olItem;
        protected object _item;  // the wrapped Outlook item
        protected Type _type;  // type for the Outlook item 
        protected object[] _args;  // dummy argument array
        //private System.Type _typeOlObjectClass;

        public OutlookItemTry(IOutlookItem olItem)
        {
            _olItem = olItem;
            _item = olItem.InnerObject;
            _type = olItem.ItemType;
            _args = olItem.Args;
        }

        #region Predefined Properties

        public Outlook.Actions Actions => TryGet(()=>_olItem.Actions);

        public Outlook.Application Application => TryGet(()=>_olItem.Application);

        public Outlook.Attachments Attachments => TryGet(() => _olItem.Attachments);
        
        public string BillingInformation { get => TryGet(() => _olItem.BillingInformation); set => TrySet<string>((x) => _olItem.BillingInformation = x, value); }

        public string Body { get => TryGet(() => _olItem.Body); set => TrySet<string>((x) => _olItem.Body = x, value); }

        public string Categories { get => TryGet(() => _olItem.Categories); set => TrySet<string>((x) => _olItem.Categories = x, value); }

        public string Companies { get => TryGet(() => _olItem.Companies); set => TrySet<string>((x) => _olItem.Companies = x, value); }

        public Outlook.OlObjectClass OlObjectClass => TryGet(() => _olItem.Class);
        
        public string ConversationIndex => TryGet(() => _olItem.ConversationIndex);

        public string ConversationTopic => TryGet(() => _olItem.ConversationTopic);

        public System.DateTime CreationTime => TryGet(() => _olItem.CreationTime);

        public Outlook.OlDownloadState DownloadState => TryGet(() => _olItem.DownloadState);

        public string EntryID => TryGet(() => _olItem.EntryID);

        public Outlook.FormDescription FormDescription => TryGet(() => _olItem.FormDescription);

        public Object InnerObject => TryGet(() => _olItem.InnerObject);

        public Outlook.Inspector GetInspector => TryGet(() => _olItem.Inspector);

        public Outlook.OlImportance Importance { get => TryGet(() => _olItem.Importance); set => TrySet((x) => _olItem.Importance = x, value); }

        public bool IsConflict => TryGet(() => _olItem.IsConflict);

        public Outlook.ItemProperties ItemProperties => TryGet(() => _olItem.ItemProperties);

        public System.DateTime LastModificationTime => TryGet(() => _olItem.LastModificationTime);

        public Outlook.Links Links => TryGet(() => _olItem.Links);

        public Outlook.OlRemoteStatus MarkForDownload { get => TryGet(() => _olItem.MarkForDownload); set => TrySet((x) => _olItem.MarkForDownload = x, value); }

        public string MessageClass { get => TryGet(() => _olItem.MessageClass); set => TrySet((x) => _olItem.MessageClass = x, value); }

        public string Mileage { get => TryGet(() => _olItem.Mileage); set => TrySet((x) => _olItem.Mileage = x, value); }

        public object Move(Outlook.Folder DestinationFolder) => TryGet(() => _olItem.Move(DestinationFolder));
        
        public OlItemType OlItemType => TryGet(_olItem.GetOlItemType);

        public long OutlookInternalVersion => TryGet(() => _olItem.OutlookInternalVersion);

        public string OutlookVersion => TryGet(() => _olItem.OutlookVersion);

        public Outlook.Folder Parent => TryGet(() => _olItem.Parent);

        public Outlook.PropertyAccessor PropertyAccessor => TryGet(() => _olItem.PropertyAccessor);

        public bool Saved => TryGet(() => _olItem.Saved);

        public Outlook.OlSensitivity Sensitivity { get => TryGet(() => _olItem.Sensitivity); set => TrySet((x) => _olItem.Sensitivity = x, value); }

        public Outlook.NameSpace Session => TryGet(() => _olItem.Session);

        public long Size => TryGet(() => _olItem.Size);

        public string Subject { get => TryGet(() => _olItem.Subject); set => TrySet((x) => _olItem.Subject = x, value); }

        public string SenderName => TryGet(() => ((Outlook.Recipient)_olItem.GetPropertyValue<Recipient>("Sender")).Name);

        public bool UnRead { get => TryGet(() => _olItem.UnRead); set => TrySet((x) => _olItem.UnRead = x, value); }

        public Outlook.UserProperties UserProperties => TryGet(() => _olItem.UserProperties);

        public object[] Args => TryGet(() => _olItem.Args);

        public OlObjectClass Class => TryGet(() => _olItem.Class);

        public Inspector Inspector => TryGet(() => _olItem.Inspector);

        public Type ItemType => TryGet(() => _olItem.ItemType);

        public bool NoAging { get => TryGet(() => _olItem.NoAging); set => TrySet((x) => _olItem.NoAging = x, value); }
        public DateTime ReminderTime { get => TryGet(() => _olItem.ReminderTime); set => TrySet((x) => _olItem.ReminderTime = x, value); }

        public DateTime TaskStartDate => TryGet(() => _olItem.TaskStartDate);

        #endregion Predefined Properties

        #region Predefined Methods

        public void Close(Outlook.OlInspectorClose SaveMode) => TryCall(() => _olItem.Close(SaveMode));
        
        public object Copy() => TryCall(() => _olItem.Copy());

        public void Display() => TryCall(() => _olItem.Display());

        public void PrintOut() => TryCall(() => _olItem.PrintOut());

        public void Save() => TryCall(() => _olItem.Save());

        public void SaveAs(string path, Outlook.OlSaveAsType type) => TryCall(() => _olItem.SaveAs(path, type));
        
        public void ShowCategoriesDialog() => TryCall(() => _olItem.ShowCategoriesDialog());

        #endregion Predefined Methods

        #region Private Helper Functions

        internal T TryGet<T>(Func<T> getter) 
        { 
            try
            {
                return getter(); 
            } 
            catch (SystemException)
            {
                return default(T); 
            } 
        }

        internal void TrySet<T>(Action<T> setter, T value)
        {
            try
            {
                setter(value); 
            } 
            catch (SystemException)
            {
            } 
        }
        
        internal void TryCall(System.Action action)
        {
            try
            {
                action(); 
            } 
            catch (SystemException)
            {
            } 
        }

        internal T TryCall<T>(Func<T> func)
        {
            try
            {
                return func(); 
            } 
            catch (SystemException)
            {
                return default(T); 
            } 
        }

        public T GetPropertyValue<T>(string propertyName) => TryGet(() => _olItem.GetPropertyValue<T>(propertyName));

        #endregion
    }
}
