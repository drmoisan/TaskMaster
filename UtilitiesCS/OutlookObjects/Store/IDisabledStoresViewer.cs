using System.Collections.Generic;
using System.Windows.Forms;
using UtilitiesCS.Interfaces.IWinForm;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Viewer seam for the disabled-stores dialog (issue #265). Extends the shared
    /// <see cref="IForm"/> mirror (as <c>IStoreWrapperViewer</c> does) so the controller can
    /// marshal via <c>InvokeRequired</c>/<c>Invoke</c> and show the dialog without a concrete
    /// Form reference. <see cref="BindRows"/> is the mockable binding seam that keeps the
    /// live <see cref="DataGridView"/> data-source write inside WinForms-exempt code, so all
    /// controller logic is verifiable with a Moq implementation and no live grid.
    /// </summary>
    internal interface IDisabledStoresViewer : IForm
    {
        /// <summary>The grid control; used by the viewer's own event wiring and Designer access.</summary>
        DataGridView Dgv { get; set; }

        /// <summary>
        /// Binds the supplied rows for display. The concrete viewer assigns
        /// <c>Dgv.DataSource</c>; the controller only calls this seam.
        /// </summary>
        /// <param name="rows">The authoritative rows to display.</param>
        void BindRows(IList<DisabledStoreRow> rows);

        /// <summary>Forwards a grid cell-content click to the controller's click handler.</summary>
        void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e);
    }
}
