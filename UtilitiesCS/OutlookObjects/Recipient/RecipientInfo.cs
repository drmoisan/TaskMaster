using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    [Serializable]
    public class RecipientInfo : IRecipientInfo
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

        public virtual string Name { get => _name; set => _name = value; }
        public virtual string Address { get => _address; set => _address = value; }
        public virtual string Html { get => _html; set => _html = value; }

        public bool Equals(IRecipientInfo other)
        {
            if (other == null) { return false; }
            else if (ReferenceEquals(this, other)) { return true; }
            else if (other.Name.IsNullOrEmpty() & other.Address.IsNullOrEmpty()) { return false; }
            else if (Name.IsNullOrEmpty() & Address.IsNullOrEmpty()) { return false; }
            else if ((Name ?? "") == (other.Name ?? "") && (Address ?? "") == (other.Address ?? "")) { return true; }
            else { return false; }
        }

        public override int GetHashCode()
        {
            // Use a simple hash code based on Name and Address
            return (Name ?? "").GetHashCode() * 31 + (Address ?? "").GetHashCode();
        }
    }
}
