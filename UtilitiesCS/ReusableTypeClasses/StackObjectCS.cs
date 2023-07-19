using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class StackObjectCS<T> : IEnumerable<T>, ICollection<T>
    {
        #region Private

        private List<T> _list;

        #endregion

        #region Constructors

        public StackObjectCS() 
        { 
            _list = new List<T>();
        }

        public StackObjectCS(IEnumerable<T> items) 
        { 
            _list = new List<T>(items);
        }

        #endregion

        #region Stack

        public T Peek()
        {
            if (_list.Count == 0)
                throw new InvalidOperationException("Stack is empty. No element to peek at");
            return _list[0]; 
        }

        public T Peek(int index)
        {
            if (_list.Count - 1 < index)
                throw new IndexOutOfRangeException($"Index {index} out of range. Stack only has {_list.Count} elements.");
            return _list[index];
        }

        public T Pop() 
        {
            if (_list.Count == 0)
                throw new InvalidOperationException("Stack is empty. Cannot pop an element");
            T result = _list[0];
            _list.RemoveAt(0);
            return result;
        }

        public T Pop(int index)
        {
            if (_list.Count -1 < index)
                throw new IndexOutOfRangeException($"Index {index} out of range. Stack only has {_list.Count} elements.");
            T result = _list[index];
            _list.RemoveAt(index);
            return result;
        }

        public void Push(T item) => _list.Insert(0,item);

        public T[] ToArray() => _list.ToArray();

        public T[] ToArray(bool reverse)
        {
            if (reverse) { return Enumerable.Reverse(_list).ToArray(); }
            else { return _list.ToArray(); }
        }

        public List<T> ToList() => _list;
        
        public List<T> ToList(bool reverse)
        {
            if (reverse) { return new List<T>(Enumerable.Reverse(_list)); }
            else { return _list; }
        }

        public bool TryPeek(out T result) 
        {
            try
            {
                result = _list[0];
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public bool TryPeek(out T result, int index)
        {
            try
            {
                result = _list[index];
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public bool TryPop(out T result)
        {
            try
            {
                result = _list[0];
                _list.RemoveAt(0);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public bool TryPop(out T result, int index)
        {
            try
            {
                result = _list[index];
                _list.RemoveAt(index);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }
        
        #endregion

        #region ICollection<T>

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(T item) => _list.Insert(0,item);
        
        public void Clear() => _list = new List<T>();

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        
        #endregion

        //#region ICollection

        //public object SyncRoot => throw new NotImplementedException();

        //public bool IsSynchronized => throw new NotImplementedException();

        //public void CopyTo(Array array, int index) => _list.CopyTo(array, index);
        
        
        //#endregion

        #region IEnumberable<T>
        
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public bool Remove(T item) => _list.Remove(item);
        
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
        
        #endregion
    }
}
