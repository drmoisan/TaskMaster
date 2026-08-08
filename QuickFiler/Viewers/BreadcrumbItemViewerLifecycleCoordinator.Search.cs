#nullable enable
using System.Collections.Generic;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Issue #438: the non-focusing drop-down path used by the folder-search presentation.
    /// <para>
    /// Held on a second partial-class part so
    /// <c>BreadcrumbItemViewerLifecycleCoordinator.cs</c> (481 lines) stays clear of the
    /// repository's 500-line ceiling.
    /// </para>
    /// </summary>
    internal sealed partial class BreadcrumbItemViewerLifecycleCoordinator
    {
        /// <summary>
        /// Presents a folder-search result set through the bridge coordinator without transferring
        /// focus to the drop-down.
        /// </summary>
        /// <param name="items">The exact search-result strings, carried verbatim.</param>
        /// <remarks>
        /// The non-focusing counterpart of <see cref="SetDroppedDown"/>:
        /// <list type="bullet">
        /// <item>with an open coordinator, the "next native open takes no focus" latch is set
        /// <em>before</em> the composite opens the selector, so the resulting
        /// <c>SelectorOpenStateChanged</c>-driven open observes it (both run FIFO on the same
        /// posted-operation queue);</item>
        /// <item>with no open coordinator, the bare fallback performs no <c>Focus(focus)</c> call at
        /// all — that call is the fallback branch's own focus steal.</item>
        /// </list>
        /// The explicit-gesture path <see cref="SetDroppedDown"/> is unchanged and keeps focusing on
        /// open (issue #400 AC-13 for gestures).
        /// </remarks>
        internal void PresentSearchResults(IReadOnlyList<string> items)
        {
            ThrowIfDisposed();

            // Latch first: the composite's OpenSelector() raises SelectorOpenStateChanged, which is
            // what posts the native open. Latching afterwards would be observed by the wrong open.
            _openCoordinator?.LatchNextOpenTakesNoFocus();

            _bridgeCoordinator?.PresentSearchResults(items);
        }
    }
}
