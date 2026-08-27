using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    public class KbdActions<TKey, UClass, VDelegate> : IEnumerable<UClass>
        where UClass : IKbdAction<TKey, VDelegate>, new()
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public KbdActions()
        {
            _list = new List<UClass>();
        }

        /// <summary>
        /// Seeds the registry from <paramref name="list"/>, enforcing the same
        /// (SourceId, stored Key) uniqueness invariant both <c>Add</c> overloads enforce.
        /// </summary>
        /// <param name="list">
        /// The seed sequence. Materialised first and enumerated exactly once, so a null argument
        /// still produces <see cref="ArgumentNullException"/> rather than
        /// <see cref="NullReferenceException"/>, and a one-shot sequence is not consumed twice.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when two or more elements share a <c>SourceId</c> and a
        /// <see cref="StoredKeyEquals"/>-equal <c>Key</c>. Comparison uses
        /// <see cref="StoredKeyEquals"/>, never the element-defined <c>KeyEquals</c>: the latter may
        /// match on a substring and carry observable side effects, so it would both reject legally
        /// coexisting keys and fire those side effects during construction.
        /// </exception>
        public KbdActions(IEnumerable<UClass> list)
        {
            _list = new List<UClass>(list);

            // O(n^2) over the seed, consistent with Add's existing _list.Any(...) scan. Seed lists
            // in this repository hold at most eight entries, so a hash set would be premature and
            // would require an IEqualityComparer<TKey>.
            for (int i = 0; i < _list.Count; i++)
            {
                for (int j = i + 1; j < _list.Count; j++)
                {
                    if (
                        _list[i].SourceId == _list[j].SourceId
                        && StoredKeyEquals(_list[i].Key, _list[j].Key)
                    )
                    {
                        string message =
                            $"Cannot add key because it already exists. Key {_list[j].Key} SourceId {_list[j].SourceId}";
                        logger.Error(message);
                        throw new ArgumentException(message, nameof(list));
                    }
                }
            }
        }

        private List<UClass> _list = new();

        private static bool StoredKeyEquals(TKey left, TKey right) =>
            EqualityComparer<TKey>.Default.Equals(left, right);

        public VDelegate this[TKey key]
        {
            get => this.Find(key).Delegate;
            set
            {
                var element = this.Find(key);
                if (element is not null)
                {
                    element.Delegate = value;
                }
            }
        }

        public bool ContainsKey(TKey key) => _list.Any(x => x.KeyEquals(key));

        public UClass[] FilterKeys(TKey key) => _list.Where(x => x.KeyEquals(key)).ToArray();

        public UClass Find(TKey key)
        {
            var matches = _list.Where(x => x.KeyEquals(key));
            var count = matches.Count();
            switch (count)
            {
                case 0:
                    return default(UClass);
                case 1:
                    return matches.First();
                default:
                    var message =
                        $"Multiple sources have registered actions for Key {key}. SourceId list ";
                    message += $"[{matches.Select(x => x.SourceId).SentenceJoin()}]";
                    throw new InvalidOperationException(message);
            }
        }

        public int FindIndex(TKey key)
        {
            var matches = _list.Where(x => x.KeyEquals(key));
            var count = matches.Count();
            switch (count)
            {
                case 0:
                    return -1;
                case 1:
                    return _list.FindIndex(x => x.KeyEquals(key));
                default:
                    var message =
                        $"Multiple sources have registered actions for Key {key}. SourceId list ";
                    message += $"[{matches.Select(x => x.SourceId).SentenceJoin()}]";
                    logger.Error(message);
                    throw new InvalidOperationException(message);
            }
        }

        public void Add(string sourceId, TKey key, VDelegate @delegate)
        {
            if (_list.Any(x => x.SourceId == sourceId && StoredKeyEquals(x.Key, key)))
            {
                string message =
                    $"Cannot add key because it already exists. Key {key} SourceId {sourceId}";
                logger.Error(message);
                throw new ArgumentException(message);
            }
            UClass instance = new();
            instance.SourceId = sourceId;
            instance.Key = key;
            instance.Delegate = @delegate;
            _list.Add(instance);
        }

        public void Add(UClass instance)
        {
            if (
                _list.Any(x =>
                    x.SourceId == instance.SourceId && StoredKeyEquals(x.Key, instance.Key)
                )
            )
            {
                string message =
                    $"Cannot add key because it already exists. Key {instance.Key} SourceId {instance.SourceId}";

                logger.Error(message);
                throw new ArgumentException(message, nameof(instance));
            }
            _list.Add(instance);
        }

        public bool Remove(string sourceId, TKey key)
        {
            var index = _list.FindIndex(x => x.SourceId == sourceId && StoredKeyEquals(x.Key, key));
            if (index == -1)
            {
                return false;
            }
            else
            {
                _list.RemoveAt(index);
                return true;
            }
        }

        public IEnumerator<UClass> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public ICollection<TKey> Keys
        {
            get => _list.Select(x => x.Key).ToList();
        }
    }
}
