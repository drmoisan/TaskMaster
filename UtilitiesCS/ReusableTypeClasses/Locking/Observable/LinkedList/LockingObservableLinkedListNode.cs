using System.Collections.Generic;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList
{
    public class LockingObservableLinkedListNode<T>
    {
        #region internal properties

        internal LockingObservableLinkedList<T> list;

        internal LockingObservableLinkedListNode<T> next;

        internal LockingObservableLinkedListNode<T> prev;

        internal T item;

        internal LockingLinkedListNode<T> innerNode;

        #endregion internal properties

        /// <summary>
        /// Gets a reference to the <see cref="LockingObservableLinkedList{T}"/> 
        /// that contains the <see cref="LockingObservableLinkedListNode{T}"/>.
        /// </summary>
        public LockingObservableLinkedList<T> List => list;

        /// <summary>
        /// Gets a reference to the next node in the <see cref="LockingObservableLinkedListNode{T}"/>,
        /// or null if the current node is the last element
        /// of the <see cref="LockingObservableLinkedListNode{T}"/>.
        /// </summary>
        public LockingObservableLinkedListNode<T> Next 
        {
            get
            {
                var nextNode = innerNode?.Next;
                if (nextNode is null) 
                { 
                    return null; 
                }
                else 
                {
                    return new LockingObservableLinkedListNode<T>(list, nextNode);
                }
            }            
        }

        /// <summary>
        /// Gets a reference to the previous node in the <see cref="LockingObservableLinkedListNode{T}"/>,
        /// or null if the current node is the first element
        /// of the <see cref="LockingObservableLinkedListNode{T}"/>.
        /// </summary>
        public LockingObservableLinkedListNode<T> Previous
        {
            get
            {
                var previousNode = innerNode?.Previous;
                if (previousNode is null)
                {
                    return null;
                }
                else
                {
                    return new LockingObservableLinkedListNode<T>(list, previousNode);
                }
            }
        }

        /// <summary>      
        /// The value contained in the node.
        /// </summary>
        public T Value { get => item; set => item = value; }

        /// <summary>
        /// Initializas a new instance of the <see cref="LockingObservableLinkedListNode{T}"/>  
        /// class containing the specific value.
        /// </summary>
        /// <param name="value">The value to contain in the <see cref="LockingObservableLinkedListNode{T}"/></param>
        public LockingObservableLinkedListNode(T value)
        {
            item = value;
        }

        internal LockingObservableLinkedListNode(LockingObservableLinkedList<T> list, LockingLinkedListNode<T> node)
        {
            this.list = list;
            item = node.Value;
            innerNode = node;
        }
        
        public void MoveBefore(LockingObservableLinkedListNode<T> node)
        {
            this.list.MoveBefore(this, node);
        }

        public void MoveAfter(LockingObservableLinkedListNode<T> node)
        {
            this.list.MoveAfter(this, node);
        }

        public void MoveUp()
        {
            this.list.MoveUp(this);
        }

        public void MoveDown()
        {
            this.list.MoveDown(this);
        }

        internal void Invalidate()
        {
            list = null;
            next = null;
            prev = null;
        }
    }
}
