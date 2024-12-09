using System.Collections.Generic;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.ReusableTypeClasses
{
    public class LockingLinkedListNode<T>
    {
        #region internal properties

        internal LockingLinkedList<T> list;

        internal LockingLinkedListNode<T> next;

        internal LockingLinkedListNode<T> prev;

        internal T item;

        internal LinkedListNode<T> innerNode;

        #endregion internal properties

        /// <summary>
        /// Gets a reference to the <see cref="LockingObservableLinkedList{T}"/> 
        /// that contains the <see cref="LockingObservableLinkedListNode{T}"/>.
        /// </summary>
        public LockingLinkedList<T> List => list;

        /// <summary>
        /// Gets a reference to the next node in the <see cref="LockingObservableLinkedListNode{T}"/>,
        /// or null if the current node is the last element
        /// of the <see cref="LockingObservableLinkedListNode{T}"/>.
        /// </summary>
        public LockingLinkedListNode<T> Next
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
                    return new LockingLinkedListNode<T>(list, nextNode);
                }
            }
        }

        /// <summary>
        /// Gets a reference to the previous node in the <see cref="LockingObservableLinkedListNode{T}"/>,
        /// or null if the current node is the first element
        /// of the <see cref="LockingObservableLinkedListNode{T}"/>.
        /// </summary>
        public LockingLinkedListNode<T> Previous
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
                    return new LockingLinkedListNode<T>(list, previousNode);
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
        public LockingLinkedListNode(T value)
        {
            item = value;
        }

        internal LockingLinkedListNode(LockingLinkedList<T> list, LinkedListNode<T> node)
        {
            this.list = list;
            item = node.Value;
            innerNode = node;
        }

        public void MoveAfter(LockingLinkedListNode<T> node)
        {
            this.list.MoveAfter(this, node);
        }

        public void MoveBefore(LockingLinkedListNode<T> node)
        {
            this.list.MoveBefore(this, node);
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
            innerNode = null;
        }
    }
}
