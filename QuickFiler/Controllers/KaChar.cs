using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler.Controllers
{
    public class KaChar : IKbdAction<char, Action<char>>
    {
        public KaChar() { }

        public KaChar(string sourceId, char key, Action<char> action)
        {
            SourceId = sourceId;
            Key = key;
            Delegate = action;
        }

        private string _sourceId;
        public string SourceId { get => _sourceId; set => _sourceId = value; }

        private char _key;
        public char Key { get => _key; set => _key = value; }

        private Action<char> _action;
        public Action<char> Delegate { get => _action; set => _action = value; }

        public Type DelegateType { get => typeof(Action<Keys>); }

        public bool KeyEquals(char other) => Key == other;

        private Action<string> _update;
        public Action<string> Update { get => _update; set => _update = value; }
    }

    public class KaCharAsync : IKbdAction<char, Func<char, Task>>
    {
        public KaCharAsync() { }

        public KaCharAsync(string sourceId, char key, Func<char, Task> function)
        {
            SourceId = sourceId;
            Key = key;
            Delegate = function;
        }

        private string _sourceId;
        public string SourceId { get => _sourceId; set => _sourceId = value; }

        private char _key;
        public char Key { get => _key; set => _key = value; }

        private Func<char, Task> _function;
        public Func<char, Task> Delegate { get => _function; set => _function = value; }

        public bool KeyEquals(char other) => Key == other;

        private Action<string> _update;
        public Action<string> Update { get => _update; set => _update = value; }
    }

}
