using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection
{
    /// <summary>
    /// A vendored-dependency-free observable collection built on
    /// <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>. It is the clean
    /// replacement base for the former <c>ScoCollection&lt;T&gt;</c> (which derived from the
    /// now-removed vendored observable-collection library).
    ///
    /// The type re-exposes the list-search surface (<see cref="Find(Predicate{T})"/>,
    /// <see cref="FindIndex(Predicate{T})"/>, <see cref="FindIndices(Predicate{T})"/>,
    /// <see cref="Exists(Predicate{T})"/>) that subclasses invoke via <c>base.</c>, an
    /// <see cref="IObserver{T}"/> <see cref="Subscribe"/> facility, and inherits the native
    /// <see cref="ObservableCollection{T}.CollectionChanged"/> event.
    ///
    /// Serialization members (file constructors, <c>Serialize</c>/<c>Deserialize</c>, the disk
    /// path accessors, and the injectable filesystem/prompt seams) live in the
    /// <c>ConcurrentObservableCollection.Serialization.cs</c> partial.
    ///
    /// Thread-safety note: unlike the former vendored base, this type does not use a
    /// <c>ReaderWriterLockSlim</c>. Mutations raise <see cref="ObservableCollection{T}"/> events
    /// synchronously on the calling thread. Production write paths already run under
    /// <c>Task.Run</c>; no consumer depends on concurrent multi-writer semantics.
    /// </summary>
    public partial class ConcurrentObservableCollection<T> : ObservableCollection<T>, IList
    {
        #region Constructors

        public ConcurrentObservableCollection()
            : base() { }

        public ConcurrentObservableCollection(IEnumerable<T> enumerable)
            : base(enumerable) { }

        #endregion Constructors

        #region List<T> search surface

        /// <summary>Returns <see langword="true"/> when any element matches <paramref name="match"/>.</summary>
        public bool Exists(Predicate<T> match) => IListExtensions.Exists(this, match);

        /// <summary>Returns the first element matching <paramref name="match"/> or <c>default</c>.</summary>
        public T Find(Predicate<T> match) => IListExtensions.Find(this, match);

        /// <summary>Returns the index of the first element matching <paramref name="match"/> or <c>-1</c>.</summary>
        public int FindIndex(Predicate<T> match) => IListExtensions.FindIndex(this, match);

        /// <summary>Returns the index of the first match at or after <paramref name="startIndex"/>.</summary>
        public int FindIndex(int startIndex, Predicate<T> match) =>
            IListExtensions.FindIndex(this, startIndex, match);

        /// <summary>Returns the index of the first match within the given range or <c>-1</c>.</summary>
        public int FindIndex(int startIndex, int count, Predicate<T> match) =>
            IListExtensions.FindIndex(this, startIndex, count, match);

        /// <summary>Returns the indices of all elements matching <paramref name="match"/>.</summary>
        public int[] FindIndices(Predicate<T> match) => IListExtensions.FindIndices(this, match);

        /// <summary>Returns the matching indices at or after <paramref name="startIndex"/>.</summary>
        public int[] FindIndices(int startIndex, Predicate<T> match) =>
            IListExtensions.FindIndices(this, startIndex, match);

        /// <summary>Returns the matching indices within the given range.</summary>
        public int[] FindIndices(int startIndex, int count, Predicate<T> match) =>
            IListExtensions.FindIndices(this, startIndex, count, match);

        #endregion List<T> search surface

        #region List conversion

        /// <summary>Returns a snapshot <see cref="List{T}"/> copy of the current elements.</summary>
        public List<T> ToList()
        {
            return new List<T>(this);
        }

        /// <summary>Replaces the current contents with <paramref name="value"/> (null clears).</summary>
        public void FromList(IList<T> value)
        {
            Clear();
            if (value is null)
            {
                return;
            }

            foreach (var item in value)
            {
                Add(item);
            }
        }

        #endregion List conversion

        #region Observer (IObservable-style subscription)

        private readonly Dictionary<int, IObserver<NotifyCollectionChangedEventArgs>> _subscribers =
            new Dictionary<int, IObserver<NotifyCollectionChangedEventArgs>>();
        private int _subscriberKey;

        /// <summary>
        /// Registers <paramref name="observer"/> to receive collection-change notifications.
        /// On subscribe, each current element is replayed as an
        /// <see cref="NotifyCollectionChangedAction.Add"/> notification (matching the prior
        /// observable semantics). The returned token unsubscribes on dispose.
        /// </summary>
        public IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs> observer)
        {
            if (observer is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            int key = _subscriberKey++;
            _subscribers.Add(key, observer);

            foreach (var item in this.ToArray())
            {
                observer.OnNext(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item)
                );
            }

            return new Unsubscriber(() => _subscribers.Remove(key));
        }

        /// <summary>
        /// Raises the native <see cref="ObservableCollection{T}.CollectionChanged"/> event and
        /// then forwards the same args to any <see cref="IObserver{T}"/> subscribers.
        /// </summary>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            if (_subscribers.Count == 0)
            {
                return;
            }

            foreach (var observer in _subscribers.Values.ToArray())
            {
                observer.OnNext(e);
            }
        }

        private sealed class Unsubscriber : IDisposable
        {
            private Action _dispose;

            public Unsubscriber(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                _dispose?.Invoke();
                _dispose = null;
            }
        }

        #endregion Observer
    }
}
