using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class FlagTranslator
    {
        public FlagTranslator() { }

        public FlagTranslator(Func<bool, string> getStrFunc,
                              Action<bool, string> setStrFunc,
                              Func<bool, ObservableCollection<string>> getListFunc,
                              Action<bool, ObservableCollection<string>> setListFunc) 
        {
            _getStrFunc = getStrFunc ?? throw new ArgumentNullException(nameof(getStrFunc));
            _setStrFunc = setStrFunc ?? throw new ArgumentNullException(nameof(setStrFunc));
            _getListFunc = getListFunc ?? throw new ArgumentNullException(nameof(getListFunc));
            _setListFunc = setListFunc ?? throw new ArgumentNullException(nameof(setListFunc));
        }

        private Func<bool, string> _getStrFunc;
        public Func<bool, string> GetStrFunc { get => _getStrFunc; set => _getStrFunc = value; }
        
        private Action<bool, string> _setStrFunc;
        public Action<bool, string> SetStrFunc { get => _setStrFunc; set => _setStrFunc = value; }
        
        private Func<bool, ObservableCollection<string>> _getListFunc;
        public Func<bool, ObservableCollection<string>> GetListFunc { get => _getListFunc; set => _getListFunc = value; }
        
        private Action<bool, ObservableCollection<string>> _setListFunc;
        public Action<bool, ObservableCollection<string>> SetListFunc { get => _setListFunc; set => _setListFunc = value; }

        public new string ToString() => AsStringNoPrefix;
        public string AsString() => AsStringNoPrefix;

        public string AsStringWithPrefix { get => _getStrFunc(true); set => _setStrFunc(true, value); }
        public string AsStringNoPrefix { get => _getStrFunc(false); set => _setStrFunc(false, value); }
        public ObservableCollection<string> AsListWithPrefix { get => _getListFunc(true); set => _setListFunc(true, value); }
        public ObservableCollection<string> AsListNoPrefix { get => _getListFunc(false); set => _setListFunc(false, value); }

    }
}
