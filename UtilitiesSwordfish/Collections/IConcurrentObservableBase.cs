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
using System.Collections.Specialized;

namespace Swordfish.NET.Collections
{
    public interface IConcurrentObservableBase<T> : IObservable<NotifyCollectionChangedEventArgs>, INotifyCollectionChanged, IEnumerable<T>, IDisposable
    {
        ImmutableCollectionBase<T> Snapshot { get; }
    }
}