using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class RecentsList<T> : SerializableList<T>, IRecentsList<T>
    {
        public RecentsList() : base() { }
        public RecentsList(List<T> listOfT, int max) : base(listOfT) { Max = max; }
        public RecentsList(IEnumerable<T> IEnumerableOfT, int max) : base(IEnumerableOfT) { Max = max; }
        public RecentsList(string filename, string folderpath, int max) : base(filename, folderpath) { Max = max;}

        private int _max = 5;
        public int Max { get => _max; set => _max = value; }
        
        public new void Add(T item)
        {
            base.ensureList();
            if (base.Contains(item))
            {
                base.Remove(item);
            }
            else
            {
                if (base.Count > 0)
                {
                    var idx = Math.Min(base.Count, Max) - 1;
                    base.RemoveAt(idx);
                }
            }
            base.Insert(0, item);
            base.Serialize();
        }
    }
}
