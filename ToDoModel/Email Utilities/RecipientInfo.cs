using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoModel
{
    public class RecipientInfo
    {
        public RecipientInfo() { }

        public RecipientInfo(string name, string address, string html)
        {
            _name = name;
            _address = address;
            _html = html;
        }

        private string _name;
        private string _address;
        private string _html;

        public string Name { get => _name; set => _name = value; }
        public string Address { get => _address; set => _address = value; }
        public string Html { get => _html; set => _html = value; }
    }
}
