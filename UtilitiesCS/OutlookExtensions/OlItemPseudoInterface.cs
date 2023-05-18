using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS
{
    public static class OlItemPseudoInterface
    {
        public static void SetCategories(this object item, string value)
        {
            if (item is MailItem) { ((MailItem)item).Categories = value; ((MailItem)item).Save(); }
            else if (item is TaskItem) { ((TaskItem)item).Categories = value; ((TaskItem)item).Save(); }
            else if (item is AppointmentItem) { ((AppointmentItem)item).Categories = value; ((AppointmentItem)item).Save(); }
            else if (item is MeetingItem) { ((MeetingItem)item).Categories = value; ((MeetingItem)item).Save(); }
            else { throw new System.ArgumentException(NotSupportedMessage(item)); }
        }

        public static string GetCategories(this object item)
        {
            if (item is MailItem) { return ((MailItem)item).Categories; }
            else if (item is TaskItem) { return ((TaskItem)item).Categories; }
            else if (item is AppointmentItem) { return ((AppointmentItem)item).Categories; }
            else if (item is MeetingItem) { return ((MeetingItem)item).Categories; }
            else { throw new System.ArgumentException(NotSupportedMessage(item)); }
        }

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
                   $"{nameof(item)} is of type {item.GetType().ToString()}";
        }

    }
}
