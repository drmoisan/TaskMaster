using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookExtensions
{
    public class OutlookItemFlaggableTry : OutlookItemTry, IOutlookItemFlaggable
    {
        protected IOutlookItemFlaggable OlItem { get; set; }

        public OutlookItemFlaggableTry(IOutlookItemFlaggable olItem) : base(olItem) { OlItem = olItem; }

        public bool Complete { get => TryGet(() => OlItem.Complete); set => TrySet((x) => OlItem.Complete = x, value); }
        public DateTime DueDate { get => TryGet(() => OlItem.DueDate); set => TrySet((x) => OlItem.DueDate = x, value); }
        public bool FlagAsTask { get => TryGet(() => OlItem.FlagAsTask); set => TrySet((x) => OlItem.FlagAsTask = x, value); }
        public new DateTime TaskStartDate { get => TryGet(() => OlItem.TaskStartDate); set => TrySet((x) => OlItem.TaskStartDate = x, value); }
        public string TaskSubject { get => TryGet(() => OlItem.TaskSubject); set => TrySet((x) => OlItem.TaskSubject = x, value); }
        public int TotalWork { get => TryGet(() => OlItem.TotalWork); set => TrySet((x) => OlItem.TotalWork = x, value); }

        //public object[] Args => TryGet(() => OlItem.Args); 
        //public OlObjectClass Class => TryGet(() => OlItem.Class);
        //public Inspector Inspector => TryGet(() => OlItem.Inspector); 
        //public bool NoAging { get => TryGet(() => OlItem.NoAging); set => TrySet((x) => OlItem.NoAging = x, value); }
        //public DateTime ReminderTime { get => TryGet(() => OlItem.ReminderTime); set => TrySet((x) => OlItem.ReminderTime = x, value); }

        DateTime IOutlookItem.TaskStartDate => TryGet(() => OlItem.TaskStartDate);

        public string GetUdfString(string fieldName) => TryCall(() => OlItem.GetUdfString(fieldName));
    }
}
