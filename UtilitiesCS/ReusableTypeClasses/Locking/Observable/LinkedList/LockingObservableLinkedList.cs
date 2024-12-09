using AngleSharp.Common;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList
{
    public class LockingObservableLinkedList<T> : LockingLinkedList<T>
    {

        public LockingObservableLinkedList() : base() { }
        public LockingObservableLinkedList(IEnumerable<T> collection) : base(collection) { }

        #region CollectionChanged Implementation

        public event EventHandler<LockingObservableLinkedListChangedEventArgs<T>> CollectionChanged;

        protected virtual void OnCollectionChanged(LockingObservableLinkedListChangedEventArgs<T> changeAction)
        {
            var tasks = new List<Task> { Task.Run(() => CollectionChanged?.Invoke(this, changeAction)) };

            if (changeAction.Action != NotifyCollectionChangedAction.Reset &&
                changeAction.OldNode is not null &&
                _observers.TryGetValue(changeAction.OldNode, out var observers))
            {
                tasks.AddRange(observers.Select(o => Task.Run(() => o.OnEventOccur(changeAction))));
            }

            Task.WaitAll(tasks.ToArray());
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, LockingObservableLinkedListNode<T> newNode, LockingObservableLinkedListNode<T> oldNode)
        {
            OnCollectionChanged(new LockingObservableLinkedListChangedEventArgs<T>(action, newNode, oldNode));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, LockingObservableLinkedListNode<T> node)
        {
            LockingObservableLinkedListNode<T> newNode = default;
            LockingObservableLinkedListNode<T> oldNode = default;
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    newNode = node;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    oldNode = node;
                    break;
                default:
                    return;
            }
            OnCollectionChanged(action, newNode, oldNode);
        }

        #endregion CollectionChanged Implementation

        #region Wrapper Methods for LockingLinkedList

        public new LockingObservableLinkedListNode<T> First { get { lock (this) { return ToLocking(base.First); } } }
        public new LockingObservableLinkedListNode<T> Last { get { lock (this) { return ToLocking(base.Last); } } }

        public new void AddFirst(T item)
        {
            base.AddFirst(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, this.First);
        }

        public void AddOrMoveFirst(T item) 
        {
            var node = Find(item);
            if (node is null)
            {
                AddFirst(item);
            }
            else if (node == First)
            {
                return;
            }
            
            else
            {
                base.Remove(node.innerNode);
                base.AddFirst(item);
                OnCollectionChanged(NotifyCollectionChangedAction.Move, this.First, node);
            }
        }

        public void AddOrMoveFirst(T item, int max)
        {
            var node = Find(item);
            if (node is null)
            {
                AddFirst(item);
                if (Count > max)
                {
                    RemoveLast();
                }
            }
            else if(node == First)
            {
                return;
            }
            else
            {
                base.Remove(node.innerNode);
                base.AddFirst(item);
                OnCollectionChanged(NotifyCollectionChangedAction.Move, this.First, node);
            }
        }

        public new void AddLast(T item)
        {
            base.AddLast(item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, this.Last);
        }

        public void AddBefore(LockingObservableLinkedListNode<T> node, T item)
        {
            base.AddBefore(node.innerNode, item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, node.Previous, node);
        }

        public void AddAfter(LockingObservableLinkedListNode<T> node, T item)
        {
            base.AddAfter(node.innerNode, item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, node.Next, node);
        }

        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, null);
        }

        public new LockingObservableLinkedListNode<T> Find(T value)
        {
            return ToLocking(base.Find(value));
        }

        public new LockingObservableLinkedListNode<T> Find(Predicate<T> predicate)
        {
            return ToLocking(base.Find(predicate));
        }

        public new LockingObservableLinkedListNode<T> FindLast(T value)
        {
            return ToLocking(base.FindLast(value));
        }

        public void MoveBefore(LockingObservableLinkedListNode<T> node, LockingObservableLinkedListNode<T> target)
        {
            base.MoveBefore(node.innerNode, target.innerNode);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, target, node);
        }

        public void MoveAfter(LockingObservableLinkedListNode<T> node, LockingObservableLinkedListNode<T> target)
        {
            base.MoveAfter(node.innerNode, target.innerNode);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, target, node);
        }

        public void MoveUp(LockingObservableLinkedListNode<T> node)
        {
            base.MoveUp(node.innerNode);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, node.Previous, node);
        }

        public void MoveDown(LockingObservableLinkedListNode<T> node)
        {
            base.MoveDown(node.innerNode);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, node.Next, node);
        }

        public new void Remove(T item)
        {
            var node = Find(item);
            if (node != null)
            {
                Remove(node);
            }
        }

        public void Remove(LockingObservableLinkedListNode<T> node)
        {
            base.Remove(node.innerNode);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, node);
        }

        public new void Remove(Predicate<T> match)
        {
            while (First is not null && match(First.Value))
            {
                Remove(First);
            }
            var current = First;

            while (current is not null)
            {
                var next = current.Next;
                if (match(current.Value))
                {
                    Remove(current);
                }
                current = next;
            }
        }

        public new void RemoveFirst()
        {
            if (First is not null)
            {
                Remove(First);
            }
        }

        public new void RemoveLast()
        {
            if (Last is not null)
            {
                Remove(Last);
            }
        }

        public new T TakeFirst()
        {
            var node = First;
            if (node is null) { return default; }
            else
            {
                Remove(node);
                return node.Value;
            }
        }

        public new T[] TakeFirst(int n)
        {
            if (n > base.Count || n < 1)
            {
                throw new ArgumentOutOfRangeException("n", $"n must be between 1 and Count {base.Count}");
            }
            var values = new T[n];
            for (int i = 0; i < n; i++)
            {
                values[i] = TakeFirst();
            }
            return values;
        }

        public new T[] TryTakeFirst(int n)
        {
            if (n < 1) { return null; }
            // Take the lesser of n and the number of elements in the list
            if (n > Count) { n = Count; }

            var values = new T[n];

            for (int i = 0; i < n; i++)
            {
                values[i] = TakeFirst();
            }

            return values;
        }

        public new T TakeLast()
        {
            var node = Last;
            if (node is null) { return default; }
            else
            {
                Remove(node);
                return node.Value;
            }
        }

        public new T[] TakeLast(int n)
        {
            var values = new T[n];
            for (int i = n - 1; i >= 0; i--)
            {
                values[i] = TakeLast();
            }
            return values;
        }

        public T[] TryTakeLast(int n)
        {
            if (n < 1) { return null; }
            // Take the lesser of n and the number of elements in the list
            if (n > Count) { n = Count; }
            var values = new T[n];
            for (int i = n - 1; i >= 0; i--)
            {
                values[i] = TakeLast();
            }
            return values;
        }

        #endregion Wrapper Methods for LockingLinkedList

        #region Observer

        public Dictionary<LockingObservableLinkedListNode<T>, HashSet<ILockingLinkedListObserver<T>>> AddPartialObserver(
            ILockingLinkedListObserver<T> observer, params LockingObservableLinkedListNode<T>[] keys)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));
            if (keys is null) throw new ArgumentNullException(nameof(keys));

            foreach (var key in keys)
            {
                _observers.AddOrUpdate(key, new HashSet<ILockingLinkedListObserver<T>> { observer }, (k, o) =>
                {
                    o.Add(observer);
                    return o;
                });
            }

            return keys.ToDictionary(k => k, k => new HashSet<ILockingLinkedListObserver<T>> { observer });
        }

        public Dictionary<LockingObservableLinkedListNode<T>, HashSet<ILockingLinkedListObserver<T>>> AddPartialObserver(
            Action<LockingObservableLinkedListChangedEventArgs<T>> action, params LockingObservableLinkedListNode<T>[] keys)
        {
            return AddPartialObserver(new SimpleActionLockingLinkedListObserver<T>(action), keys);
        }

        public Dictionary<LockingObservableLinkedListNode<T>, HashSet<ILockingLinkedListObserver<T>>> RemovePartialObserver(
            ILockingLinkedListObserver<T> observer, params LockingObservableLinkedListNode<T>[] keys)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));
            if (keys is null) throw new ArgumentNullException(nameof(keys));

            var removed = keys.Where(key =>
                _observers.TryGetValue(key, out var observers) && observers.Contains(observer) && observers.Remove(observer));
            return removed.ToDictionary(k => k, k => new HashSet<ILockingLinkedListObserver<T>> { observer });
        }

        public Dictionary<LockingObservableLinkedListNode<T>, HashSet<ILockingLinkedListObserver<T>>> RemovePartialObserver(
            ILockingLinkedListObserver<T> observer)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));

            var removed = _observers.Where(pair => pair.Value.Contains(observer) && pair.Value.Remove(observer)).Select(pair => pair.Key);
            return removed.ToDictionary(k => k, k => new HashSet<ILockingLinkedListObserver<T>> { observer });
        }

        public Dictionary<LockingObservableLinkedListNode<T>, HashSet<ILockingLinkedListObserver<T>>> RemovePartialObserver(
            params LockingObservableLinkedListNode<T>[] keys)
        {
            if (keys is null) throw new ArgumentNullException(nameof(keys));

            var removed = keys.Select(key =>
            {
                if (_observers.ContainsKey(key) && _observers.TryRemove(key, out var observers))
                {
                    return new KeyValuePair<LockingObservableLinkedListNode<T>, HashSet<ILockingLinkedListObserver<T>>>(
                        key, new HashSet<ILockingLinkedListObserver<T>>(observers));
                }
                return new KeyValuePair<LockingObservableLinkedListNode<T>, HashSet<ILockingLinkedListObserver<T>>>(key, null);
            });
            return removed.Where(pair => pair.Value != null).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public Dictionary<LockingObservableLinkedListNode<T>, HashSet<ILockingLinkedListObserver<T>>> RemoveAllObservers()
        {
            var ret = _observers.ToDictionary(kv => kv.Key, kv => new HashSet<ILockingLinkedListObserver<T>>(kv.Value));
            _observers.Clear();
            return ret;
        }


        #endregion Observer

        #region private data

        private readonly ConcurrentDictionary<LockingObservableLinkedListNode<T>, ICollection<ILockingLinkedListObserver<T>>> _observers
            = new ConcurrentDictionary<LockingObservableLinkedListNode<T>, ICollection<ILockingLinkedListObserver<T>>>();

        
        private LockingObservableLinkedListNode<T> ToLocking(LockingLinkedListNode<T> node)
        {
            if (node is null) { return null; }
            else { return new LockingObservableLinkedListNode<T>(this, node); }
        }

        #endregion


    }
}
