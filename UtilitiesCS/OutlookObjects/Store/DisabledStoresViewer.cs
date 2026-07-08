using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// WinForms viewer for the disabled-stores dialog (issue #265). Thin shell over
    /// <see cref="DisabledStoresController"/>: wires the grid click to the controller and
    /// implements the <see cref="BindRows"/> seam by assigning the grid data source. Form-derived
    /// and Designer-generated code; exempt from the coverage floor per the repository
    /// COM/VSTO/WinForms exemption.
    /// </summary>
    public partial class DisabledStoresViewer : Form, IDisabledStoresViewer
    {
        public DisabledStoresViewer(DisabledStoresController controller)
        {
            InitializeComponent();
            Controller = controller;
            Dgv.CellContentClick += (s, e) => Controller.Dgv_CellContentClick(s, e);
        }

        public DisabledStoresController Controller { get; set; }

        #region Make testable

        public DataGridView Dgv
        {
            get => _dgv;
            set => _dgv = value;
        }

        #endregion Make testable

        /// <summary>
        /// Binds the supplied rows for display by assigning the grid data source. This is the only
        /// place a live <see cref="DataGridView.DataSource"/> is written, keeping that write inside
        /// WinForms-exempt code.
        /// </summary>
        public void BindRows(IList<DisabledStoreRow> rows)
        {
            Dgv.DataSource = new BindingList<DisabledStoreRow>(new List<DisabledStoreRow>(rows));
        }

        /// <summary>Forwards a grid cell-content click to the controller.</summary>
        public void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Controller?.Dgv_CellContentClick(sender, e);
        }
    }
}
