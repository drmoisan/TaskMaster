// Authored by: John Stewien
// Year: 2011
// Company: Swordfish Computing
// License: 
// The Code Project Open License http://www.codeproject.com/info/cpol10.aspx
// Originally published at:
// http://www.codeproject.com/Articles/208361/Concurrent-Observable-Collection-Dictionary-and-So
// Last Revised: September 2012

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using Swordfish.NET.General.Collections;

namespace Swordfish.NET.Collections 
{
    /// <summary>
    /// This class provides a collection that can be bound to
    /// a WPF control, where the collection can be modified from a thread
    /// that is not the GUI thread. The notify event is thrown using the
    /// dispatcher from the event listener(s).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentObservableCollection<T> :
        ConcurrentObservableBase<T>,
        IList<T>,
        ICollection<T>,
        IList,
        ICollection
    {

        // ************************************************************************
        // Constructors
        // ************************************************************************
        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConcurrentObservableCollection() { }

        /// <summary>
        /// Constructor that takes an enumerable from which the collection is populated
        /// </summary>
        /// <param name="enumerable"></param>
        public ConcurrentObservableCollection(IEnumerable<T> enumerable) : base(enumerable) { }

        #endregion Constructors

        // ************************************************************************
        // IList<T> Implementation
        // ************************************************************************
        #region IList<T> Implementation

        public int IndexOf(T item)
        {
            return DoBaseRead(() =>
            {
                return ReadCollection.IndexOf(item);
            });
        }

        public void Insert(int index, T item)
        {
            DoBaseWrite(() =>
            {
                WriteCollection.Insert(index, item);
            });
        }

        public void RemoveAt(int index)
        {
            DoBaseWrite(() =>
            {
                WriteCollection.RemoveAt(index);
            });
        }

        public T this[int index]
        {
            get => DoBaseRead(() => ReadCollection[index]);
            set => DoBaseWrite(() => WriteCollection[index] = value);
        }

        #endregion IList<T> Implementation

        // ************************************************************************
        // ICollection<T> Implementation
        // ************************************************************************
        #region ICollection<T> Implementation

        public void Add(T item) => DoBaseWrite(() => WriteCollection.Add(item));

        public void Clear() => DoBaseClear(() => { });

        public bool Contains(T item) => DoBaseRead(() => ReadCollection.Contains(item));

        public void CopyTo(T[] array, int arrayIndex) => DoBaseRead(() => ReadCollection.CopyTo(array, arrayIndex));

        public int Count => DoBaseRead(() => ReadCollection.Count);

        public bool IsReadOnly => DoBaseRead(() => ((ICollection<T>)ReadCollection).IsReadOnly);

        public bool Remove(T item) => DoBaseWrite(() => WriteCollection.Remove(item));

        #endregion ICollection<T> Implementation

        // ************************************************************************
        // ICollection Implementation
        // ************************************************************************
        #region ICollection Implementation

        void ICollection.CopyTo(Array array, int index) =>
            DoBaseRead(() => ((ICollection)ReadCollection).CopyTo(array, index));

        bool ICollection.IsSynchronized =>
            DoBaseRead(() => ((ICollection)ReadCollection).IsSynchronized);

        object ICollection.SyncRoot
        {
            get
            {
                return DoBaseRead(() =>
                {
                    return ((ICollection)ReadCollection).SyncRoot;
                });
            }
        }

        #endregion ICollection Implementation

        // ************************************************************************
        // IList Implementation
        // ************************************************************************
        #region IList Implementation

        int IList.Add(object value)
        {
            return DoBaseWrite(() =>
            {
                return ((IList)WriteCollection).Add(value);
            });
        }

        bool IList.Contains(object value)
        {
            return DoBaseRead(() =>
            {
                return ((IList)ReadCollection).Contains(value);
            });
        }

        int IList.IndexOf(object value)
        {
            return DoBaseRead(() =>
            {
                return ((IList)ReadCollection).IndexOf(value);
            });
        }

        void IList.Insert(int index, object value)
        {
            DoBaseWrite(() =>
            {
                ((IList)WriteCollection).Insert(index, value);
            });
        }

        bool IList.IsFixedSize
        {
            get
            {
                return DoBaseRead(() =>
                {
                    return ((IList)ReadCollection).IsFixedSize;
                });
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return DoBaseRead(() =>
                {
                    return ((IList)ReadCollection).IsReadOnly;
                });
            }
        }

        void IList.Remove(object value)
        {
            DoBaseWrite(() =>
            {
                ((IList)WriteCollection).Remove(value);
            });
        }

        void IList.RemoveAt(int index)
        {
            DoBaseWrite(() =>
            {
                ((IList)WriteCollection).RemoveAt(index);
            });
        }

        object IList.this[int index]
        {
            get
            {
                return DoBaseRead(() =>
                {
                    return ((IList)ReadCollection)[index];
                });
            }
            set
            {
                DoBaseWrite(() =>
                {
                    ((IList)WriteCollection)[index] = value;
                });
            }
        }

        #endregion IList Implementation

        #region List<T> Methods by Dan Moisan

        public bool Exists(Predicate<T> match)
        {
            return this.FindIndex(match) != -1;
        }

        public T Find(Predicate<T> match)
        {
            var index = this.FindIndex(0, this.Count, match);
            if (index == -1)
            {
                return default(T);
            }
            else
            {
                return this[index];
            }
        }

        public int[] FindIndices(Predicate<T> match)
        {
            return this.FindIndices(0, this.Count, match);
        }

        public int[] FindIndices(int startIndex, Predicate<T> match)
        {
            return this.FindIndices(startIndex, this.Count, match);
        }

        public int[] FindIndices(int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)this.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startIndex), $"{nameof(startIndex)} " +
                    $"has a value of {startIndex} which is greater " +
                    $"than the list length of {this.Count}");
            }

            if (count < 0 || startIndex > this.Count - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var indices = new List<int>();

            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(this[i]))
                {
                    indices.Add(i);
                }
            }

            return indices.ToArray();
        }

        public int FindIndex(Predicate<T> match)
        {
            return this.FindIndex(0, this.Count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return this.FindIndex(startIndex, this.Count - startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if ((uint)startIndex > (uint)this.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startIndex), $"{nameof(startIndex)} has " +
                    $"a value of {startIndex} which is greater than " +
                    $"the list length of {this.Count}");
            }

            if (count < 0 || startIndex > this.Count - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(this[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion List<T> Methods by Dan Moisan
    }
}
