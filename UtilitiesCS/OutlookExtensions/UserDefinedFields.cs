using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace UtilitiesCS.OutlookExtensions
{
    public static class UserDefinedFields
    {
        public static UserProperty GetUdf(this OutlookItem item, string fieldName) => item.UserProperties.Find(fieldName);
        public static UserProperty GetUdf(this MailItem item, string fieldName) => item.UserProperties.Find(fieldName);

        public static string GetUdfString(this MailItem item, string fieldName) => item.GetUdf(fieldName).GetUdfString();
        public static string GetUdfString(this OutlookItem item, string fieldName) => item.GetUdf(fieldName).GetUdfString();
        public static string GetUdfString(this UserProperty property) => (string)property.GetUdfValue(OlUserPropertyType.olText, true);

        public static T GetUdfValue<T>(this UserProperty property, bool flatten = true)
        {
            if ((property is null) || (property.Value is null))
                return default(T);
            var result = property.Value;
            if (flatten && result.IsArray())
            {
                result = (object)result.FlattenArrayTree<T>();
            }
            return (T)result;

        }

        public static object GetUdfValue(this UserProperty property,
                                         OlUserPropertyType olFieldType = OlUserPropertyType.olText,
                                         bool flatten = true)
        {
            if ((property != null) && (property.Value != null))
            {
                var result = property.Value;
                if (result.IsArray()) { result = (object)result.FlattenArrayTree<string>(); }
                return (object)result;
            }
            else
            {
                TypeGroup group = udfGroupLookup[olFieldType];
                switch (group)
                {
                    case TypeGroup.@string:
                        return (object)"";
                    case TypeGroup.numeric:
                        return (object)0;
                    case TypeGroup.@bool:
                        return (object)false;
                    default:
                        return null;
                }
            }
        }

        public static object GetUdfValue(this OutlookItem item,
                                         string fieldName,
                                         OlUserPropertyType olFieldType = OlUserPropertyType.olText,
                                         bool flatten = true)
        {
            UserProperty property = item.GetUdf(fieldName);
            return property.GetUdfValue(olFieldType, flatten);
        }

        public static T GetUdfValue<T>(this OutlookItem item,
                                         string fieldName,
                                         OlUserPropertyType olFieldType = OlUserPropertyType.olText,
                                         bool flatten = true)
        {
            UserProperty property = item.GetUdf(fieldName);
            return property.GetUdfValue<T>(flatten);
        }

        /// <summary>
        /// <seealso cref="PropertyAccessor"/> extension to return a typed value of an extended Outlook Property accessed 
        /// with schema string. Null is returned if the property does not exist or is not of the specified type.
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="schema"></param>
        /// <returns>Value from Field or null</returns>
        public static object TryGetProperty(this PropertyAccessor accessor, string schema)
        {
            try
            {
                return accessor.GetProperty(schema);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Generic <seealso cref="PropertyAccessor"/> extension to return a typed value of an extended Outlook Property accessed 
        /// with schema string. Default value is returned if the property does not exist or is not of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="accessor"></param>
        /// <param name="schema">Schema of the property to access</param>
        /// <returns>Typed value from the field or default value if the property does not exist or is not of the specified type</returns>
        public static T TryGetProperty<T>(this PropertyAccessor accessor, string schema)
        {
            try
            {
                return (T)accessor.GetProperty(schema);
            }
            catch (System.Exception)
            {
                return default(T);
            }
        }

        public static bool Exists(this PropertyAccessor accessor, string schema)
        {
            try
            {
                object value = accessor.GetProperty(schema);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static bool TrySetProperty<T>(this PropertyAccessor accessor, string schema, T value)
        {
            try
            {
                accessor.SetProperty(schema, value);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Extension function to determine if a user defined property exists in the OutlookItem
        /// </summary>
        /// <param name="item">Outlook.MailItem</param>
        /// <param name="fieldName">Name of field to check</param>
        /// <returns>true if exists. false if it does not exist</returns>
        public static bool UdfExists(this OutlookItem item, string fieldName)
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
        public static bool SetUdf(this OutlookItem item,
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

        private enum TypeGroup
        {
            @numeric = 1,
            @string = 2,
            @bool = 4,
            other = 8
        }

        private static Dictionary<OlUserPropertyType, TypeGroup> udfGroupLookup = new Dictionary<OlUserPropertyType, TypeGroup>
        {
            {OlUserPropertyType.olText, TypeGroup.@string},
            {OlUserPropertyType.olNumber, TypeGroup.numeric },
            {OlUserPropertyType.olDateTime, TypeGroup.other },
            {OlUserPropertyType.olYesNo, TypeGroup.@bool },
            {OlUserPropertyType.olDuration, TypeGroup.numeric },
            {OlUserPropertyType.olKeywords, TypeGroup.@string },
            {OlUserPropertyType.olPercent, TypeGroup.numeric  },
            {OlUserPropertyType.olCurrency, TypeGroup.numeric  },
            {OlUserPropertyType.olFormula, TypeGroup.@string },
            {OlUserPropertyType.olCombination, TypeGroup.@string},
            {OlUserPropertyType.olInteger, TypeGroup.numeric  },
            {OlUserPropertyType.olEnumeration, TypeGroup.other }
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

        [Obsolete()]
        private static void EnsureSupported(this object item)
        {
            if (!((item is MailItem) || (item is MeetingItem) || (item is AppointmentItem) || (item is TaskItem)))
            {
                throw new ArgumentException(NotSupportedMessage(item), nameof(item));
            }
        }

        private static string NotSupportedMessage(object item)
        {
            return "Unsupported type. Extension defined for MailItem, " +
                   "TaskItem, AppointmentItem, and MeetingItem. " +
                   $"{nameof(item)} is of type {item.GetType().Name}";
        }

        #region Deprecated UDF Functions That Don't Extend OutlookItem Class

        ///// <summary>
        ///// Extension function to determine if a user defined property exists 
        ///// on an Outlook item of unknown type.
        ///// </summary>
        ///// <param name="item">Outlook item</param>
        ///// <param name="fieldName">Name of field to check</param>
        ///// <returns>true if exists. false if it does not exist</returns>
        //public static bool UdfExists(this object item, string fieldName)
        //{
        //    try // Resolve type if supported and call overload. Else throw exception.
        //    {
        //        if (item is MailItem) { return ((MailItem)item).UdfExists(fieldName); }
        //        else if (item is TaskItem) { return ((TaskItem)item).UdfExists(fieldName); }
        //        else if (item is AppointmentItem) { return ((AppointmentItem)item).UdfExists(fieldName); }
        //        else if (item is MeetingItem) { return ((MeetingItem)item).UdfExists(fieldName); }
        //        else
        //        {
        //            throw new ArgumentException("Unsupported type. Extension defined for MailItem, " +
        //                                           "TaskItem, AppointmentItem, and MeetingItem. " +
        //                                           $"{nameof(item)} is of type {item.GetType().ToString()}",
        //                                           nameof(item));
        //        }
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //        Debug.WriteLine(ex.StackTrace);
        //        return false;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Debug.WriteLine($"Exception caught:");
        //        Debug.WriteLine(ex.ToString());
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Extension function to determine if a user defined property exists in the Outlook 
        ///// MailItem
        ///// </summary>
        ///// <param name="item">Outlook.MailItem</param>
        ///// <param name="fieldName">Name of field to check</param>
        ///// <returns>true if exists. false if it does not exist</returns>
        //public static bool UdfExists(this MailItem item, string fieldName) 
        //{
        //    UserProperty objProperty = item.UserProperties.Find(fieldName);
        //    return objProperty is not null;
        //}

        ///// <summary>
        ///// Extension function to determine if a user defined property exists in the Outlook 
        ///// AppointmentItem
        ///// </summary>
        ///// <param name="item">Outlook.AppointmentItem</param>
        ///// <param name="fieldName">Name of field to check</param>
        ///// <returns>true if exists. false if it does not exist</returns>
        //public static bool UdfExists(this AppointmentItem item, string fieldName) 
        //{
        //    UserProperty objProperty = item.UserProperties.Find(fieldName);
        //    return objProperty is not null;
        //}

        ///// <summary>
        ///// Extension function to determine if a user defined property exists in the Outlook 
        ///// MeetingItem
        ///// </summary>
        ///// <param name="item">Outlook.MeetingItem</param>
        ///// <param name="fieldName">Name of field to check</param>
        ///// <returns>true if exists. false if it does not exist</returns>
        //public static bool UdfExists(this MeetingItem item, string fieldName)
        //{
        //    UserProperty objProperty = item.UserProperties.Find(fieldName);
        //    return objProperty is not null;
        //}

        ///// <summary>
        ///// Extension function to determine if a user defined property exists in the Outlook 
        ///// TaskItem
        ///// </summary>
        ///// <param name="item">Outlook.TaskItem</param>
        ///// <param name="fieldName">Name of field to check</param>
        ///// <returns>true if exists. false if it does not exist</returns>
        //public static bool UdfExists(this TaskItem item, string fieldName)
        //{
        //    UserProperty objProperty = item.UserProperties.Find(fieldName);
        //    return objProperty is not null;
        //}

        [Obsolete("Use GetUdf with OutlookItem instead")]
        public static UserProperty GetUdf(this object item,
                                          string fieldName)
        {
            if (item is MailItem) { return ((MailItem)item).UserProperties.Find(fieldName); }
            else if (item is TaskItem) { return ((TaskItem)item).UserProperties.Find(fieldName); }
            else if (item is AppointmentItem) { return ((AppointmentItem)item).UserProperties.Find(fieldName); }
            else if (item is MeetingItem) { return ((MeetingItem)item).UserProperties.Find(fieldName); }
            else
            {
                throw new ArgumentException("Unsupported type. Extension defined for MailItem, " +
                                               "TaskItem, AppointmentItem, and MeetingItem. " +
                                               $"{nameof(item)} is of type {item.GetType().Name}",
                                               nameof(item));
            }
        }

        [Obsolete("Use GetUdfString with OutlookItem instead")]
        public static string GetUdfString(this object item, string fieldName)
        {
            UserProperty property = item.GetUdf(fieldName);
            return property.GetUdfString();
        }

        [Obsolete("Use GetUdfValue with OutlookItem instead")]
        public static object GetUdfValue(this object item,
                                         string fieldName,
                                         OlUserPropertyType olFieldType = OlUserPropertyType.olText,
                                         bool flatten = true)
        {
            UserProperty property = item.GetUdf(fieldName);
            return property.GetUdfValue(olFieldType, flatten);
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
        [Obsolete("Use SetUdf with OutlookItem instead")]
        public static bool SetUdf(this object item,
                                  string udfName,
                                  object value,
                                  OlUserPropertyType olUdfType = OlUserPropertyType.olText)
        {
            var olItem = new OutlookItem(item);
            return olItem.SetUdf(udfName, value, olUdfType);
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
        [Obsolete("Use SetUdf with OutlookItem instead")]
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
        [Obsolete("Use SetUdf with OutlookItem instead")]
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
        [Obsolete("Use SetUdf with OutlookItem instead")]
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

        #endregion
    }
}
