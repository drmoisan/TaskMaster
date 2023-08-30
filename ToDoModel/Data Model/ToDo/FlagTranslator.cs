using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoModel
{
    public class FlagTranslator
    {
        public FlagTranslator() { }

        public FlagTranslator(Func<string> getStrFunc,
                              Action<string> setStrFunc,
                              Func<ObservableCollection<string>> getListFunc,
                              Action<ObservableCollection<string>> setListFunc) 
        {
            _getStrFunc = getStrFunc ?? throw new ArgumentNullException(nameof(getStrFunc));
            _setStrFunc = setStrFunc ?? throw new ArgumentNullException(nameof(setStrFunc));
            _getListFunc = getListFunc ?? throw new ArgumentNullException(nameof(getListFunc));
            _setListFunc = setListFunc ?? throw new ArgumentNullException(nameof(setListFunc));
        }

        private Func<string> _getStrFunc;
        public Func<string> GetStrFunc { get => _getStrFunc; set => _getStrFunc = value; }
        
        private Action<string> _setStrFunc;
        public Action<string> SetStrFunc { get => _setStrFunc; set => _setStrFunc = value; }
        
        private Func<ObservableCollection<string>> _getListFunc;
        public Func<ObservableCollection<string>> GetListFunc { get => _getListFunc; set => _getListFunc = value; }
        
        private Action<ObservableCollection<string>> _setListFunc;
        public Action<ObservableCollection<string>> SetListFunc { get => _setListFunc; set => _setListFunc = value; }

        public string AsStringWithPrefix { get => _getStrFunc(); set => _setStrFunc(value); }
        public string AsStringNoPrefix { get => _getStrFunc(); set => _setStrFunc(value); }
        public ObservableCollection<string> AsListWithPrefix { get => _getListFunc(); set => _setListFunc(value); }
        public ObservableCollection<string> AsListNoPrefix { get => _getListFunc(); set => _setListFunc(value); }

    }
}
