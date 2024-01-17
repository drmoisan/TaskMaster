using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("UtilitiesCS.Test")]
namespace UtilitiesCS
{
    public static class OlItemSummary
    {
        [Flags]
        public enum Details
        {
            None = 0,
            Type = 1,
            Subject = 2,
            Date = 4,
            All = 7,
            Folderpath = 8
        }
        public static string Extract(object Item, Details Flags) 
        {
            if (Item is AppointmentItem) { return ExtractSummary((AppointmentItem)Item).ToString(Flags); }
            else if (Item is MailItem) { return ExtractSummary((MailItem)Item).ToString(Flags); }
            else if (Item is MeetingItem) { return ExtractSummary((MeetingItem)Item).ToString(Flags); }
            else if (Item is TaskRequestItem) { return ExtractSummary((TaskRequestItem)Item).ToString(Flags); }
            else if (Item is TaskRequestUpdateItem) { return ExtractSummary((TaskRequestUpdateItem)Item).ToString(Flags); }
            else { return $"Details.Type: {Item.GetType().ToString()}"; }
        }

        public static string ToString(this Dictionary<Details, string> Dict, Details Flags)
        {
            return String.Join(", ",Dict.Where(x => Flags.HasFlag(x.Key)).Select(x => $"{x.Key}: {x.Value}"));
        }

        public static Dictionary<Details, string> ExtractSummary(MailItem item)
        {
            MailItem OlMail = MailResolution.TryResolveMailItem(item);
            if (OlMail == null)
            {
                return new Dictionary<Details, string>()
                {
                    { Details.Type, typeof(MailItem).ToString() },
                    { Details.Subject, item.MessageClass.ToString() } 
                };
            }

            return new Dictionary<Details, string>()
            {
                { Details.Type, typeof(MailItem).ToString() },
                { Details.Subject, OlMail.Subject },
                { Details.Date, OlMail.SentOn.ToString("MM-dd-yyyy h:mm tt") },
                { Details.Folderpath, ((MAPIFolder)OlMail.Parent).FolderPath}
            };
            //return $"Type: {item.GetType()} Subject: {item.Subject} StartsOn: {item.Start.ToString("MM-dd-yyyy h:mm t")}";
        }

        public static Dictionary<Details, string> ExtractSummary(object Item)
        {
            if (Item is AppointmentItem) { return ExtractSummary((AppointmentItem)Item); }
            else if (Item is MailItem) { return ExtractSummary((MailItem)Item); }
            else if (Item is MeetingItem) { return ExtractSummary((MeetingItem)Item); }
            else if (Item is TaskRequestItem) { return ExtractSummary((TaskRequestItem)Item); }
            else if (Item is TaskRequestUpdateItem) { return ExtractSummary((TaskRequestUpdateItem)Item); }
            else { throw new ArgumentException($"{Item.GetType().ToString()} is an unsupported type"); }
            //return $"Type: {item.GetType()} Subject: {item.Subject} StartsOn: {item.Start.ToString("MM-dd-yyyy h:mm t")}";
        }

        public static Dictionary<Details, string> ExtractSummary(AppointmentItem item)
        {
            return new Dictionary<Details, string>()
            {
                { Details.Type, typeof(AppointmentItem).ToString() },
                { Details.Subject, item.Subject },
                { Details.Date, item.Start.ToString("MM-dd-yyyy h:mm tt") },
                { Details.Folderpath, ((MAPIFolder) item.Parent).FolderPath}
            };
            //return $"Type: {item.GetType()} Subject: {item.Subject} StartsOn: {item.Start.ToString("MM-dd-yyyy h:mm t")}";
        }

        public static Dictionary<Details, string> ExtractSummary(MeetingItem item)
        {
            return new Dictionary<Details, string>()
            {
                { Details.Type, typeof(MeetingItem).ToString() },
                { Details.Subject, item.Subject },
                { Details.Date, item.SentOn.ToString("MM-dd-yyyy h:mm tt") },
                { Details.Folderpath, ((MAPIFolder) item.Parent).FolderPath}
            };
            //return $"Type: {item.GetType()} Subject: {item.Subject} SentOn: {item.SentOn.ToString("MM-dd-yyyy h:mm t")}";
        }

        public static Dictionary<Details, string> ExtractSummary(TaskRequestItem item)
        {
            
            return new Dictionary<Details, string>()
            {
                { Details.Type, typeof(TaskRequestItem).ToString() },
                { Details.Subject, item.Subject },
                { Details.Date, item.CreationTime.ToString("MM-dd-yyyy h:mm tt") },
                { Details.Folderpath, ((MAPIFolder) item.Parent).FolderPath}
            };
            //AppointmentItem, MeetingItem, TaskRequestItem, TaskRequestUpdateItem
        }

        public static Dictionary<Details, string> ExtractSummary(TaskRequestUpdateItem item)
        {
            return new Dictionary<Details, string>()
            {
                { Details.Type, typeof(TaskRequestUpdateItem).ToString() },
                { Details.Subject, item.Subject },
                { Details.Date, item.LastModificationTime.ToString("MM-dd-yyyy h:mm tt") },
                { Details.Folderpath, ((MAPIFolder) item.Parent).FolderPath}
            };
            //AppointmentItem, MeetingItem, TaskRequestItem, TaskRequestUpdateItem
        }
    }
}
