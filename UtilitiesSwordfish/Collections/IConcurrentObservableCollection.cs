using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.General.Collections
{
    public interface IConcurrentObservableCollection<T>: IConcurrentObservableBase<T>, IList<T>, ICollection<T>, IList, ICollection    
    {
    }
}
