using System;

namespace UtilitiesCS.OutlookExtensions
{
    public interface IOutlookItemFlaggable: IOutlookItem
    {
        bool Complete { get; set; }
        DateTime DueDate { get; set; }
        bool FlagAsTask { get; set; }
        new DateTime TaskStartDate { get; set; }
        string TaskSubject { get; set; }
        int TotalWork { get; set; }
    }
}