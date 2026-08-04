# quickfiler-folder-selector-dropdown (Issue #400)

- Date captured: 2026-07-21
- Author: Dan Moisan
- Status: Implemented -> pending feature review (Issue #400)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #400
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/400
- Last Updated: 2026-07-21
- Work Mode: full-bug

## Summary

QuickFiler's WebView-based folder selector does not preserve the user-visible behavior of the Windows Forms drop-down control it replaced. The collapsed control can display a row other than the selected folder, omits the selected suggestion's existing normalized probability, exposes in-place vertical scrolling instead of one drop-down affordance, cannot expand over neighboring controls, and does not implement the former Up, Down, Enter, and Escape selection contract.

## Environment

- OS/version: Supported Windows desktop environments, including multi-monitor working areas and negative monitor coordinates
- Runtime: .NET Framework 4.8.1 WinForms add-in with Microsoft WebView2
- UI path: QuickFiler `ItemViewer` folder selection widget
- Data source or fixture: `FolderPredictor` suggestions and their existing normalized `FolderRow.Score` values
- Relevant predecessor guarantee: issue #398 atomic synchronous-to-asynchronous breadcrumb upgrade and selection preservation

## Steps to Reproduce

1. Open QuickFiler for an item that produces multiple folder suggestions with normalized probability scores.
2. Select a suggestion other than the first selectable row and return the selector to its collapsed state.
3. Observe the single-row host, its right-side affordances, and the selected suggestion's percentage.
4. Press Up and Down while the selector is closed, then open it and repeat Up and Down before pressing Enter or Escape.
5. Open the selector and dismiss it by clicking outside without committing a pending selection.
6. Position `ItemViewer` near the top and bottom of the active monitor working area and open the selector.
7. Exercise immediate rendering and hierarchy-upgrade cases where a suggestion is unresolved, resolves to an empty chain, or the hierarchy provider fails.

## Expected Behavior

- When a selectable folder is selected, the collapsed control renders exactly one folder row: that selected folder and its existing normalized probability percentage. It does not recompute or renormalize the score.
- The right side contains exactly one accessible drop-down arrow. No vertical scrollbar, spinner, or scroll arrows are present.
- Activating the arrow opens a native transient popup that overlays sibling controls, remains owned by and above its `ItemViewer`, and is not globally topmost.
- The popup opens at its full desired height below the anchor when it fits in the active monitor working area. Otherwise it opens above; if neither side fits fully, it uses the side with more available space, favors below on a tie, and clamps within the working area.
- Closed-state Up and Down immediately select the previous or next selectable folder. Open-state Up and Down change only the pending selection. These keys never scroll the control and skip non-selectable rows without wrapping past the first or last selectable row.
- Enter and mouse row activation commit the pending selection and close the popup. Escape and any uncommitted outside/auto-close restore the selection that was active when the popup opened.
- Left and Right retain the existing breadcrumb expansion, collapse, and fall-through behavior.
- Immediate, resolved, unresolved, empty-chain, and hierarchy-provider-failure renders retain the supplied probability, stable row identity, and issue #398 atomic-upgrade selection guarantees.
- Light and dark theme state, keyboard focus, disposal and pooled `ItemViewer` reuse, and event routing remain correct for both WebView surfaces without duplicate selection events.

## Actual Behavior

- The selected row can remain outside the visible 25-pixel WebView viewport while another row is shown.
- The embedded page exposes vertical scrolling controls and scrolls rows in place.
- There is no dedicated drop-down arrow or native overlay list that can cross neighboring layout cells.
- Up, Down, Enter, and Escape do not implement the former combo-box selection, commit, and cancel behavior.
- The probability percentage supplied with each scored `FolderRow` is discarded from the immediate fallback render and from unresolved, empty-chain, and provider-failure fallback paths, so it is not visible in the widget.

## Logs / Screenshots

- [x] Code and research evidence captured; no user-operated screenshot or manual validation is required for acceptance.
- `QuickFiler/Resources/FolderBreadcrumb.html` renders all rows inside the fixed-height WebView, leaves vertical overflow available, and handles only Left and Right.
- `QuickFiler/Viewers/ItemViewer.Designer.cs` constrains the child WebView to one row, so HTML and CSS cannot overlay sibling WinForms controls.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` and `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` convert several fallback suggestions to non-scored plain rows before projection.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: users cannot reliably see or operate the current folder selection, and the probability percentage that motivated the selector is absent in normal and failure-path presentations. Keyboard cancellation can also leave a different folder selected than the user intended.

## Suspected Cause / Notes

The WebView replacement retained a fixed-height child control but rendered the full row collection inside it. Content in that child HWND cannot cross the `TableLayoutPanel` boundary, so the requested drop-down behavior requires a native owned popup rather than CSS positioning. Separately, the coordinator's synchronous render and the router's unresolved, empty-chain, and failure fallbacks convert scored suggestions to plain rows, which removes `FolderRow.Score` before `PercentageFormatter` projects it. The fix must preserve issue #398's atomic row replacement and stable selection while adding an explicit committed/original/pending selection session.

## Proposed Fix / Validation Ideas

- [x] Add deterministic fail-before MSTest coverage for closed projection, normalized probability retention, selection-session transitions, placement geometry, bridge messages, accessibility asset contracts, focus/lifecycle seams, and duplicate-event prevention.
- [x] Represent unresolved suggestions as scored fallback suggestions with stable identity, fallback text, and the supplied probability; upgrade display chains atomically without recomputing the score.
- [x] Use a host-neutral committed/original/pending selection session and a pure active-monitor placement calculator so keyboard, cancel, and geometry behavior can be verified without a live UI.
- [x] Host a lazily initialized popup WebView2 in `ToolStripControlHost` inside an owned `ToolStripDropDown`, reusing the existing `CoreWebView2Environment` and preserving the existing `IItemViewer.SetFolderDroppedDown(bool)` seam.
- [x] Extend the shared breadcrumb asset with collapsed and expanded view modes, exactly one accessible drop-down button, hidden closed overflow, selector-key messages, and unchanged Left/Right routing.
- [x] Keep every added production and test file below 500 lines, explicitly include new `.cs` files in the applicable legacy `.csproj`, and run the exact C# format, analyzer, nullable, MSTest, and coverage gates in one uninterrupted final pass.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
