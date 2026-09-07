#nullable enable
using System.Windows.Forms;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Issue #796 (AC6): close-ordering diagnostics for the breadcrumb popup host.
    /// <para>
    /// Held on a third partial-class part because <c>BreadcrumbDropDownHost.cs</c> stands at the
    /// repository's 500-line ceiling, leaving no room there for a logger declaration, a message
    /// formatter, and the log statement itself. The native-close handler moves here with them: it is
    /// the site the diagnostic instruments, and relocating it is what buys the main part headroom.
    /// </para>
    /// <para>
    /// This part adds no behaviour. The handler body is the one that previously lived in the main
    /// part, with one Debug-level log statement added ahead of it.
    /// </para>
    /// </summary>
    public sealed partial class BreadcrumbDropDownHost
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(BreadcrumbDropDownHost)
        );

        /// <summary>
        /// Renders the one-line close-ordering diagnostic emitted on entry to
        /// <see cref="OnDropDownClosed"/>.
        /// </summary>
        /// <param name="closeReason">The reason WinForms gave for closing the drop-down.</param>
        /// <param name="programmaticClose">Whether this host initiated the close itself.</param>
        /// <param name="openState">The host's own open state at entry.</param>
        /// <param name="autoClose">The drop-down's <c>AutoClose</c> setting at entry.</param>
        /// <param name="disposed">Whether the host has already been disposed.</param>
        /// <param name="pendingClose">Whether a close completion is already pending.</param>
        /// <returns>A single line carrying a sentence prefix and six Key=Value pairs.</returns>
        /// <remarks>
        /// Pure and static so the AC6 evidence rests on a deterministic managed-seam assertion
        /// rather than a source-text scan: a test calls this directly with a fixed argument tuple
        /// and needs no popup, window, or WebView2 surface.
        /// </remarks>
        internal static string FormatDropDownClosedDiagnostics(
            ToolStripDropDownCloseReason closeReason,
            bool programmaticClose,
            bool openState,
            bool autoClose,
            bool disposed,
            bool pendingClose
        ) =>
            "Issue #796: BreadcrumbDropDownHost.OnDropDownClosed entered. "
            + $"CloseReason={closeReason} ProgrammaticClose={programmaticClose} "
            + $"OpenState={openState} AutoClose={autoClose} "
            + $"Disposed={disposed} PendingClose={pendingClose}";

        private void OnDropDownClosed(object? sender, ToolStripDropDownClosedEventArgs e)
        {
            // Issue #796 (AC6): emitted at entry, ahead of the guard return, so a close this host
            // suppresses is still visible in the ordering evidence the Phase 2 runbook collects.
            log.Debug(
                FormatDropDownClosedDiagnostics(
                    e.CloseReason,
                    _programmaticClose,
                    OpenState,
                    DropDown.AutoClose,
                    _disposed,
                    _openLifetime.IsPendingClose
                )
            );
            if (_disposed || _programmaticClose || !OpenState)
                return;
            _openLifetime.InvalidateAndSchedule(() =>
            {
                if (_disposed || _programmaticClose || !OpenState)
                    return;
                OpenState = false;
                FinishClose(BreadcrumbDropDownCloseReason.Uncommitted);
            });
        }
    }
}
