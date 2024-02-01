using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class RecentsList<T> : ScoCollection<T>
    {
        public RecentsList() : base() 
        {
            _ct = _cts.Token;
            //_addTask = Task.Run(() => NonBlockingConsumer_Add(_ct), _ct);
        }
        
        public RecentsList(List<T> listOfT, int max) : base(listOfT) 
        { 
            Max = max;
            _ct = _cts.Token;
            //_addTask = Task.Run(() => NonBlockingConsumer_Add(_ct), _ct);
        }
        
        public RecentsList(IEnumerable<T> IEnumerableOfT, int max) : base(IEnumerableOfT) 
        { 
            Max = max;
            _ct = _cts.Token;
            //_addTask = Task.Run(() => NonBlockingConsumer_Add(_ct), _ct);
        }
        
        public RecentsList(string filename, string folderpath, int max) : base(filename, folderpath) 
        { 
            Max = max;
            _ct = _cts.Token;
            //_addTask = Task.Run(() => NonBlockingConsumer_Add(_ct), _ct);
        }

        private int _max = 5;
        public int Max { get => _max; set => _max = value; }
        
        public new void Add(T item)
        {
            _bc.TryAdd(item);
        }

        private void AddThreadsafe(T item)
        {
            if (base.Contains(item))
            {
                base.Remove(item);
            }
            else
            {
                if (base.Count >= Max)
                {
                    var idx = Math.Min(base.Count, Max) - 1;
                    base.RemoveAt(idx);
                }
            }
            base.Insert(0, item);
            base.Serialize();
        }
        private BlockingCollection<T> _bc = [];
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private CancellationToken _ct = default;
        //private Task _addTask; 
        //private void NonBlockingConsumer_Add(CancellationToken ct) 
        //{
        //    foreach (T item in _bc.GetConsumingEnumerable(ct))
        //    {
        //        AddThreadsafe(item);
        //    }
        //}
    }
}
