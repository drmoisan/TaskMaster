using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class FilterEntry: ICloneable
    {
        public FilterEntry() 
        {
            _name = "";
            _description = "";
            _folders = new();
            _flags = new("");
        }
        public FilterEntry(string name, List<string> folders, IList<string> categoryList)
        {
            _name = name;
            _folders = folders;
            _flags = new(categoryList);
        }
        public FilterEntry(string name, string description, List<string> folders, FlagClassNoItem flags)
        {
            _name = name;
            _description = description;
            _folders = folders;
            _flags = flags;
        }

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _description;
        public string Description { get => _description; set => _description = value; }
        
        private List<string> _folders;
        public List<string> Folders { get => _folders; set => _folders = value; }
        
        private FlagClassNoItem _flags;
        public FlagClassNoItem Flags { get => _flags; set => _flags = value; }

        public object Clone()
        {
            FilterEntry newFilterEntry = (FilterEntry)this.MemberwiseClone();
            newFilterEntry.Folders = this.Folders.ToList();
            newFilterEntry.Flags = (FlagClassNoItem)this.Flags.Clone();
            return newFilterEntry;
        }

        public void RevertToCopy(FilterEntry copy)
        {
            this.Name = copy.Name;
            this.Description = copy.Description;
            this.Folders = copy.Folders;
            this.Flags = copy.Flags;
        }
    }
}
