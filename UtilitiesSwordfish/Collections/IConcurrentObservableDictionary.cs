// Authored by: John Stewien
// Year: 2011
// Company: Swordfish Computing
// License: 
// The Code Project Open License http://www.codeproject.com/info/cpol10.aspx
// Originally published at:
// http://www.codeproject.com/Articles/208361/Concurrent-Observable-Collection-Dictionary-and-So
// Last Revised: September 2012

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Swordfish.NET.Collections
{
    public interface IConcurrentObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, ICollection, INotifyCollectionChanged, IDisposable
    {
        int IndexOfKey(TKey key);
        TValue RetrieveOrAdd(TKey key, Func<TValue> getValue);
        bool TryAdd(TKey key, TValue value);
        bool TryGetIndexOf(TKey key, out int index);
        
    }
}