using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler.Controllers
{
    public class KaKey : IKbdAction<Keys, Action<Keys>>
    {
        public KaKey() { }

        public KaKey(string sourceId, Keys key, Action<Keys> action)
        {
            SourceId = sourceId;
            Key = key;
            Delegate = action;
        }

        private string _sourceId;
        public string SourceId { get => _sourceId; set => _sourceId = value; }

        private Keys _key;
        public Keys Key { get => _key; set => _key = value; }

        private Action<Keys> _action;
        public Action<Keys> Delegate { get => _action; set => _action = value; }

        public Type DelegateType { get => typeof(Action<Keys>); }

        public bool KeyEquals(Keys other) => Key == other;

        private Action<string> _update;
        public Action<string> Update { get => _update; set => _update = value; }
    }

    public class KaKeyAsync : IKbdAction<Keys, Func<Keys, Task>>
    {
        public KaKeyAsync() { }

        public KaKeyAsync(string sourceId, Keys key, Func<Keys, Task> function)
        {
            SourceId = sourceId;
            Key = key;
            Delegate = function;
        }

        private string _sourceId;
        public string SourceId { get => _sourceId; set => _sourceId = value; }

        private Keys _key;
        public Keys Key { get => _key; set => _key = value; }

        private Func<Keys, Task> _function;
        public Func<Keys, Task> Delegate { get => _function; set => _function = value; }

        public bool KeyEquals(Keys other) => Key == other;

        private Action<string> _update;
        public Action<string> Update { get => _update; set => _update = value; }
    }

    

}
