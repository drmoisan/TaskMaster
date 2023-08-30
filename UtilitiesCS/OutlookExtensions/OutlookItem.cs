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
    class OutlookItem
    {
        private object _item;  // the wrapped Outlook item
        private Type _type;  // type for the Outlook item 
        private object[] _args;  // dummy argument array
        private System.Type _typeOlObjectClass;

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

        #region Constructor
        public OutlookItem(object item)
        {
            _item = item;
            _type = _item.GetType();
            _args = new Object[] { };
        }
        #endregion

        #region Public Methods and Properties
        public Outlook.Actions Actions => this.GetPropertyValue(OlActions) as Outlook.Actions;
        
        public Outlook.Application Application => this.GetPropertyValue(OlApplication) as Outlook.Application;

        public Outlook.Attachments Attachments => this.GetPropertyValue(OlAttachments) as Outlook.Attachments;

        public string BillingInformation { get => this.GetPropertyValue(OlBillingInformation).ToString(); set => SetPropertyValue(OlBillingInformation, value); }
        

        public string Body { get => this.GetPropertyValue(OlBody).ToString(); set => SetPropertyValue(OlBody, value); }

        public string Categories { get => this.GetPropertyValue(OlCategories).ToString(); set => SetPropertyValue(OlCategories, value); }

        public void Close(Outlook.OlInspectorClose SaveMode)
        {
            object[] MyArgs = { SaveMode };
            this.CallMethod(OlClose);
        }

        public string Companies { get => this.GetPropertyValue(OlCompanies).ToString(); set => SetPropertyValue(OlCompanies, value); }

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
                return (Outlook.OlObjectClass)System.Enum.ToObject(_typeOlObjectClass, this.GetPropertyValue(OlClass));
            }
        }

        public string ConversationIndex => this.GetPropertyValue(OlConversationIndex).ToString();

        public string ConversationTopic => this.GetPropertyValue(OlConversationTopic).ToString();

        public object Copy() => (this.CallMethod(OlCopy));

        public System.DateTime CreationTime => (System.DateTime)this.GetPropertyValue(OlCreationTime);

        public void Display() => this.CallMethod(OlDisplay);

        public Outlook.OlDownloadState DownloadState => (Outlook.OlDownloadState)this.GetPropertyValue(OlDownloadState);

        public string EntryID => this.GetPropertyValue(OlEntryID).ToString();

        public Outlook.FormDescription FormDescription => (Outlook.FormDescription)this.GetPropertyValue(OlFormDescription);

        public Object InnerObject => this._item;

        public Outlook.Inspector GetInspector => this.GetPropertyValue(OlGetInspector) as Outlook.Inspector;

        public Outlook.OlImportance Importance { get => (Outlook.OlImportance)this.GetPropertyValue(OlImportance); set => SetPropertyValue(OlImportance, value); }

        public bool IsConflict => (bool)this.GetPropertyValue(OlIsConflict);

        public Outlook.ItemProperties ItemProperties => (Outlook.ItemProperties)this.GetPropertyValue(OlItemProperties);

        public System.DateTime LastModificationTime => (System.DateTime)this.GetPropertyValue(OlLastModificationTime);

        public Outlook.Links Links => this.GetPropertyValue(OlLinks) as Outlook.Links;

        public Outlook.OlRemoteStatus MarkForDownload { get => (Outlook.OlRemoteStatus)this.GetPropertyValue(OlMarkForDownload); set => SetPropertyValue(OlMarkForDownload, value); }

        public string MessageClass { get => this.GetPropertyValue(OlMessageClass).ToString(); set => SetPropertyValue(OlMessageClass, value); }

        public string Mileage { get => this.GetPropertyValue(OlMileage).ToString(); set => SetPropertyValue(OlMileage, value); }

        public object Move(Outlook.Folder DestinationFolder)
        {
            object[] myArgs = { DestinationFolder };
            return this.CallMethod(OlMove, myArgs);
        }

        public bool NoAging { get => (bool)this.GetPropertyValue(OlNoAging); set => SetPropertyValue(OlNoAging, value); }

        public long OutlookInternalVersion => (long)this.GetPropertyValue(OlOutlookInternalVersion);

        public string OutlookVersion => this.GetPropertyValue(OlOutlookVersion).ToString();

        public Outlook.Folder Parent => this.GetPropertyValue(OlParent) as Outlook.Folder;

        public Outlook.PropertyAccessor PropertyAccessor => this.GetPropertyValue(OlPropertyAccessor) as Outlook.PropertyAccessor;

        public void PrintOut() => this.CallMethod(OlPrintOut);

        public void Save() => this.CallMethod(OlSave);

        public void SaveAs(string path, Outlook.OlSaveAsType type)
        {
            object[] myArgs = { path, type };
            this.CallMethod(OlSaveAs, myArgs);
        }

        public bool Saved => (bool)this.GetPropertyValue(OlSaved);

        public Outlook.OlSensitivity Sensitivity { get => (Outlook.OlSensitivity)this.GetPropertyValue(OlSensitivity); set => SetPropertyValue(OlSensitivity, value); }

        public Outlook.NameSpace Session => this.GetPropertyValue(OlSession) as Outlook.NameSpace;

        public void ShowCategoriesDialog() => this.CallMethod(OlShowCategoriesDialog);

        public long Size => (long)this.GetPropertyValue(OlSize);

        public string Subject { get => this.GetPropertyValue(OlSubject).ToString(); set => SetPropertyValue(OlSubject, value); }

        public bool UnRead { get => (bool)this.GetPropertyValue(OlUnRead); set => SetPropertyValue(OlUnRead, value); }

        public Outlook.UserProperties UserProperties => this.GetPropertyValue(OlUserProperties) as Outlook.UserProperties;

        #endregion

        #region Private Helper Functions
        
        private object GetPropertyValue(string propertyName)
        {
            try
            {
                // An invalid property name exception is propagated to client
                return _type.InvokeMember(
                    propertyName,
                    BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty,
                    null,
                    _item,
                    _args);
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

        private void SetPropertyValue(string propertyName, object propertyValue)
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

        private object CallMethod(string methodName)
        {
            try
            {
                // An invalid property name exception is propagated to client
                return _type.InvokeMember(
                    methodName,
                    BindingFlags.Public | BindingFlags.InvokeMethod,
                    null,
                    _item,
                    _args);
            }
            catch (SystemException ex)
            {
                Debug.WriteLine(
                    string.Format(
                    "OutlookItem: CallMethod for {0} Exception: {1} ",
                    methodName, ex.Message));
                throw;
            }
        }

        private object CallMethod(string methodName, object[] args)
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
            catch (SystemException ex)
            {
                Debug.WriteLine(
                    string.Format(
                    "OutlookItem: CallMethod for {0} Exception: {1} ",
                    methodName, ex.Message));
                throw;
            }
        }
        
        #endregion

    }
}