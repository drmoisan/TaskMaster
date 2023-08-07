using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace ToDoModel
{


    public static class ModuleMailItemsSort
    {
        public static IList MailItemsSort(Items olItems, SortOptionsEnum options)
        {
            var query = QueryMailItems(olItems, options);

            if (options.HasFlag(SortOptionsEnum.ConversationUniqueOnly))
            {
                query = QueryUniqueConversations(query, options);
            }

            if (options.HasFlag(SortOptionsEnum.TriageIgnore))
            {
                if (options.HasFlag(SortOptionsEnum.DateRecentFirst))
                {
                    query = query.OrderByDescending(mail => mail.SentOn);
                }
                else { query = query.OrderBy(mail => mail.SentOn); }
            }
            else 
            {
                if (options.HasFlag(SortOptionsEnum.TriageImportantFirst))
                {
                    if (options.HasFlag(SortOptionsEnum.DateRecentFirst))
                    {
                        query = query.OrderBy(mail =>
                        {
                            var letter = mail.GetTriage();
                            if (letter == "") { return "Z"; }
                            else { return letter; }
                        }).ThenByDescending(mail => mail.SentOn);
                    }
                    else
                    {
                        query = query.OrderBy(mail =>
                        {
                            var letter = mail.GetTriage();
                            if (letter == "") { return "Z"; }
                            else { return letter; }
                        }).ThenBy(mail => mail.SentOn);
                    }

                }
                else if (options.HasFlag(SortOptionsEnum.TriageImportantLast))
                {
                    if (options.HasFlag(SortOptionsEnum.DateRecentFirst))
                    {
                        query = query.OrderByDescending(mail => mail.GetTriage())
                                     .ThenByDescending(mail => mail.SentOn);
                    }
                    else
                    {
                        query = query.OrderByDescending(mail => mail.GetTriage())
                                     .ThenBy(mail => mail.SentOn);
                    }
                        
                }
                else { throw new ArgumentException("No triage option is selected"); }
            }

            return query.ToList();

        }

        private static IEnumerable<MailItem> QueryMailItems(Items olItems, SortOptionsEnum options)
        {
            return (olItems as List<object>).Select(item => MailResolution.TryResolveMailItem(item))
                                            .Where(mail => mail is not null);
        }

        private static IEnumerable<MailItem> QueryUniqueConversations(IEnumerable<MailItem> query, SortOptionsEnum options)
        {
            var groupBy = query.GroupBy(mail => mail.ConversationID);
            if (options.HasFlag(SortOptionsEnum.DateRecentFirst))
            {
                return query.GroupBy(mail => mail.ConversationID)
                            .Select(group => new { Group = group.Key, Elements = group.OrderByDescending(mail => mail.SentOn) })
                            .Select(group => group.Elements.First());
            }
            else
            {
                return query.GroupBy(mail => mail.ConversationID)
                            .Select(group => new { Group = group.Key, Elements = group.OrderByDescending(mail => mail.SentOn) })
                            .Select(group => group.Elements.First());
            }
        }
        
        private static (Items Filtered, Items Remaining) FilterTriageGroup(Items olItems, string triageIdentifier)
        {
            var filter = "[Triage] = " + '"' + triageIdentifier + '"';
            var filterInverse = "[Triage] <> " + '"' + triageIdentifier + '"';
            var olFilteredItems = olItems.Restrict(filter);
            var olRemainingItems = olItems.Restrict(filterInverse);
            return (Filtered: olFilteredItems, Remaining: olRemainingItems);
        }

        private static (int From, int To, int Step, bool Triage) GetIterationRange(SortOptionsEnum options)
        {
            
            if (options.HasFlag(SortOptionsEnum.TriageImportantFirst))
            {
                return (From: 1, To: 3, Step: 1, Triage: true);
            }
            else if (options.HasFlag(SortOptionsEnum.TriageImportantLast))
            {
                return (From: 3, To: 1, Step: -1, Triage: true);
            }
            else { throw new ArgumentException("Unsupported sort option"); }
        }

        [Flags]
        public enum SortOptionsEnum
        {
            TriageIgnore = 1,
            TriageImportantFirst = 2,
            TriageImportantLast = 4,
            DateRecentFirst = 8,
            DateOldestFirst = 16,
            ConversationUniqueOnly = 32
        }

    }
}