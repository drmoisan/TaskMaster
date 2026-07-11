using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;

namespace UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable
{
    /// <summary>
    /// A vendored-dependency-free serializable stack built on <see cref="SloLinkedList{T}"/>. It is the clean
    /// replacement for the former <c>ScoStack&lt;T&gt;</c> and provides the positional/stack surface
    /// that the QuickFiler and SortEmail undo loops depend on, with <b>top-of-stack == index 0 ==
    /// <see cref="SloLinkedList{T}"/> front</b>.
    ///
    /// <para><see cref="Push(T)"/> maps to <c>AddFirst</c>, <see cref="Pop()"/> to <c>TakeFirst</c>,
    /// and <see cref="Peek()"/> to <c>First.Value</c> (all O(1)). The indexed members
    /// (<see cref="this[int]"/>, <see cref="Peek(int)"/>, <see cref="Pop(int)"/>) walk the linked
    /// list (O(n)); a positional <see cref="Pop(int)"/> removes and returns the element at the
    /// ordinal and shifts higher indices down.</para>
    ///
    /// <para>Serialization re-exposes a typed <see cref="ISmartSerializable{T}"/> surface bound to
    /// <c>SloStack&lt;T&gt;</c>. The four intentionally-unimplemented <see cref="SloLinkedList{T}"/>
    /// members remain stubbed (they are not on the MovedMails file-based deserialize path).</para>
    /// </summary>
    public class SloStack<T> : SloLinkedList<T>, ISmartSerializable<SloStack<T>>
    {
        #region ctor

        public SloStack()
            : base()
        {
            ism = new SmartSerializable<SloStack<T>>(this);
        }

        public SloStack(IEnumerable<T> collection)
            : base(collection)
        {
            ism = new SmartSerializable<SloStack<T>>(this);
        }

        #endregion ctor

        #region ISmartSerializable (typed to SloStack<T>)

        protected new SmartSerializable<SloStack<T>> ism;

        public new NewSmartSerializableConfig Config
        {
            get => ism.Config;
            set => ism.Config = value;
        }

        public new void Serialize() => ism.Serialize();

        public new void Serialize(string filePath) => ism.Serialize(filePath);

        public new void SerializeThreadSafe(string filePath) => ism.SerializeThreadSafe(filePath);

        /// <summary>Async wrapper over the synchronous <see cref="Serialize()"/>; persists the same payload.</summary>
        public async Task SerializeAsync()
        {
            Serialize();
            await Task.CompletedTask;
        }

        public new SloStack<T> Deserialize(string fileName, string folderPath) =>
            ism.Deserialize(fileName, folderPath);

        public new SloStack<T> Deserialize(
            string fileName,
            string folderPath,
            bool askUserOnError
        ) => ism.Deserialize(fileName, folderPath, askUserOnError);

        public new SloStack<T> Deserialize(
            string fileName,
            string folderPath,
            bool askUserOnError,
            JsonSerializerSettings settings
        ) => ism.Deserialize(fileName, folderPath, askUserOnError, settings);

        public new async Task<SloStack<T>> DeserializeAsync<U>(SmartSerializable<U> config)
            where U : class, ISmartSerializable<U>, new() => await ism.DeserializeAsync(config);

        public new async Task<SloStack<T>> DeserializeAsync<U>(
            SmartSerializable<U> config,
            bool askUserOnError
        )
            where U : class, ISmartSerializable<U>, new() =>
            await ism.DeserializeAsync(config, askUserOnError);

        #region Not Implemented Yet (mirrors SloLinkedList stubs — off the MovedMails path)

        SloStack<T> ISmartSerializable<SloStack<T>>.Deserialize<U>(SmartSerializable<U> loader)
        {
            throw new NotImplementedException();
        }

        SloStack<T> ISmartSerializable<SloStack<T>>.Deserialize<U>(
            SmartSerializable<U> loader,
            bool askUserOnError,
            Func<SloStack<T>> altLoader
        )
        {
            throw new NotImplementedException();
        }

        Task<SloStack<T>> ISmartSerializable<SloStack<T>>.DeserializeAsync<U>(
            SmartSerializable<U> config,
            bool askUserOnError,
            Func<SloStack<T>> altLoader
        )
        {
            throw new NotImplementedException();
        }

        public new SloStack<T> DeserializeObject(string json, JsonSerializerSettings settings)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Yet

        #endregion ISmartSerializable

        #region Stack (positional surface — top == index 0 == front)

        /// <summary>Pushes <paramref name="item"/> onto the top of the stack (front, O(1)).</summary>
        public void Push(T item) => AddFirst(item);

        /// <summary>Removes and returns the top element (front, O(1)).</summary>
        public T Pop()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack is empty. Cannot pop an element");
            }
            return TakeFirst();
        }

        /// <summary>Removes and returns the element at <paramref name="index"/>, shifting higher indices down (O(n)).</summary>
        public T Pop(int index)
        {
            var node = NodeAt(index);
            T value = node.Value;
            Remove(node);
            return value;
        }

        /// <summary>Returns the top element without removing it (front, O(1)).</summary>
        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack is empty. No element to peek at");
            }
            return First.Value;
        }

        /// <summary>Returns the element at <paramref name="index"/> without removing it (O(n)).</summary>
        public T Peek(int index) => NodeAt(index).Value;

        /// <summary>Gets the element at <paramref name="index"/> (top == index 0, O(n)).</summary>
        public T this[int index] => NodeAt(index).Value;

        /// <summary>Tries to read the top element without removing it.</summary>
        public bool TryPeek(out T result)
        {
            if (Count == 0)
            {
                result = default;
                return false;
            }
            result = First.Value;
            return true;
        }

        /// <summary>Tries to read the element at <paramref name="index"/> without removing it.</summary>
        public bool TryPeek(out T result, int index)
        {
            if (index < 0 || index >= Count)
            {
                result = default;
                return false;
            }
            result = NodeAt(index).Value;
            return true;
        }

        /// <summary>Tries to remove and return the top element.</summary>
        public bool TryPop(out T result)
        {
            if (Count == 0)
            {
                result = default;
                return false;
            }
            result = TakeFirst();
            return true;
        }

        /// <summary>Tries to remove and return the element at <paramref name="index"/>, shifting higher indices down.</summary>
        public bool TryPop(out T result, int index)
        {
            if (index < 0 || index >= Count)
            {
                result = default;
                return false;
            }
            var node = NodeAt(index);
            result = node.Value;
            Remove(node);
            return true;
        }

        /// <summary>Walks the linked list to the node at <paramref name="index"/> (top == index 0).</summary>
        private LockingObservableLinkedListNode<T> NodeAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} out of range. Stack only has {Count} elements."
                );
            }
            var node = First;
            for (int i = 0; i < index; i++)
            {
                node = node.Next;
            }
            return node;
        }

        #endregion Stack

        #region Static (file-based deserialize used by LoadMovedMails)

        public static new class Static
        {
            private static SmartSerializable<SloStack<T>> GetInstance() => new();

            public static SloStack<T> Deserialize(string fileName, string folderPath) =>
                GetInstance().Deserialize(fileName, folderPath);

            public static SloStack<T> Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError
            ) => GetInstance().Deserialize(fileName, folderPath, askUserOnError);

            public static SloStack<T> Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError,
                JsonSerializerSettings settings
            ) => GetInstance().Deserialize(fileName, folderPath, askUserOnError, settings);
        }

        #endregion Static
    }
}
