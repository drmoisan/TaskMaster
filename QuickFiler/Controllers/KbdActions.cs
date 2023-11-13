using QuickFiler.Interfaces;
using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    public class KbdActions<TKey, UClass, VDelegate> : IEnumerable<UClass> where UClass : IKbdAction<TKey, VDelegate>, new()
    {
        public KbdActions()
        {
            _list = new ConcurrentObservableCollection<UClass>();
        }

        public KbdActions(IEnumerable<UClass> list)
        {
            _list = new ConcurrentObservableCollection<UClass>(list);
        }

        private ConcurrentObservableCollection<UClass> _list = new();

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
                    var message = $"Multiple sources have registered actions for Key {key}. SourceId list ";
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
                    var message = $"Multiple sources have registered actions for Key {key}. SourceId list ";
                    message += $"[{matches.Select(x => x.SourceId).SentenceJoin()}]";
                    throw new InvalidOperationException(message);
            }
        }

        public void Add(string sourceId, TKey key, VDelegate @delegate)
        {
            if (_list.Any(x => x.SourceId == sourceId && x.KeyEquals(key)))
            {
                string message = $"Cannot add key because it already exists. Key {key} SourceId {sourceId}";
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
            if (_list.Any(x => x.SourceId == instance.SourceId && x.KeyEquals(instance.Key)))
            {
                string message = $"Cannot add key because it already exists. Key {instance.Key} SourceId {instance.SourceId}";
                throw new ArgumentException(message);
            }
            _list.Add(instance);
        }

        public bool Remove(string sourceId, TKey key)
        {
            var index = _list.FindIndex(x => x.SourceId == sourceId && x.KeyEquals(key));
            if (index == -1) { return false; }
            else
            {
                _list.RemoveAt(index);
                return true;
            }
        }

        public IEnumerator<UClass> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public ICollection<TKey> Keys { get => _list.Select(x => x.Key).ToList(); }
    }

}
