using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookExtensions
{
    public static class OutlookItemExtensions
    {
        public static OutlookItemTry Try(this OutlookItem item) => new OutlookItemTry(item);
        
        public static OlItemType GetOlItemType(this OutlookItem item)
        {
            if (item.InnerObject is AppointmentItem) { return OlItemType.olAppointmentItem; }
            else if (item.InnerObject is ContactItem) { return OlItemType.olContactItem; }
            else if (item.InnerObject is DistListItem) { return OlItemType.olDistributionListItem; }
            else if (item.InnerObject is JournalItem) { return OlItemType.olJournalItem; }
            else if (item.InnerObject is MailItem) { return OlItemType.olMailItem; }
            else if (item.InnerObject is MobileItem) { return OlItemType.olMobileItemMMS; }
            else if (item.InnerObject is NoteItem) { return OlItemType.olNoteItem; }
            else if (item.InnerObject is PostItem) { return OlItemType.olPostItem; }
            else if (item.InnerObject is TaskItem) { return OlItemType.olTaskItem; }
            else { throw new ArgumentException($"{item.InnerObject.GetType().Name} is not a supported type for {nameof(OlItemType)} class."); }
        }


        


        #region Helper Methods

        internal static T TryGet<T>(Func<T> getter)
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

        internal static void TrySet<T>(Action<T> setter, T value)
        {
            try
            {
                setter(value);
            }
            catch (SystemException)
            {
            }
        }

        internal static void TryCall(System.Action action)
        {
            try
            {
                action();
            }
            catch (SystemException)
            {
            }
        }

        internal static T TryCall<T>(Func<T> func)
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

        internal static object TryGetPropertyValue<T>(this OutlookItem item, string propertyName, string propertyNameAlt, Func<object, T> converter, Func<object, T> converterAlt) 
        {
            var value = TryGetPropertyValue(item, propertyName);
            if (value is not null) 
            {
                if (converter is null) { return value; }
                else { return converter(value); }
            } 
            value = TryGetPropertyValue(item, propertyNameAlt);
            if (value is not null) 
            {
                if (converterAlt is null) { return value; }
                else { return converterAlt(value); }
            }
            else { return null; }            
        }
        
        internal static object TryGetPropertyValue(this OutlookItem item, string propertyName, string propertyNameAlt) => TryGetPropertyValue(item, propertyName) ?? TryGetPropertyValue(item, propertyNameAlt);

        internal static object TryGetPropertyValue(this OutlookItem item, string propertyName)
        {
            try
            {
                return item.ItemType.InvokeMember(
                    propertyName,
                    BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty,
                    null,
                    item.InnerObject,
                    item.Args);
            }
            catch (COMException ex)
            {
                Debug.WriteLine(
                    string.Format(
                    "OutlookItem: GetPropertyValue for {0} Exception: {1} ",
                    propertyName, ex.Message));
                return null;
            }
        }

        internal static bool TrySetPropertyValue<T>(this OutlookItem item, string propertyName, string propertyNameAlt, object propertyValue, Func<object, T> converter, Func<object, Table> converterAlt)
        {
            if (item.TrySetPropertyValue(propertyName, converter(propertyValue))) { return true; }
            else { return item.TrySetPropertyValue(propertyNameAlt, converterAlt(propertyValue)); }
        }

        internal static bool TrySetPropertyValue(this OutlookItem item, string propertyName, string propertyNameAlt, object propertyValue, object propertyValueAlt)
        {
            if (item.TrySetPropertyValue(propertyName, propertyValue)) { return true; }
            else { return item.TrySetPropertyValue(propertyNameAlt, propertyValueAlt); }
        }

        internal static bool TrySetPropertyValue(this OutlookItem item, string propertyName, string propertyNameAlt, object propertyValue)
        {
            if (item.TrySetPropertyValue(propertyName, propertyValue)) { return true; }
            else { return item.TrySetPropertyValue(propertyNameAlt, propertyValue); }
        }

        internal static bool TrySetPropertyValue(this OutlookItem item, string propertyName, object propertyValue)
        {
            try
            {
                item.ItemType.InvokeMember(
                    propertyName,
                    BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty,
                    null,
                    item.InnerObject,
                    new object[] { propertyValue });
                return true;
            }
            catch (COMException)
            {
                //Debug.WriteLine(
                //   string.Format(
                //   "OutlookItem: SetPropertyValue for {0} Exception: {1} ",
                //   propertyName, ex.Message));
                return false;
            }
        }

        internal static object TryCallMethod(this OutlookItem item, string methodName)
        {
            try
            {
                // An invalid property name exception is propagated to client
                return item.ItemType.InvokeMember(
                    methodName,
                    BindingFlags.Public | BindingFlags.InvokeMethod,
                    null,
                    item.InnerObject,
                    item.Args);
            }
            catch (SystemException ex)
            {
                Debug.WriteLine(
                    string.Format(
                    "OutlookItem: CallMethod for {0} Exception: {1} ",
                    methodName, ex.Message));
                return null;
            }
        }

        internal static object TryCallMethod(this OutlookItem item, string methodName, object[] args)
        {
            try
            {
                // An invalid property name exception is propagated to client
                return item.ItemType.InvokeMember(
                    methodName,
                    BindingFlags.Public | BindingFlags.InvokeMethod,
                    null,
                    item.InnerObject,
                    args);
            }
            catch (SystemException ex)
            {
                Debug.WriteLine(
                    string.Format(
                    "OutlookItem: CallMethod for {0} Exception: {1} ",
                    methodName, ex.Message));
                return null;
            }
        }

        #endregion

    }


}
