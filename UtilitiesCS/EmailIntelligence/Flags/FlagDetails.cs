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
//using Microsoft.VisualStudio.Services.Common;
using Swordfish.NET.Collections;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public class FlagDetails : INotifyCollectionChanged, ICloneable
    {
        #region Constructors and Initializers

        public FlagDetails() 
        { 
            Subscribe();
            SubscribeWithPrefix();
        }

        public FlagDetails(string prefix) 
        { 
            this.Prefix = prefix; 
            Subscribe();
            SubscribeWithPrefix();
        }

        #endregion

        #region Public Properties

        private ObservableCollection<string> _list = new();
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
                    if (value.Count > 0 && value[0].Length >= Prefix.Length && value[0].Substring(0, Prefix.Length) == Prefix)
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
                        var oldValue = _list;
                        _list = value;
                        Subscribe();
                        List_CollectionChanged(_list, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                }
            }
        }

        private ObservableCollection<string> _listWithPrefix = new();
        public ObservableCollection<string> ListWithPrefix { get => _listWithPrefix; }
                                
        private string _withPrefix = "";
        public string WithPrefix { get => _withPrefix; }
        
        private string _noPrefix = "";
        public string NoPrefix { get => _noPrefix; }
        
        private string _prefix = "";
        public string Prefix { get => _prefix; set => _prefix = value; }

        #endregion

        #region INotifyCollectionChanged

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

        #endregion

        #region IClonable

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public FlagDetails DeepCopy()
        {
            var clone = (FlagDetails)MemberwiseClone();
            clone.Unsubscribe();
            clone.UnsubscribeWithPrefix();
            clone._list = new ObservableCollection<string>(_list);
            clone._listWithPrefix = new ObservableCollection<string>(_listWithPrefix);
            clone.Subscribe();
            clone.SubscribeWithPrefix();
            return clone;
        }

        #endregion IClonable

    }
}
