using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickFiler.Interfaces
{
    public interface IKbdAction<T, U>
    {
        string SourceId { get; set; }
        T Key { get; set; }
        U Delegate { get; set; }
        bool KeyEquals(T other);
        //Action<string> Update { get; set; }
        //Type DelegateType { get; }
    }

}
