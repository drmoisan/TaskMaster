using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using UtilitiesCS;

namespace TaskVisualization
{
    [ExcludeFromCodeCoverage]
    public partial class ManageFilters : Form, IManageFiltersViewer
    {
        public ManageFilters()
        {
            InitializeComponent();
        }

        private ManageFiltersController _controller;

        /// <summary>
        /// Preserved public surface (invariant 2): builds the host-neutral
        /// <see cref="ManageFiltersController"/> and delegates load to it.
        /// </summary>
        public void LoadFilters(IApplicationGlobals globals)
        {
            _controller = new ManageFiltersController(this, globals);
            _controller.LoadFilters();
        }

        #region IManageFiltersViewer pass-throughs

        public FilterEntry SelectedFilter => (FilterEntry)FiltersOlv.SelectedItem.RowObject;

        public void SetFilters(IEnumerable<FilterEntry> filters)
        {
            FiltersOlv.SetObjects(filters);
        }

        public void RebuildList()
        {
            FiltersOlv.BuildList();
        }

        public event EventHandler AddFilterClick
        {
            add => BtnAddFilter.Click += value;
            remove => BtnAddFilter.Click -= value;
        }

        public event EventHandler EditFilterClick
        {
            add => BtnEditFilter.Click += value;
            remove => BtnEditFilter.Click -= value;
        }

        public event EventHandler DeleteClick
        {
            add => BtnDelete.Click += value;
            remove => BtnDelete.Click -= value;
        }

        #endregion IManageFiltersViewer pass-throughs

        #region Designer-wired handlers (delegate to controller)

        private void BtnEditFilter_Click(object sender, EventArgs e)
        {
            _controller.EditSelected();
        }

        private void BtnAddFilter_Click(object sender, EventArgs e)
        {
            _controller.AddFilter();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            _controller.DeleteSelected();
        }

        #endregion Designer-wired handlers (delegate to controller)
    }
}
