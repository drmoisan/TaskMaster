using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Controller for the disabled-stores settings dialog (issue #265). Owns the authoritative
    /// in-memory row list, populates it from F1's <see cref="IStoreDisableService"/>, resolves
    /// per-row Reenable clicks by <c>RowIndex</c> against its own list (never the live grid), and
    /// re-fetches the true state after every reenable. All decision logic is verifiable behind the
    /// <see cref="IDisabledStoresViewer"/> seam with Moq and no live <see cref="DataGridView"/>.
    /// </summary>
    public class DisabledStoresController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>Creates a controller bound to the supplied application globals.</summary>
        /// <param name="globals">The application globals exposing the F1 <c>StoreDisable</c> service.</param>
        public DisabledStoresController(IApplicationGlobals globals)
        {
            Globals = globals;
        }

        internal IApplicationGlobals Globals { get; set; }

        /// <summary>
        /// The viewer seam; set when the dialog is launched. Declared <c>internal</c> (not
        /// <c>public</c>) because its type <see cref="IDisabledStoresViewer"/> is an internal seam
        /// — a public property cannot expose an internal type (CS0053). Nothing outside the
        /// assembly consumes it; the ribbon entry point uses only the public <see cref="Launch"/>.
        /// </summary>
        internal IDisabledStoresViewer Viewer { get; set; }

        /// <summary>The authoritative row list bound to the grid for display.</summary>
        internal List<DisabledStoreRow> Rows { get; set; } = new();

        /// <summary>Re-entrancy guard so a second click cannot start an overlapping reenable.</summary>
        internal bool ReenableInFlight { get; set; }

        /// <summary>
        /// The grid column index of the Reenable button, matching the Designer column order
        /// (0 = DisplayName, 1 = ScopeLabel, 2 = Reenable).
        /// </summary>
        internal int ReenableColumnIndex { get; set; } = 2;

        /// <summary>
        /// Fetches the disabled stores from F1 and projects each entry into a
        /// <see cref="DisabledStoreRow"/>, assigns the projection to <see cref="Rows"/>, and binds
        /// it through the viewer seam. Does not touch a live grid. An empty service result yields an
        /// empty <see cref="Rows"/> list with no special-case branch.
        /// </summary>
        internal void PopulateRows()
        {
            IReadOnlyCollection<DisabledStoreEntry> entries =
                Globals.StoreDisable.GetDisabledStores();
            var rows = new List<DisabledStoreRow>();
            foreach (var entry in entries)
            {
                bool isFutureSession = entry.Scope == DisableScope.FutureSessions;
                rows.Add(
                    new DisabledStoreRow
                    {
                        Identity = entry.Identity,
                        DisplayName = entry.Identity.Value,
                        IsFutureSession = isFutureSession,
                        ScopeLabel = isFutureSession ? "Future Sessions" : "Session Only",
                    }
                );
            }

            Rows = rows;
            Viewer.BindRows(Rows);
        }

        /// <summary>
        /// Handles a grid cell-content click. Returns early for header/invalid rows, non-Reenable
        /// columns, or out-of-range indices; otherwise resolves the clicked row from
        /// <see cref="Rows"/> by <c>e.RowIndex</c> and starts the reenable path.
        /// </summary>
        public void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            if (e.ColumnIndex != ReenableColumnIndex)
            {
                return;
            }
            if (e.RowIndex >= Rows.Count)
            {
                return;
            }

            var row = Rows[e.RowIndex];
            // Fire-and-forget from the synchronous WinForms event; ReenableInFlight guards
            // against overlapping in-flight reenables on rapid double-clicks.
            _ = ReenableAsync(row);
        }

        /// <summary>
        /// Reenables the supplied row through F1's <c>StoreDisable.ReenableAsync</c>, then
        /// unconditionally re-fetches the disabled-store state. On failure the exception is logged
        /// and surfaced through the <see cref="MyBox"/> dialog seam without escaping the method; the
        /// list is refreshed on both the success and failure paths so the displayed state cannot
        /// drift from the service.
        /// </summary>
        /// <param name="row">The row whose store should be reenabled.</param>
        internal async Task ReenableAsync(DisabledStoreRow row)
        {
            if (ReenableInFlight)
            {
                return;
            }

            ReenableInFlight = true;
            try
            {
                await Globals.StoreDisable.ReenableAsync(row.Identity);
            }
            catch (Exception e)
            {
                logger.Error($"Error reenabling store '{row?.DisplayName}'. {e.Message}", e);
                MyBox.ShowDialog(
                    "The store could not be reenabled. Please try again.",
                    "Reenable Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            finally
            {
                // Re-fetch the true state on the UI thread, marshaling through the viewer's
                // InvokeRequired/Invoke convention before touching viewer state.
                if (Viewer != null && Viewer.InvokeRequired)
                {
                    Viewer.Invoke(() => PopulateRows());
                }
                else
                {
                    PopulateRows();
                }

                ReenableInFlight = false;
            }
        }

        /// <summary>
        /// Opens the disabled-stores dialog. Applies the shared readiness gate; when the model is
        /// not ready it shows the same warning as the single-store editor and leaves
        /// <see cref="Viewer"/> null, otherwise constructs the viewer, populates the list, and shows
        /// the dialog modally. WinForms shell; excluded from coverage.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void Launch()
        {
            var readiness = StoreLaunchReadinessEvaluator.Evaluate(Globals);
            if (readiness.State != StoreLaunchReadinessState.Ready)
            {
                MyBox.ShowDialog(
                    "Store settings are not available yet. Please try again after startup completes.",
                    "Store Settings Unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            Viewer = new DisabledStoresViewer(this);
            PopulateRows();
            Viewer.ShowDialog();
        }
    }
}
