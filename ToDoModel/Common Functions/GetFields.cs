using System;
using Microsoft.VisualBasic;
using UtilitiesVB;

namespace ToDoModel
{

    public static class GetFields
    {
        public static string CustomFieldID_GetValue(object objItem, string UserDefinedFieldName)
        {
            string CustomFieldID_GetValueRet = default;
            Microsoft.Office.Interop.Outlook.MailItem OlMail;
            Microsoft.Office.Interop.Outlook.TaskItem OlTask;
            Microsoft.Office.Interop.Outlook.AppointmentItem OlAppt;
            Microsoft.Office.Interop.Outlook.UserProperty objProperty;


            if (objItem is Microsoft.Office.Interop.Outlook.MailItem)
            {
                OlMail = (Microsoft.Office.Interop.Outlook.MailItem)objItem;
                objProperty = OlMail.UserProperties.Find(UserDefinedFieldName);
            }

            else if (objItem is Microsoft.Office.Interop.Outlook.TaskItem)
            {
                OlTask = (Microsoft.Office.Interop.Outlook.TaskItem)objItem;
                objProperty = OlTask.UserProperties.Find(UserDefinedFieldName);
            }
            else if (objItem is Microsoft.Office.Interop.Outlook.AppointmentItem)
            {
                OlAppt = (Microsoft.Office.Interop.Outlook.AppointmentItem)objItem;
                objProperty = OlAppt.UserProperties.Find(UserDefinedFieldName);
            }
            else
            {
                objProperty = null;
                var unused = Interaction.MsgBox("Unsupported object type");
            }

            CustomFieldID_GetValueRet = objProperty is null ? "" : objProperty is Array ? FlattenArray.FlattenArry((object[])objProperty) : (string)objProperty;
            return CustomFieldID_GetValueRet;
        }
    }
}