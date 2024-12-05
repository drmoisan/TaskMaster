using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    /// <summary>
    /// Helper class to access common Outlook item members. 
    /// <see href="https://learn.microsoft.com/en-us/office/client-developer/outlook/pia/how-to-create-a-helper-class-to-access-common-outlook-item-members"/> 
    /// </summary>
    public class OutlookItem : IOutlookItem
    {
        protected object _item;  // the wrapped Outlook item
        protected Type _type;  // type for the Outlook item 
        protected object[] _args;  // dummy argument array
        protected System.Type _typeOlObjectClass;

        #region OutlookItem Constants

        private const string OlActions = "Actions";
        private const string OlApplication = "Application";
        private const string OlAttachments = "Attachments";
        private const string OlBillingInformation = "BillingInformation";
        private const string OlBody = "Body";
        private const string OlCategories = "Categories";
        private const string OlClass = "Class";
        private const string OlClose = "Close";
        private const string OlCompanies = "Companies";
        private const string OlConversationIndex = "ConversationIndex";
        private const string OlConversationTopic = "ConversationTopic";
        private const string OlCopy = "Copy";
        private const string OlCreationTime = "CreationTime";
        private const string OlDisplay = "Display";
        private const string OlDownloadState = "DownloadState";
        private const string OlEntryID = "EntryID";
        private const string OlFormDescription = "FormDescription";
        private const string OlGetInspector = "GetInspector";
        private const string OlImportance = "Importance";
        private const string OlIsConflict = "IsConflict";
        private const string OlItemProperties = "ItemProperties";
        private const string OlLastModificationTime = "LastModificationTime";
        private const string OlLinks = "Links";
        private const string OlMarkForDownload = "MarkForDownload";
        private const string OlMessageClass = "MessageClass";
        private const string OlMileage = "Mileage";
        private const string OlMove = "Move";
        private const string OlNoAging = "NoAging";
        private const string OlOutlookInternalVersion = "OutlookInternalVersion";
        private const string OlOutlookVersion = "OutlookVersion";
        private const string OlParent = "Parent";
        private const string OlPrintOut = "PrintOut";
        private const string OlPropertyAccessor = "PropertyAccessor";
        private const string OlReminderTime = "ReminderTime";
        private const string OlSave = "Save";
        private const string OlSaveAs = "SaveAs";
        private const string OlSaved = "Saved";
        private const string OlSensitivity = "Sensitivity";
        private const string OlSession = "Session";
        private const string OlShowCategoriesDialog = "ShowCategoriesDialog";
        private const string OlSize = "Size";
        private const string OlSubject = "Subject";
        private const string OlUnRead = "UnRead";
        private const string OlUserProperties = "UserProperties";
        #endregion

        #region ctor
        protected OutlookItem() { }

        public OutlookItem(object item)
        {
            _item = item;
            _type = _item?.GetType();
            _args = new Object[] { };
        }
        #endregion ctor

        #region Internal Properties

        internal object Item { get => _item; }
        internal Type ItemType { get => _type; }
        public object[] Args { get => _args; }

        #endregion

        #region Predefined Properties

        public Outlook.Actions Actions => this.GetPropertyValue< Outlook.Actions>(OlActions);

        public Outlook.Application Application => this.GetPropertyValue<Outlook.Application>(OlApplication);

        public Outlook.Attachments Attachments => this.GetPropertyValue<Outlook.Attachments>(OlAttachments);

        public string BillingInformation { get => this.GetPropertyValue<string>(OlBillingInformation); set => SetPropertyValue(OlBillingInformation, value); }

        public string Body { get => this.GetPropertyValue<string>(OlBody); set => SetPropertyValue(OlBody, value); }

        public string Categories { get => this.GetPropertyValue<string>(OlCategories); set => SetPropertyValue(OlCategories, value); }

        public void Close(Outlook.OlInspectorClose SaveMode)
        {
            object[] MyArgs = { SaveMode };
            this.CallMethod(OlClose);
        }

        public string Companies { get => this.GetPropertyValue<string>(OlCompanies); set => SetPropertyValue(OlCompanies, value); }

        public Outlook.OlObjectClass Class
        {
            get
            {
                if (_typeOlObjectClass == null)
                {
                    // Note: instantiate dummy ObjectClass enumeration to get type.
                    //       type = System.Type.GetType("Outlook.OlObjectClass") doesn't seem to work
                    Outlook.OlObjectClass objClass = Outlook.OlObjectClass.olAction;
                    _typeOlObjectClass = objClass.GetType();
                }
                return (Outlook.OlObjectClass)System.Enum.ToObject(_typeOlObjectClass, this.GetPropertyValue<object>(OlClass));
            }
        }

        public string ConversationIndex => this.GetPropertyValue<string>(OlConversationIndex);

        public string ConversationTopic => this.GetPropertyValue<string>(OlConversationTopic);

        public System.DateTime CreationTime => this.GetPropertyValue<System.DateTime>(OlCreationTime);

        public Outlook.OlDownloadState DownloadState => this.GetPropertyValue<Outlook.OlDownloadState>(OlDownloadState);

        public string EntryID => this.GetPropertyValue<string>(OlEntryID);

        public Outlook.FormDescription FormDescription => this.GetPropertyValue<Outlook.FormDescription>(OlFormDescription);

        public Object InnerObject => this._item;

        public Outlook.Inspector Inspector => this.GetPropertyValue<Outlook.Inspector>(OlGetInspector);

        public Outlook.OlImportance Importance { get => this.GetPropertyValue<Outlook.OlImportance>(OlImportance); set => SetPropertyValue(OlImportance, value); }

        public bool IsConflict => this.GetPropertyValue<bool>(OlIsConflict);
        
        public Outlook.ItemProperties ItemProperties => this.GetPropertyValue<Outlook.ItemProperties>(OlItemProperties);

        public System.DateTime LastModificationTime => this.GetPropertyValue<System.DateTime>(OlLastModificationTime);

        public Outlook.Links Links => this.GetPropertyValue<Outlook.Links>(OlLinks);

        public Outlook.OlRemoteStatus MarkForDownload { get => this.GetPropertyValue<Outlook.OlRemoteStatus>(OlMarkForDownload); set => SetPropertyValue(OlMarkForDownload, value); }

        public string MessageClass { get => this.GetPropertyValue<string>(OlMessageClass); set => SetPropertyValue(OlMessageClass, value); }

        public string Mileage { get => this.GetPropertyValue<string>(OlMileage); set => SetPropertyValue(OlMileage, value); }

        public object Move(Outlook.Folder DestinationFolder)
        {
            object[] myArgs = { DestinationFolder };
            return this.CallMethod(OlMove, myArgs);
        }

        public bool NoAging { get => this.GetPropertyValue<bool>(OlNoAging); set => SetPropertyValue(OlNoAging, value); }

        public long OutlookInternalVersion => this.GetPropertyValue<long>(OlOutlookInternalVersion);

        public string OutlookVersion => this.GetPropertyValue<string>(OlOutlookVersion);

        public Outlook.Folder Parent => this.GetPropertyValue<Outlook.Folder>(OlParent);

        public Outlook.PropertyAccessor PropertyAccessor => this.GetPropertyValue<Outlook.PropertyAccessor>(OlPropertyAccessor);

        public DateTime ReminderTime { get => this.GetPropertyValue<DateTime>(OlReminderTime); set => SetPropertyValue(OlReminderTime, value); }

        public bool Saved => this.GetPropertyValue<bool>(OlSaved);

        public Outlook.OlSensitivity Sensitivity { get => this.GetPropertyValue<Outlook.OlSensitivity>(OlSensitivity); set => SetPropertyValue(OlSensitivity, value); }

        public Outlook.NameSpace Session => this.GetPropertyValue<Outlook.NameSpace>(OlSession);

        public long Size => this.GetPropertyValue<long>(OlSize);

        public string Subject { get => this.GetPropertyValue<string>(OlSubject); set => SetPropertyValue(OlSubject, value); }

        public bool UnRead { get => this.GetPropertyValue<bool>(OlUnRead); set => SetPropertyValue(OlUnRead, value); }

        public Outlook.UserProperties UserProperties => this.GetPropertyValue<Outlook.UserProperties>(OlUserProperties);

        #endregion Predefined Properties

        #region Predefined Methods

        public object Copy() => (this.CallMethod(OlCopy));

        public void Display() => this.CallMethod(OlDisplay);

        public void PrintOut() => this.CallMethod(OlPrintOut);

        public void Save() => this.CallMethod(OlSave);

        public void SaveAs(string path, Outlook.OlSaveAsType type)
        {
            object[] myArgs = { path, type };
            this.CallMethod(OlSaveAs, myArgs);
        }

        public void ShowCategoriesDialog() => this.CallMethod(OlShowCategoriesDialog);

        #endregion Predefined Methods

        #region Internal Helper Functions

        internal virtual T GetPropertyValue<T>(string propertyName)
        {
            try
            {
                // An invalid property name exception is propagated to client
                T obj = (T)_type.InvokeMember(
                    propertyName,
                    BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty,
                    null,
                    _item,
                    _args);
                return obj;
            }
            catch (SystemException ex)
            {
                Debug.WriteLine(
                    string.Format(
                    "OutlookItem: GetPropertyValue for {0} Exception: {1} ",
                    propertyName, ex.Message));
                throw;
            }
        }

        internal virtual void SetPropertyValue<T>(string propertyName, T propertyValue)
        {
            try
            {
                _type.InvokeMember(
                    propertyName,
                    BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty,
                    null,
                    _item,
                    new object[] { propertyValue });
                
            }
            catch (SystemException ex)
            {
                Debug.WriteLine(
                   string.Format(
                   "OutlookItem: SetPropertyValue for {0} Exception: {1} ",
                   propertyName, ex.Message));
                throw;
            }
        }

        internal virtual object CallMethod(string methodName)
        {
            try
            {
                // An invalid property name exception is propagated to client
                var obj = _type.InvokeMember(
                    methodName,
                    BindingFlags.Public | BindingFlags.InvokeMethod,
                    null,
                    _item,
                    _args);
                return obj;
            }
            catch (SystemException)
            {
                //Debug.WriteLine(
                //    string.Format(
                //    "OutlookItem: CallMethod for {0} Exception: {1} ",
                //    methodName, ex.Message));
                throw;
            }
        }

        internal virtual object CallMethod(string methodName, object[] args)
        {
            try
            {
                // An invalid property name exception is propagated to client
                return _type.InvokeMember(
                    methodName,
                    BindingFlags.Public | BindingFlags.InvokeMethod,
                    null,
                    _item,
                    args);
            }
            catch (SystemException)
            {
                //Debug.WriteLine(
                //    string.Format(
                //    "OutlookItem: CallMethod for {0} Exception: {1} ",
                //    methodName, ex.Message));
                throw;
            }
        }

        #endregion Internal Helper Functions

    }
}