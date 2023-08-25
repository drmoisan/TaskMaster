using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.Common;
using Swordfish.NET.Collections;
using UtilitiesCS.ReusableTypeClasses;

namespace ToDoModel
{
    public class FlagDetails : INotifyCollectionChanged
    {
        public FlagDetails() { }

        public FlagDetails(string prefix) { this.Prefix = prefix; }

        private ObservableCollection<string> _list = new();
        private ObservableCollection<string> _listWithPrefix = new();

        private string _prefix;
        private string _withPrefix;
        private string _noPrefix;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Subscribe()
        {
            if (_list is not null)
            {
                _list.CollectionChanged += List_CollectionChanged;
            }
        }

        public void Unsubscribe()
        {
            if (_list is not null)
            {
                _list.CollectionChanged -= List_CollectionChanged;
            }
        }

        public ObservableCollection<string> List
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _list;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (value is null) { _list.Clear(); }
                else
                {
                    IEnumerable<string> temp;
                    if (value.Count > 0 && value[0].Substring(0, Prefix.Length) == Prefix)
                    {
                        temp = value.Select(x => x.Replace(Prefix, ""));
                    }
                    else
                    {
                        temp = value;
                    }
                    if (!_list.SequenceEqual(temp))
                    {
                        Unsubscribe();
                        _list = value;
                        Subscribe();
                        List_CollectionChanged(_list, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                }
            }
        }

        public void SubscribeWithPrefix()
        {
            if (_listWithPrefix is not null)
            {
                _listWithPrefix.CollectionChanged += ListWithPrefix_CollectionChanged;
            }
        }

        public void UnsubscribeWithPrefix()
        {
            if (_listWithPrefix is not null)
            {
                _listWithPrefix.CollectionChanged -= ListWithPrefix_CollectionChanged;
            }
        }

        public ObservableCollection<string> ListWithPrefix { get => _listWithPrefix; }
                                
        public string WithPrefix { get => _withPrefix; }
        public string NoPrefix { get => _noPrefix; }
        public string Prefix { get => _prefix; set => _prefix = value; }

        private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UnsubscribeWithPrefix();
            _listWithPrefix = new ObservableCollection<string>(_list.Select(x => $"{Prefix}{x}"));
            _withPrefix = string.Join(", ", _listWithPrefix);
            _noPrefix = string.Join(", ", _list);
            SubscribeWithPrefix();
            CollectionChanged?.Invoke(this, e);
        }

        private void ListWithPrefix_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Unsubscribe();
            _list = new ObservableCollection<string>(_listWithPrefix.Select(x => x.Replace(Prefix, "")));
            _withPrefix = string.Join(", ", _listWithPrefix);
            _noPrefix = string.Join(", ", _list);
            Subscribe();
            CollectionChanged?.Invoke(this, e);
        }

        

        //private void ListChange_Refresh()
        //{
        //    _withPrefix = string.Join(", ", _list.Select(x => Prefix + x));
        //    _noPrefix = string.Join(", ", _list);
        //}

        //private sealed class RestrictedList<T> : INotifyCollectionChanged
        //{
        //    public RestrictedList(List<T> wrappedList, FlagDetails outer) 
        //    {
        //        _wrappedList = wrappedList ?? throw new ArgumentNullException(nameof(wrappedList));
        //        _outer = outer;
        //    }

        //    private List<T> _wrappedList;
        //    FlagDetails _outer;

        //    public event NotifyCollectionChangedEventHandler CollectionChanged;

        //    public void Add(T item)
        //    {
        //        _wrappedList.Add(item);
        //        _outer.ListChange_Refresh();
        //    }

        //    public void Remove(T item)
        //    {
        //        _wrappedList.Remove(item);
        //        _outer.ListChange_Refresh();
        //    }

        //}

    }
}
