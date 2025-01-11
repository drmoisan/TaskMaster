using ExCSS;
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
    //TODO: Move to OutlookItem rather than as extensions
    public static class OutlookItemExtensions
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static OutlookItemTry Try(this OutlookItem item) => new OutlookItemTry(item);

        public static OutlookItemTryGet TryGet(this OutlookItem item) => new OutlookItemTryGet(item);

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

        internal static PropertyInfo TryGetPropertyInfo(this OutlookItem item, string propertyName)
        {
            try
            {
                return item.ItemType.GetProperty(propertyName);
            }
            catch (SystemException e)
            {
                logger.Info($"{nameof(OutlookItem)}.{nameof(TryGetPropertyInfo)} threw an " +
                    $"exception for property [{propertyName}]. {e.Message}", e);
                return null;
            }
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
            catch (COMException e)
            {
                var propertyInfo = item.TryGetPropertyInfo(propertyName);
                try
                {
                    return propertyInfo?.GetValue(item.InnerObject);
                }
                catch (COMException e2)
                {
                    logger.Debug($"{nameof(OutlookItemExtensions)}.{nameof(TryGetPropertyValue)} threw an " +
                        $"exception for property [{propertyName}]. {e2.Message}", e2);
                    return null;                    
                }
            }

        }

        internal static bool TrySetPropertyValue<T>(this OutlookItem item, string propertyName, string propertyNameAlt, object propertyValue, Func<object, T> converter, Func<object, Table> converterAlt)
        {
            if (TrySetPropertyValue(item, propertyName, converter(propertyValue))) { return true; }
            else { return TrySetPropertyValue(item, propertyNameAlt, converterAlt(propertyValue)); }
        }

        internal static bool TrySetPropertyValue(this OutlookItem item, string propertyName, string propertyNameAlt, object propertyValue, object propertyValueAlt)
        {
            if (TrySetPropertyValue(item, propertyName, propertyValue)) { return true; }
            else { return TrySetPropertyValue(item, propertyNameAlt, propertyValueAlt); }
        }

        internal static bool TrySetPropertyValue(this OutlookItem item, string propertyName, string propertyNameAlt, object propertyValue)
        {
            if (TrySetPropertyValue(item, propertyName, propertyValue)) { return true; }
            else { return TrySetPropertyValue(item, propertyNameAlt, propertyValue); }
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
            catch (COMException e1)
            {
                var propertyInfo = item.TryGetPropertyInfo(propertyName) ?? throw new MissingMemberException(item.ItemType.Name, propertyName);
                try
                {
                    propertyInfo.SetValue(item.InnerObject, propertyValue);
                    return true;
                }
                catch (COMException e)
                {
                    logger.Debug($"{nameof(OutlookItemExtensions)}.{nameof(TrySetPropertyValue)} threw a " +
                        $"COM exception for property [{propertyName}] and value {propertyValue}. \n{e.Message}", e);

                    return false;
                }
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
            catch (SystemException e)
            {
                logger.Debug($"{nameof(OutlookItemExtensions)}.{nameof(TryCallMethod)} threw an " +
                    $"exception for property [{methodName}]. \n{e.Message}", e);

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
            catch (SystemException e)
            {
                logger.Debug($"{nameof(OutlookItemExtensions)}.{nameof(TryCallMethod)} threw an " +
                    $"exception for property [{methodName}]. \n{e.Message}", e);
                return null;
            }
        }

        #endregion

    }


}
