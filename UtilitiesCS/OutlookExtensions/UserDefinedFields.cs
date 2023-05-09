using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookExtensions
{
    public static class UserDefinedFields
    {

        /// <summary>
        /// Extension function to determine if a user defined property exists 
        /// on an Outlook item of unknown type.
        /// </summary>
        /// <param name="item">Outlook item</param>
        /// <param name="fieldName">Name of field to check</param>
        /// <returns>true if exists. false if it does not exist</returns>
        public static bool UdfExists(this object item, string fieldName)
        {
            try // Resolve type if supported and call overload. Else throw exception.
            {
                if (item is MailItem) { return ((MailItem)item).UdfExists(fieldName); }
                else if (item is TaskItem) { return ((TaskItem)item).UdfExists(fieldName); }
                else if (item is AppointmentItem) { return ((AppointmentItem)item).UdfExists(fieldName); }
                else if (item is MeetingItem) { return ((MeetingItem)item).UdfExists(fieldName); }
                else
                {
                    throw new ArgumentException("Unsupported type. Extension defined for MailItem, " +
                                                   "TaskItem, AppointmentItem, and MeetingItem. " +
                                                   $"{nameof(item)} is of type {item.GetType().ToString()}",
                                                   nameof(item));
                }
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Exception caught:");
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Extension function to determine if a user defined property exists in the Outlook 
        /// MailItem
        /// </summary>
        /// <param name="item">Outlook.MailItem</param>
        /// <param name="fieldName">Name of field to check</param>
        /// <returns>true if exists. false if it does not exist</returns>
        public static bool UdfExists(this MailItem item, string fieldName) 
        {
            UserProperty objProperty = item.UserProperties.Find(fieldName);
            return objProperty is not null;
        }

        /// <summary>
        /// Extension function to determine if a user defined property exists in the Outlook 
        /// AppointmentItem
        /// </summary>
        /// <param name="item">Outlook.AppointmentItem</param>
        /// <param name="fieldName">Name of field to check</param>
        /// <returns>true if exists. false if it does not exist</returns>
        public static bool UdfExists(this AppointmentItem item, string fieldName) 
        {
            UserProperty objProperty = item.UserProperties.Find(fieldName);
            return objProperty is not null;
        }

        /// <summary>
        /// Extension function to determine if a user defined property exists in the Outlook 
        /// MeetingItem
        /// </summary>
        /// <param name="item">Outlook.MeetingItem</param>
        /// <param name="fieldName">Name of field to check</param>
        /// <returns>true if exists. false if it does not exist</returns>
        public static bool UdfExists(this MeetingItem item, string fieldName)
        {
            UserProperty objProperty = item.UserProperties.Find(fieldName);
            return objProperty is not null;
        }

        /// <summary>
        /// Extension function to determine if a user defined property exists in the Outlook 
        /// TaskItem
        /// </summary>
        /// <param name="item">Outlook.TaskItem</param>
        /// <param name="fieldName">Name of field to check</param>
        /// <returns>true if exists. false if it does not exist</returns>
        public static bool UdfExists(this TaskItem item, string fieldName)
        {
            UserProperty objProperty = item.UserProperties.Find(fieldName);
            return objProperty is not null;
        }

        /// <summary>
        /// Extension function to set a user defined property on an Outlook item of unknown type. 
        /// </summary>
        /// <param name="item">Outlook item. Supported types include MailItem, TaskItem, 
        /// AppointmentItem, and MeetingItem.</param>
        /// <param name="udfName">Name of the user defined field</param>
        /// <param name="value">Value to assign to the user defined field</param>
        /// <param name="olUdfType">Property type as defined by OlUserPropertyType enum</param>
        /// <returns>true if successful. false if unsuccessful</returns>
        public static bool SetUdf(this object item,
                                  string udfName,
                                  object value,
                                  OlUserPropertyType olUdfType = OlUserPropertyType.olText)
        {
            try // Resolve type if supported and call overload. Else throw exception.
            {
                if (item is MailItem) { return ((MailItem)item).SetUdf(udfName, value, olUdfType); }
                else if (item is TaskItem) { return ((TaskItem)item).SetUdf(udfName, value, olUdfType); }
                else if (item is AppointmentItem) { return ((AppointmentItem)item).SetUdf(udfName, value, olUdfType); }
                else if (item is MeetingItem) { return ((MeetingItem)item).SetUdf(udfName, value, olUdfType); }
                else
                {
                    throw new ArgumentException("Unsupported type. Extension defined for MailItem, " +
                                                   "TaskItem, AppointmentItem, and MeetingItem. " +
                                                   $"{nameof(item)} is of type {item.GetType().ToString()}",
                                                   nameof(item));
                }
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Exception caught:");
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Extension function to set a user defined property on an object of type Outlook.MailItem. 
        /// </summary>
        /// <param name="item">Outlook MailItem which will hold the UDF</param>
        /// <param name="udfName">Name of the user defined field</param>
        /// <param name="value">Value to assign to the user defined field</param>
        /// <param name="olUdfType">Property type as defined by OlUserPropertyType enum</param>
        /// <returns>true if successful. false if unsuccessful</returns>
        public static bool SetUdf(this MailItem item,
                                  string udfName,
                                  object value,
                                  OlUserPropertyType olUdfType = OlUserPropertyType.olText)
        {
            try
            {
                if (!ValidPropertyArgs(value, olUdfType)) { return false; }
                UserProperty property = item.UserProperties.Find(udfName);
                if (property is null)
                    property = item.UserProperties.Add(udfName, olUdfType);
                property.Value = value;
                item.Save();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error in set user property: {ex.Message}");
                Debug.WriteLine($"Call Stack: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Extension function to set a user defined property on an object of type Outlook.AppointmentItem. 
        /// </summary>
        /// <param name="item">Outlook AppointmentItem which will hold the UDF</param>
        /// <param name="udfName">Name of the user defined field</param>
        /// <param name="value">Value to assign to the user defined field</param>
        /// <param name="olUdfType">Property type as defined by OlUserPropertyType enum</param>
        /// <returns>true if successful. false if unsuccessful</returns>
        public static bool SetUdf(this AppointmentItem item,
                                  string udfName,
                                  object value,
                                  OlUserPropertyType olUdfType = OlUserPropertyType.olText)
        {
            try
            {
                if (!ValidPropertyArgs(value, olUdfType)) { return false; }
                UserProperty property = item.UserProperties.Find(udfName);
                if (property is null)
                    property = item.UserProperties.Add(udfName, olUdfType);
                property.Value = value;
                item.Save();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error in set user property: {ex.Message}");
                Debug.WriteLine($"Call Stack: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Extension function to set a user defined property on an object of type Outlook.MeetingItem. 
        /// </summary>
        /// <param name="item">Outlook MeetingItem which will hold the UDF</param>
        /// <param name="udfName">Name of the user defined field</param>
        /// <param name="value">Value to assign to the user defined field</param>
        /// <param name="olUdfType">Property type as defined by OlUserPropertyType enum</param>
        /// <returns>true if successful. false if unsuccessful</returns>
        public static bool SetUdf(this MeetingItem item,
                                  string udfName,
                                  object value,
                                  OlUserPropertyType olUdfType = OlUserPropertyType.olText)
        {
            try
            {
                if (!ValidPropertyArgs(value, olUdfType)) { return false; }
                UserProperty property = item.UserProperties.Find(udfName);
                if (property is null)
                    property = item.UserProperties.Add(udfName, olUdfType);
                property.Value = value;
                item.Save();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error in set user property: {ex.Message}");
                Debug.WriteLine($"Call Stack: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Extension function to set a user defined property on an object of type Outlook.TaskItem. 
        /// </summary>
        /// <param name="item">Outlook TaskItem which will hold the UDF</param>
        /// <param name="udfName">Name of the user defined field</param>
        /// <param name="value">Value to assign to the user defined field</param>
        /// <param name="olUdfType">Property type as defined by OlUserPropertyType enum</param>
        /// <returns>true if successful. false if unsuccessful</returns>
        public static bool SetUdf(this TaskItem item,
                                  string udfName,
                                  object value,
                                  OlUserPropertyType olUdfType = OlUserPropertyType.olText)
        {
            try
            {
                if (!ValidPropertyArgs(value, olUdfType)) { return false; }
                UserProperty property = item.UserProperties.Find(udfName);
                if (property is null)
                    property = item.UserProperties.Add(udfName, olUdfType);
                property.Value = value;
                item.Save();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error in set user property: {ex.Message}");
                Debug.WriteLine($"Call Stack: {ex.StackTrace}");
                return false;
            }
        }

        private static Dictionary<OlUserPropertyType, Type> udfTypeLookup = new Dictionary<OlUserPropertyType, Type> 
        {
            {OlUserPropertyType.olText, typeof(string)},
            {OlUserPropertyType.olNumber, typeof(double) },
            {OlUserPropertyType.olDateTime, typeof(DateTime) },
            {OlUserPropertyType.olYesNo, typeof(bool) },
            {OlUserPropertyType.olDuration, typeof(double) },
            {OlUserPropertyType.olKeywords, typeof(string) },
            {OlUserPropertyType.olPercent, typeof(double) },
            {OlUserPropertyType.olCurrency, typeof(decimal) },
            {OlUserPropertyType.olFormula, typeof(string) },
            {OlUserPropertyType.olCombination, typeof(string)},
            {OlUserPropertyType.olInteger, typeof(int) },
            {OlUserPropertyType.olEnumeration, typeof(Enum) }
        };

        internal static bool ValidPropertyArgs(object value, OlUserPropertyType olUdfType)
        {
            Type destinationType = udfTypeLookup[olUdfType];
            Type valueType = value.GetType();
            if (destinationType.IsAssignableFrom(valueType)) { return true; }
            else 
            {
                string msg = $"Argument {nameof(value)} is of type {valueType} " +
                        $"which is not assignable to {olUdfType} which is of type {destinationType.ToString()}";
                Debug.WriteLine(msg);
                return false;
            }
        }
    }
}
