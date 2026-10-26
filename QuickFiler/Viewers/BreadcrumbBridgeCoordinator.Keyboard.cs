#nullable enable
using System;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Issue #1007 keyboard routing for the QFC selector: selector keys (including Ctrl+Left,
    /// Home, End and the page keys) and the page <c>arrowKey</c> and <c>unhandledArrow</c> messages,
    /// which pass through one decision against the pending entry of an open list (R8).
    /// </summary>
    public sealed partial class BreadcrumbBridgeCoordinator
    {
        /// <summary>
        /// Applies native combo-box key semantics: closed arrows commit, open arrows move pending,
        /// Enter commits the pending entry, and Escape restores the opening selection. Open-list
        /// Ctrl+Left (<see cref="BreadcrumbSelectorKey.Parent"/>), Home, End, PageUp and PageDown
        /// move only the pending highlight (AC8, AC11).
        /// </summary>
        /// <param name="key">The selector key; page keys need a page size and are no-ops here.</param>
        /// <returns>True when the key changed selector state.</returns>
        public bool HandleSelectorKey(BreadcrumbSelectorKey key) =>
            HandleSelectorKeyCore(key, null);

        /// <summary>
        /// <see cref="HandleSelectorKey"/> with the page size carried by a <c>selectorKey</c>
        /// message. Kept separate so the public one-argument signature, which existing callers bind
        /// by name, is unchanged (#1007 R8).
        /// </summary>
        /// <param name="key">The selector key.</param>
        /// <param name="pageSize">
        /// The fully visible row count for PageUp and PageDown; a missing or non-positive value
        /// makes a page key a no-op (S3).
        /// </param>
        private bool HandleSelectorKeyCore(BreadcrumbSelectorKey key, int? pageSize)
        {
            switch (key)
            {
                case BreadcrumbSelectorKey.Up:
                    return MoveSelector(previous: true);
                case BreadcrumbSelectorKey.Down:
                    return MoveSelector(previous: false);
                case BreadcrumbSelectorKey.Enter:
                    return CommitSelector();
                case BreadcrumbSelectorKey.Escape:
                    return CancelSelector();
                case BreadcrumbSelectorKey.Parent:
                    return ApplyTransition(_router.MoveSelectorToParent());
                case BreadcrumbSelectorKey.Home:
                case BreadcrumbSelectorKey.End:
                    // Closed Home and End commit the first and last top-level row (D-Q16).
                    return ApplyTransition(
                        IsSelectorOpen
                            ? _router.MoveSelectorToBoundary(key == BreadcrumbSelectorKey.Home)
                            : _router.CommitSelectorBoundaryRow(key == BreadcrumbSelectorKey.Home)
                    );
                case BreadcrumbSelectorKey.PageUp:
                case BreadcrumbSelectorKey.PageDown:
                    return pageSize.HasValue
                        && ApplyTransition(
                            _router.MoveSelectorPage(
                                previous: key == BreadcrumbSelectorKey.PageUp,
                                pageSize.Value
                            )
                        );
                default:
                    return false;
            }
        }

        /// <summary>
        /// Routes a page <c>arrowKey</c> or <c>unhandledArrow</c> message through one decision
        /// (R8). Closed: Right opens the list and Left is consumed (D-Q14). Open: Left on a
        /// non-expanded pending entry commits and closes (D-Q15); Right on a pending entry without
        /// children closes the list keeping the original choice and then raises
        /// <see cref="UnhandledArrow"/> for the pop-out (D-Q21). A consumed arrow raises no
        /// <see cref="FolderArrowKeyDown"/>. Every other open-list arrow keeps the router route and
        /// then republishes the selector state, because the pending entry may have moved.
        /// </summary>
        /// <returns>True when the message was handled here; false to take the default route.</returns>
        private async Task<bool> TryDispatchOpenArrowAsync(string json)
        {
            BreadcrumbArrowDirection? direction = ArrowDirection(json);
            if (direction == null)
            {
                return false;
            }
            if (!IsSelectorOpen)
            {
                if (direction.Value == BreadcrumbArrowDirection.Right)
                {
                    await _dispatcher.Dispatch(() => OpenSelector()).ConfigureAwait(false);
                }
                return true;
            }
            switch (_router.ClassifyPendingArrow(direction.Value))
            {
                case BreadcrumbPendingArrowAction.CommitAndClose:
                    await _dispatcher.Dispatch(() => CommitSelector()).ConfigureAwait(false);
                    return true;
                case BreadcrumbPendingArrowAction.CancelAndPopOut:
                    await _dispatcher.Dispatch(() => CancelSelector()).ConfigureAwait(false);
                    await _dispatcher
                        .Dispatch(() => UnhandledArrow?.Invoke(this, direction.Value))
                        .ConfigureAwait(false);
                    return true;
                default:
                    await DispatchAsync(json).ConfigureAwait(false);
                    await _dispatcher
                        .Dispatch(() => PostSelectorStateCore(_router.GetSelectorState()))
                        .ConfigureAwait(false);
                    return true;
            }
        }

        /// <summary>
        /// Applies the #1007 selector click messages dispatched from the selector channel: a
        /// segment double-click commits that segment's folder and closes (AC9, S5), and an open-list
        /// single click on a non-leaf segment moves only the pending highlight (D-Q27).
        /// </summary>
        private void HandleSelectorClick(BreadcrumbSelectorMessage message)
        {
            switch (message)
            {
                case BreadcrumbSelectorSegmentDoubleClickMessage doubleClick:
                    ApplyTransition(
                        _router.DoubleClickSelectorSegment(
                            doubleClick.RowIdentity,
                            doubleClick.SegmentIndex
                        )
                    );
                    break;
                case BreadcrumbSelectorHighlightMessage highlight:
                    ApplyTransition(_router.HighlightSelector(highlight.Identity));
                    break;
            }
        }

        private static BreadcrumbArrowDirection? ArrowDirection(string json)
        {
            if (!(MessageType(json) is "arrowKey" or "unhandledArrow"))
            {
                return null;
            }
            try
            {
                switch (BreadcrumbBridgeSerializer.Parse(json))
                {
                    case ArrowKeyMessage arrow:
                        return arrow.Direction;
                    case UnhandledArrowMessage report:
                        return report.Direction;
                    default:
                        return null;
                }
            }
            catch (FormatException)
            {
                // Malformed arrows take the default route and surface as a router error response.
                return null;
            }
        }
    }
}
