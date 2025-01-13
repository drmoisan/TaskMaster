using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Outlook = Microsoft.Office.Interop.Outlook;
using UtilitiesCS.Extensions;

namespace UtilitiesCS
{
    /// <summary>
    /// Helper class to access common Outlook item members. 
    /// <see href="https://learn.microsoft.com/en-us/office/client-developer/outlook/pia/how-to-create-a-helper-class-to-access-common-outlook-item-members"/> 
    /// </summary>
    public class OutlookItem : IOutlookItem
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected object _item;  // the wrapped Outlook item
        protected Type _type;  // type for the Outlook item 
        protected object[] _args;  // dummy argument array
        protected System.Type _typeOlObjectClass;

        #region OutlookItem Constants

        private const string olActions = "Actions";
        private const string olApplication = "Application";
        private const string olAttachments = "Attachments";
        private const string olBillingInformation = "BillingInformation";
        private const string olBody = "Body";
        private const string olCategories = "Categories";
        private const string olClass = "Class";
        private const string olClose = "Close";
        private const string olCompanies = "Companies";
        private const string olConversationIndex = "ConversationIndex";
        private const string olConversationTopic = "ConversationTopic";
        private const string olCopy = "Copy";
        private const string olCreationTime = "CreationTime";
        private const string olDisplay = "Display";
        private const string olDownloadState = "DownloadState";
        private const string olEntryID = "EntryID";
        private const string olFormDescription = "FormDescription";
        private const string olGetInspector = "GetInspector";
        private const string olImportance = "Importance";
        private const string olIsConflict = "IsConflict";
        private const string olItemProperties = "ItemProperties";
        private const string olLastModificationTime = "LastModificationTime";
        private const string olLinks = "Links";
        private const string olMarkForDownload = "MarkForDownload";
        private const string olMessageClass = "MessageClass";
        private const string olMileage = "Mileage";
        private const string olMove = "Move";
        private const string olNoAging = "NoAging";
        private const string olOutlookInternalVersion = "OutlookInternalVersion";
        private const string olOutlookVersion = "OutlookVersion";
        private const string olParent = "Parent";
        private const string olPrintOut = "PrintOut";
        private const string olPropertyAccessor = "PropertyAccessor";
        private const string olReminderTime = "ReminderTime";
        private const string olSave = "Save";
        private const string olSaveAs = "SaveAs";
        private const string olSaved = "Saved";
        private const string olSensitivity = "Sensitivity";
        private const string olSession = "Session";
        private const string olShowCategoriesDialog = "ShowCategoriesDialog";
        private const string olSize = "Size";
        private const string olSubject = "Subject";
        private const string olTaskStartDate = "TaskStartDate";
        private const string olUnRead = "UnRead";
        private const string olUserProperties = "UserProperties";
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

        public Outlook.Actions Actions => this.GetPropertyValue< Outlook.Actions>(olActions);

        public Outlook.Application Application => this.GetPropertyValue<Outlook.Application>(olApplication);

        public Outlook.Attachments Attachments => this.GetPropertyValue<Outlook.Attachments>(olAttachments);

        public string BillingInformation { get => this.GetPropertyValue<string>(olBillingInformation); set => SetPropertyValue(olBillingInformation, value); }

        public string Body { get => this.GetPropertyValue<string>(olBody); set => SetPropertyValue(olBody, value); }

        public string Categories { get => this.GetPropertyValue<string>(olCategories); set => SetPropertyValue(olCategories, value); }

        public void Close(Outlook.OlInspectorClose SaveMode)
        {
            object[] MyArgs = { SaveMode };
            this.CallMethod(olClose);
        }

        public string Companies { get => this.GetPropertyValue<string>(olCompanies); set => SetPropertyValue(olCompanies, value); }

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
                return (Outlook.OlObjectClass)System.Enum.ToObject(_typeOlObjectClass, this.GetPropertyValue<object>(olClass));
            }
        }

        public string ConversationIndex => this.GetPropertyValue<string>(olConversationIndex);

        public string ConversationTopic => this.GetPropertyValue<string>(olConversationTopic);

        public System.DateTime CreationTime => this.GetPropertyValue<System.DateTime>(olCreationTime);

        public Outlook.OlDownloadState DownloadState => this.GetPropertyValue<Outlook.OlDownloadState>(olDownloadState);

        public string EntryID => this.GetPropertyValue<string>(olEntryID);

        public Outlook.FormDescription FormDescription => this.GetPropertyValue<Outlook.FormDescription>(olFormDescription);

        public object InnerObject => this._item;

        public Outlook.Inspector Inspector => this.GetPropertyValue<Outlook.Inspector>(olGetInspector);

        public Outlook.OlImportance Importance { get => this.GetPropertyValue<Outlook.OlImportance>(olImportance); set => SetPropertyValue(olImportance, value); }

        public bool IsConflict => this.GetPropertyValue<bool>(olIsConflict);
        
        public Outlook.ItemProperties ItemProperties => this.GetPropertyValue<Outlook.ItemProperties>(olItemProperties);

        public System.DateTime LastModificationTime => this.GetPropertyValue<System.DateTime>(olLastModificationTime);

        public Outlook.Links Links => this.GetPropertyValue<Outlook.Links>(olLinks);

        public Outlook.OlRemoteStatus MarkForDownload { get => this.GetPropertyValue<Outlook.OlRemoteStatus>(olMarkForDownload); set => SetPropertyValue(olMarkForDownload, value); }

        public string MessageClass { get => this.GetPropertyValue<string>(olMessageClass); set => SetPropertyValue(olMessageClass, value); }

        public string Mileage { get => this.GetPropertyValue<string>(olMileage); set => SetPropertyValue(olMileage, value); }

        public object Move(Outlook.Folder DestinationFolder)
        {
            object[] myArgs = { DestinationFolder };
            return this.CallMethod(olMove, myArgs);
        }

        public bool NoAging { get => this.GetPropertyValue<bool>(olNoAging); set => SetPropertyValue(olNoAging, value); }

        public long OutlookInternalVersion => this.GetPropertyValue<long>(olOutlookInternalVersion);

        public string OutlookVersion => this.GetPropertyValue<string>(olOutlookVersion);

        public Outlook.Folder Parent => this.GetPropertyValue<Outlook.Folder>(olParent);

        public Outlook.PropertyAccessor PropertyAccessor => this.GetPropertyValue<Outlook.PropertyAccessor>(olPropertyAccessor);

        public DateTime ReminderTime { get => this.GetPropertyValue<DateTime>(olReminderTime); set => SetPropertyValue(olReminderTime, value); }

        public bool Saved => this.GetPropertyValue<bool>(olSaved);

        public Outlook.OlSensitivity Sensitivity { get => this.GetPropertyValue<Outlook.OlSensitivity>(olSensitivity); set => SetPropertyValue(olSensitivity, value); }

        public Outlook.NameSpace Session => this.GetPropertyValue<Outlook.NameSpace>(olSession);

        public long Size => this.GetPropertyValue<long>(olSize);

        public string Subject { get => this.GetPropertyValue<string>(olSubject); set => SetPropertyValue(olSubject, value); }

        public DateTime TaskStartDate => this.GetPropertyValueIfExists<DateTime>(olTaskStartDate);

        public bool UnRead { get => this.GetPropertyValue<bool>(olUnRead); set => SetPropertyValue(olUnRead, value); }

        public Outlook.UserProperties UserProperties => this.GetPropertyValue<Outlook.UserProperties>(olUserProperties);

        #endregion Predefined Properties

        #region Predefined Methods

        public object Copy() => (this.CallMethod(olCopy));

        public void Display() => this.CallMethod(olDisplay);

        public void PrintOut() => this.CallMethod(olPrintOut);

        public void Save() => this.CallMethod(olSave);

        public void SaveAs(string path, Outlook.OlSaveAsType type)
        {
            object[] myArgs = { path, type };
            this.CallMethod(olSaveAs, myArgs);
        }

        public void ShowCategoriesDialog() => this.CallMethod(olShowCategoriesDialog);

        #endregion Predefined Methods

        #region Internal Helper Functions

        internal virtual T GetPropertyValueIfExists<T>(string propertyName)
        {
            var propertyInfo = TryGetPropertyInfo(propertyName);
            try
            {
               return (T)(propertyInfo?.GetValue(_item) ?? default(T));
            }
            catch (SystemException e)
            {
                logger.Info($"{nameof(OutlookItem)}.{nameof(GetPropertyValueIfExists)}<{nameof(T)}> threw an " +
                    $"exception for property [{propertyName}]. {e.Message}", e);
                return default(T);
            }
        }

        internal virtual PropertyInfo TryGetPropertyInfo(string propertyName)
        {
            try
            {
                return _type.GetProperty(propertyName);
            }
            catch (SystemException e)
            {
                logger.Debug($"{nameof(OutlookItem)}.{nameof(TryGetPropertyInfo)} threw an " +
                    $"exception for property [{propertyName}] of item type [{ItemType.Name}]. {e.Message}", e);
                return null;
            }
        }

        internal virtual T GetPropertyValue<T>(string propertyName)
        {
            try
            {
                return (T)ItemType.InvokeMember(
                    propertyName,
                    BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty,
                    null,
                    InnerObject,
                    Args);
                
            }
            catch (Exception)
            {
                var propertyInfo = TryGetPropertyInfo(propertyName) ?? throw new MissingMemberException(ItemType.Name, propertyName);
                try
                {
                    var value = propertyInfo.GetValue(_item);
                    if (value is null) { return default(T); }
                    else
                    {
                        var typedValue = (T)value;
                        return typedValue;
                    }
                }
                catch (SystemException e)
                {
                    // An invalid property name exception is propagated to client
                    logger.Error($"{nameof(OutlookItem)}.{nameof(GetPropertyValue)}<{nameof(T)}> threw an " +
                        $"exception for property [{propertyName}]. {e.Message}", e);
                    throw;
                }
            }
            
        }

        internal virtual void SetPropertyValue<T>(string propertyName, T propertyValue)
        {
            try
            {
                ItemType.InvokeMember(
                    propertyName,
                    BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty,
                    null,
                    InnerObject,
                    [propertyValue]);
            }
            catch (Exception)
            {
                var propertyInfo = TryGetPropertyInfo(propertyName) ?? throw new MissingMemberException(ItemType.Name, propertyName);
                try
                {
                    propertyInfo.SetValue(_item, propertyValue);
                }
                catch (SystemException ex)
                {
                    logger.Error($"{nameof(OutlookItem)}.{nameof(SetPropertyValue)}<{nameof(T)}> threw an " +
                        $"exception for property [{propertyName}]. {ex.Message}", ex);
                    throw;
                }
            }                        
        }

        internal virtual object CallMethod(string methodName)
        {
            try
            {
                var methodInfo = _type.GetMethod(methodName).ThrowIfNull();
                return methodInfo.Invoke(_item, _args);
                //var obj = _type.InvokeMember(
                //    methodName,
                //    BindingFlags.Public | BindingFlags.InvokeMethod,
                //    null,
                //    _item,
                //    _args);
                //return obj;
            }
            catch (SystemException)
            {
                // An invalid property name exception is propagated to client
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