using System;
using System.Collections.Generic;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers
{
    internal class EmailSorter
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public EmailSorter() { }

        public EmailSorter(SortOptionsEnum options)
        {
            _options = options;
        }

        private SortOptionsEnum _options = SortOptionsEnum.Default;
        private Dictionary<string, int> _triageImportantFirst = new Dictionary<string, int>
        {
            { "A", 1 },
            { "B", 2 },
            { "C", 3 },
            { "Z", 4 },
        };

        private Dictionary<string, int> _triageImportantLast = new Dictionary<string, int>
        {
            { "A", 4 },
            { "B", 3 },
            { "C", 2 },
            { "Z", 1 },
        };

        public SortOptionsEnum Options
        {
            get => _options;
            set => _options = value;
        }

        public long GetSortKey(string triage, DateTime dateTime)
        {
            if (
                _options.HasFlag(SortOptionsEnum.TriageImportantFirst)
                && _options.HasFlag(SortOptionsEnum.DateRecentFirst)
            )
            {
                try
                {
                    var triageKey =
                        (long)(100000000000000 * _triageImportantLast[triage])
                        + GetDateKey(dateTime);
                    return triageKey;
                }
                catch (KeyNotFoundException e)
                {
                    logger.Error(
                        $"Triage value {triage} not found in "
                            + $"dictionary from date {GetDateKey(dateTime)} "
                            + $"\n {e.Message} \n {e.StackTrace}"
                    );
                    throw;
                }
            }
            return -1;
        }

        public long GetDateKey(DateTime dateTime)
        {
            return long.Parse(dateTime.ToString("yyyyMMddHHmmss"));
        }
    }

    public interface IEmailSortInfo
    {
        string EntryId { get; }
        string MessageClass { get; }
        DateTime SentOn { get; }
        string ConversationId { get; }
        string Triage { get; }
        string StoreId { get; }
    }
}
