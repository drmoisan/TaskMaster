using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class ScoStack<T>: ScoCollection<T>
    {
        public ScoStack() : base() { }
        public ScoStack(List<T> listOfT) : base(listOfT) { }
        public ScoStack(IEnumerable<T> IEnumerableOfT) : base(IEnumerableOfT) { }
        public ScoStack(string filename, string folderpath) : base(filename, folderpath) { }
        public ScoStack(string filename, string folderpath, bool askUserOnError) : base(filename, folderpath, askUserOnError) { }

        #region Stack

        public T Peek()
        {
            if (this.Count == 0)
                throw new InvalidOperationException("Stack is empty. No element to peek at");
            return this[0];
        }

        public T Peek(int index)
        {
            if (this.Count - 1 < index)
                throw new IndexOutOfRangeException($"Index {index} out of range. Stack only has {this.Count} elements.");
            return this[index];
        }

        public T Pop()
        {
            if (this.Count == 0)
                throw new InvalidOperationException("Stack is empty. Cannot pop an element");
            T result = this[0];
            this.RemoveAt(0);
            return result;
        }

        public T Pop(int index)
        {
            if (this.Count - 1 < index)
                throw new IndexOutOfRangeException($"Index {index} out of range. Stack only has {this.Count} elements.");
            T result = this[index];
            this.RemoveAt(index);
            return result;
        }

        public void Push(T item) => this.Insert(0, item);

        public T[] ToArray() => this.ToArray();

        public T[] ToArray(bool reverse)
        {
            if (reverse) { return Enumerable.Reverse(this).ToArray(); }
            else { return this.ToArray(); }
        }


        public List<T> ToList(bool reverse)
        {
            if (reverse) { return new List<T>(Enumerable.Reverse(this)); }
            else { return this.ToList(); }
        }

        public bool TryPeek(out T result)
        {
            try
            {
                result = this[0];
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
                result = this[index];
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
                result = this[0];
                this.RemoveAt(0);
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
                result = this[index];
                this.RemoveAt(index);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        #endregion
    }
}
