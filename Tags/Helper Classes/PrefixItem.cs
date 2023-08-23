using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;

namespace Tags
{
    internal class PrefixItem : IPrefix
    {
        public PrefixItem(string key, string value, OlCategoryColor color)
        {
            _key = key;
            Value = value;
            Color = color;
        }

        private string _key;
        public string Key { get => _key; set => _key = value; }

        private string _value;
        public string Value { get => _value; set => _value = value; }

        private OlCategoryColor _color;
        public OlCategoryColor Color { get => _color; set => _color = value; }
    }

}
