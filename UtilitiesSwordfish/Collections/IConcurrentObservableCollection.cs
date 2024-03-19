using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swordfish.NET.General.Collections
{
    public interface IConcurrentObservableCollection<T>:
        IConcurrentObservableBase<T>,
        IList<T>,
        ICollection<T>,
        IList,
        ICollection
    {
        #region IList Implementation

        


        #endregion IList Implementation

        #region List<T> Selected Methods

        bool Exists(Predicate<T> match);
        T Find(Predicate<T> match);
        int FindIndex(int startIndex, int count, Predicate<T> match);
        int FindIndex(int startIndex, Predicate<T> match);
        int FindIndex(Predicate<T> match);
        int[] FindIndices(int startIndex, int count, Predicate<T> match);
        int[] FindIndices(int startIndex, Predicate<T> match);
        int[] FindIndices(Predicate<T> match);

        #endregion List<T> Selected Methods
    }
}
