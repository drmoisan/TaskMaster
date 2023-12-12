using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace QuickFiler.Helper_Classes
{
    // Simple wrapper around LinkedList<T> that locks on all public methods except
    // GetEnumerator(), GetObjectData(), and OnDeserialization().
    internal class LockingLinkedList<T>: LinkedList<T>
    {
        public new LinkedListNode<T> First { get { lock (this) { return base.First; } } }
        public new LinkedListNode<T> Last { get { lock (this) { return base.Last; } } }
        public new int Count { get { lock (this) { return base.Count; } } }
        
        public new void AddFirst(T item)
        {
            lock (this)
            {
                base.AddFirst(item);
            }
        }
        
        public new void AddLast(T item) 
        { 
            lock (this) 
            { 
                base.AddLast(item); 
            } 
        }

        public new void AddBefore(LinkedListNode<T> node, T item)
        {
            lock (this)
            {
                base.AddBefore(node, item);
            }
        }

        public new void AddAfter(LinkedListNode<T> node, T item)
        {
            lock (this)
            {
                base.AddAfter(node, item);
            }
        }

        public new void Clear() 
        { 
            lock (this)
            {
                base.Clear(); 
            } 
        }

        public new bool Contains(T item)
        {
            lock (this)
            {
                return base.Contains(item);
            }
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            lock (this)
            {
                base.CopyTo(array, arrayIndex);
            }
        }

        public new LinkedListNode<T> Find(T value)
        {
            lock (this)
            {
                return base.Find(value);
            }
        }

        public LinkedListNode<T> Find(Predicate<T> predicate)
        {
            lock (this)
            {
                foreach (var item in this)
                {
                    if(predicate(item))
                    {
                        return Find(item);
                    }
                }
                return null;
            }
        }

        public new LinkedListNode<T> FindLast(T value)
        {
            lock (this)
            {
                return base.FindLast(value);
            }
        }

        public new void Remove(T item)
        {
            lock (this)
            {
                base.Remove(item);
            }
        }

        public new void Remove(LinkedListNode<T> node)
        {
            lock (this)
            {
                base.Remove(node);
            }
        }

        public void Remove(Predicate<T> match)
        {
            lock (this)
            {
                foreach (var item in this)
                {
                    if (match(item))
                    {
                        Remove(item);
                        break;
                    }
                }
            }
        }

        public new void RemoveFirst()
        {
            lock (this)
            {
                base.RemoveFirst();
            }
        }

        public new void RemoveLast()
        {
            lock (this)
            {
                base.RemoveLast();
            }
        }
        
        public T TakeFirst()
        {
            lock (this)
            {
                var node = base.First;
                base.RemoveFirst();
                return node.Value;
            }
        }

        public T[] TakeFirst(int n) 
        {             
            lock (this)
            {
                if (n > base.Count || n < 1)
                {
                    throw new ArgumentOutOfRangeException("n", $"n must be between 1 and Count {base.Count}");
                }
                var nodes = new T[n];
                for (int i = 0; i < n; i++)
                {
                    nodes[i] = base.First.Value;
                    base.RemoveFirst();
                }
                return nodes;
            }
        }

        public T[] TryTakeFirst(int n)
        {
            lock (this)
            {
                if (n < 1) { return null; }
                
                var nodes = new T[n];
                var nAdj = Math.Min(n, base.Count);
                
                for (int i = 0; i < n; i++)
                {
                    nodes[i] = base.First.Value;
                    base.RemoveFirst();
                }

                return nodes;
            }
        }

        public T TakeLast()
        {
            lock (this)
            {
                var node = base.Last;
                base.RemoveLast();
                return node.Value;
            }
        }

        public T[] TakeLast(int n)
        {
            lock (this)
            {
                var nodes = new T[n];
                for (int i = n - 1; i >= 0; i--)
                {
                    nodes[i] = base.Last.Value;
                    base.RemoveLast();
                }
                return nodes;
            }
        }
    }
}
