using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoModel
{
    public class FlagDetails
    {
        public FlagDetails() { }

        public FlagDetails(string prefix) { this.Prefix = prefix; }

        private RestrictedList<string> _list;
        private string _prefix;
        private string _withPrefix;
        private string _noPrefix;

        public List<string> List
        {
            get
            {
                return _list;
            }
            set
            {
                List<string> TmpList;
                if (value is null)
                {
                    TmpList = new List<string>();
                }
                else if (value.Count == 0)
                {
                    TmpList = value;
                }
                else if (value[0].Length < Prefix.Length)
                {
                    TmpList = value;
                }
                else if ((value[0].Substring(0, Prefix.Length) ?? "") == (Prefix ?? ""))
                {
                    TmpList = value.Select(x => x.Replace(Prefix, "")).ToList();
                }
                else
                {
                    TmpList = value;
                }
                _list = new RestrictedList<string>(TmpList, this);
                ListChange_Refresh();
            }
        }

        public List<string> ListWithPrefix
        {
            get
            {
                return _list.Select(x => Prefix + x).ToList();
            }
        }
                
        public string WithPrefix { get => _withPrefix; }
        public string NoPrefix { get => _noPrefix; }
        public string Prefix { get => _prefix; set => _prefix = value; }

        private void ListChange_Refresh()
        {
            _withPrefix = string.Join(", ", _list.Select(x => Prefix + x));
            _noPrefix = string.Join(", ", _list);
        }

        private sealed class RestrictedList<T> : List<T>
        {
            public RestrictedList(List<T> wrapped_list, FlagDetails outer) : base(wrapped_list)
            {
                if (wrapped_list is null)
                {
                    throw new ArgumentNullException("wrapped_list");
                }
                this.outer = outer;
            }

            private readonly FlagDetails outer;

            public new void Add(T item)
            {
                base.Add(item);
                outer.ListChange_Refresh();
            }

            public new void Remove(T item)
            {
                base.Remove(item);
                outer.ListChange_Refresh();
            }

            // Public Function ToClonedList() As List(Of T)
            // Dim ClonedList As List(Of T) = TryCast(Me.Clone(), List(Of T))
            // Return ClonedList
            // End Function

            // Private Function Clone() As Object Implements ICloneable.Clone
            // Return MyBase.MemberwiseClone()
            // End Function
        }

    }
}
