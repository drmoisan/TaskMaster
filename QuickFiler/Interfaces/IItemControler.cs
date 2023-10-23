using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickFiler
{
    public interface IItemControler
    {
        int CounterEnter { get; set; }
        int CounterComboRight { get; set; }
        public Dictionary<string, System.Action> RightKeyActions { get; }
    }
}
