using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Interfaces;

namespace TaskVisualization
{
    public class FlagChangeItem : IFlagChangeItem
    {
        public FlagChangeItem() { }

        public string ClassifierName { get; set; }

        public IList<string> UntrainFlags { get; set; } = [];

        public IList<string> TrainFlags { get; set; } = [];

    }
}
