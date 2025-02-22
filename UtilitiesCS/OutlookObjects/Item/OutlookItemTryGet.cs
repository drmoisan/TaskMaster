using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace UtilitiesCS.OutlookExtensions
{
    public class OutlookItemTryGet
    {
        private OutlookItem _olItem;
        
        public OutlookItemTryGet(OutlookItem olItem)
        {
            _olItem = olItem;
        }

        #region Predefined Properties

        public bool Actions(out Actions result) => TryGet(() => _olItem.Actions, out result);

        public bool Application(out Outlook.Application result) => TryGet(() => _olItem.Application, out result);

        public bool Attachments(out Outlook.Attachments result) => TryGet(() => _olItem.Attachments, out result);

        public bool BillingInformation(out string result) => TryGet(() => _olItem.BillingInformation, out result);

        public bool Body(out string result) => TryGet(() => _olItem.Body, out result);

        public bool Categories(out string result) => TryGet(() => _olItem.Categories, out result);

        public bool Companies(out string result) => TryGet(() => _olItem.Companies, out result); 

        public bool OlObjectClass(out Outlook.OlObjectClass result) => TryGet(() => _olItem.Class, out result);

        public bool ConversationIndex(out string result) => TryGet(() => _olItem.ConversationIndex, out result);

        public bool ConversationTopic(out string result) => TryGet(() => _olItem.ConversationTopic, out result);

        public bool CreationTime(out System.DateTime result) => TryGet(() => _olItem.CreationTime, out result);

        public bool DownloadState(out Outlook.OlDownloadState result) => TryGet(() => _olItem.DownloadState, out result);

        public bool EntryID(out string result) => TryGet(() => _olItem.EntryID, out result);

        public bool FormDescription(out Outlook.FormDescription result) => TryGet(() => _olItem.FormDescription, out result);

        public bool InnerObject(out object result) => TryGet(() => _olItem.InnerObject, out result);

        public bool GetInspector(out Outlook.Inspector result) => TryGet(() => _olItem.Inspector, out result);

        public bool Importance(out Outlook.OlImportance result) => TryGet(() => _olItem.Importance, out result);

        public bool IsConflict(out bool result) => TryGet(() => _olItem.IsConflict, out result);

        public bool ItemProperties(out Outlook.ItemProperties result) => TryGet(() => _olItem.ItemProperties, out result);

        public bool LastModificationTime(out System.DateTime result) => TryGet(() => _olItem.LastModificationTime, out result);

        public bool Links(out Outlook.Links result) => TryGet(() => _olItem.Links, out result);

        public bool MarkForDownload(out Outlook.OlRemoteStatus result) => TryGet(() => _olItem.MarkForDownload, out result);

        public bool MessageClass(out string result) => TryGet(() => _olItem.MessageClass, out result); 

        public bool Mileage(out string result) => TryGet(() => _olItem.Mileage, out result);
                
        public bool OlItemType(out OlItemType result) => TryGet(_olItem.GetOlItemType, out result);

        public bool OutlookInternalVersion(out long result) => TryGet(() => _olItem.OutlookInternalVersion, out result);

        public bool OutlookVersion(out string result) => TryGet(() => _olItem.OutlookVersion, out result);

        public bool Parent(out Outlook.Folder result) => TryGet(() => _olItem.Parent, out result);

        public bool PropertyAccessor(out Outlook.PropertyAccessor result) => TryGet(() => _olItem.PropertyAccessor, out result);

        public bool Saved(out bool result) => TryGet(() => _olItem.Saved, out result);

        public bool Sensitivity(out Outlook.OlSensitivity result) => TryGet(() => _olItem.Sensitivity, out result);

        public bool Session(out Outlook.NameSpace result) => TryGet(() => _olItem.Session, out result);

        public bool Size(out long result) => TryGet(() => _olItem.Size, out result);

        public bool Subject(out string result) => TryGet(() => _olItem.Subject, out result);

        public bool UnRead(out bool result) => TryGet(() => _olItem.UnRead, out result);

        public bool UserProperties(out Outlook.UserProperties result) => TryGet(() => _olItem.UserProperties, out result);

        #endregion Predefined Properties

        #region Private Helper Functions

        
        
        internal bool TryGet<T>(Func<T> getter, out T result)
        {
            try
            {
                result = getter();
                return true;
            }
            catch (SystemException)
            {
                result = default;
                return false;
            }
        }

        internal bool TrySet<T>(Action<T> setter, T value)
        {
            try
            {
                setter(value);
                return true;
            }
            catch (SystemException)
            {
                return false;
            }
        }

        internal bool TryCall(System.Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (SystemException)
            {
                return false;
            }
        }

        internal bool TryCall<T>(Func<T> func, out T result)
        {
            try
            {
                result= func();
                return true;
            }
            catch (SystemException)
            {
                result = default;
                return false;
            }
        }

        #endregion

    }
}
