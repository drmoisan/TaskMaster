using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace TaskVisualization
{
    public partial class ManageFilters : Form
    {
        public ManageFilters()
        {
            InitializeComponent();
        }

        private IApplicationGlobals _globals;
        
        public void LoadFilters(IApplicationGlobals globals)
        {
            _globals = globals;
            this.FiltersOlv.SetObjects(_globals.AF.Filters);
        }

        private void BtnEditFilter_Click(object sender, EventArgs e)
        {
            var filterEntry = (FilterEntry)FiltersOlv.SelectedItem.RowObject;
            var editor = new EditFilterController(_globals, filterEntry);
        }

        private void BtnAddFilter_Click(object sender, EventArgs e)
        {
            var editor = new EditFilterController(_globals, EditFilterCallback);
            FiltersOlv.SetObjects(_globals.AF.Filters);
            FiltersOlv.BuildList();
        }
        
        private void EditFilterCallback(EditFilterController controller, FilterEntry filterEntry)
        {
            _globals.AF.Filters.Add(filterEntry);
            _globals.AF.Filters.Serialize();
            FiltersOlv.BuildList();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var filterEntry = (FilterEntry)FiltersOlv.SelectedItem.RowObject;
        }
    }
}
