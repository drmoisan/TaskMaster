# quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select (Issue #796)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select/ (Issue #796)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #796
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/796
- Last Updated: 2026-09-07
- Work Mode: full-bug

## Summary

In the QuickFiler item view, opening the folder drop-down makes the list flash open and immediately close, whether opened by clicking the arrow or by pressing Down in the search box. Typing letters in the search box does expand the list and keep it open, but clicking an item in the expanded list closes it without selecting that item; only the Up and Down keys change the selection. The list should stay open until it is closed explicitly, an item is selected, or a different QfcItem is selected, and a click on an item should select it.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in; the drop-down is a WebView2 breadcrumb (`QuickFiler\Resources\FolderBreadcrumb.html`) in `ItemViewer` plus a `ToolStripDropDown` popup hosting a second WebView2 (`BreadcrumbDropDownHost`); debug build from `TaskMaster\bin\Debug`, HEAD `c431dc32` (2026-09-06)
- Command/flags used: Outlook ribbon -> QuickFiler (ordinary and High Confidence)
- Data source or fixture: live mailbox, Inbox view

## Steps to Reproduce

1. Launch QuickFiler. On any item, click the drop-down arrow in the folder field. Observe: the list opens and closes within a fraction of a second.
2. Put the caret in the search box and press Down. Observe: the same flash.
3. Type two or three letters in the search box. Observe: the list expands with search results and stays open.
4. Click an item in the expanded list. Observe: the list closes and the folder field still shows the previous selection.
5. Use Up/Down keys instead. Observe: the selection changes as expected.

Reproduces on every item; timing after item load does not matter (maintainer confirmed "it always occurs").

## Expected Behavior

- Opening the list by mouse or keyboard keeps it open until: the user closes it (Escape, Left arrow, clicking the arrow again), an item is selected, or a different QfcItem is selected.
- Clicking an item in the open list selects that item and closes the list.
- A refresh of the row set while the list is open (search results, late suggestion decoration) does not close it (already guaranteed by #438 AC-3 and must remain so).

## Actual Behavior

- Open-by-arrow and open-by-Down both close immediately.
- Click on a row closes the list and discards the selection.
- Keyboard selection works.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- No ERROR/WARN lines are logged for this behavior; the close is a normal code path. Runtime instrumentation is required to confirm which of the two candidate close paths fires first (see Suspected Cause).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Mouse selection of a filing folder is not possible in the item view; the user must type a search string and navigate with the keyboard.

## Suspected Cause / Notes

Confirmed by code read (2026-09-06):

- The drop-down is not a ComboBox. The arrow is `#dropDownButton` in `FolderBreadcrumb.html:440-442`, which posts `selectorToggle`. The open pipeline is `BreadcrumbBridgeCoordinator.HandleSelectorMessage` (`:349-358`) -> `FolderBreadcrumbBridgeRouter.OpenSelector` -> `BreadcrumbSelectionSession.Open` -> `SelectorOpenStateChanged` -> `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged` (`:178-191`) -> `BreadcrumbDropDownHost.OpenAsync` -> `BreadcrumbDropDownOpenLifetime.OpenCoreAsync` (`:215-256`) -> `FocusCurrentSurface` (`BreadcrumbDropDownOpenLifetime.Focus.cs:32-51`) -> `_host.FocusPending()`. The popup is a `ToolStripDropDown` with `AutoClose = true` (`BreadcrumbDropDownHost.cs:165-172`, `BreadcrumbDropDownHost.Open.cs:98-102`). The mouse toggle and the programmatic open share one request path (`QuickFiler.Test\Viewers\BreadcrumbSelectorOpenRetryTests.cs:55`), which is consistent with mouse and keyboard failing identically.
- Asynchronous suggestion decoration is not the cause: `SetSuggestionsAsync` / `SetSuggestionFallbacks` route through `ReplaceRowsPreservingSession` (`FolderBreadcrumbBridgeRouter.cs:478-482`) -> `BreadcrumbSelectionSession.ReconcileRowsReplaced` (`:119-147`), which preserves `IsOpen` and raises no `SelectorOpenStateChanged`.
- Three code paths cancel the selector session from outside the user's gesture and each closes the popup:
  1. `QfcFormController.ParkFocusAndCancelSelectors` (`QuickFiler\Controllers\QfcFormController.Deactivate.cs:39-57`, wired to `Form.Deactivate` at `QfcFormController.SetupDisposal.cs:175`, added by #677) cancels every item's selector when the QuickFiler form loses activation and moves `ActiveControl` back into the form (`QfcFormViewer.cs:207`). Opening the popup calls `Control.Focus()` on the popup's own top-level window (`ItemViewer.Breadcrumb.cs:203`), which deactivates the QuickFiler form. There is no latch distinguishing a self-inflicted deactivation from a real one. `FinishOpenCore` (`BreadcrumbDropDownOpenCoordinator.cs:273-288`) also re-checks `_isSelectorOpen()` after the async open and closes if the session was cancelled meanwhile. Primary suspect for the flash. (Win32 activation ordering is inferred; the downstream code is confirmed.)
  2. Native `ToolStripDropDown` auto-close -> `BreadcrumbDropDownHost.OnDropDownClosed` (`:426-437`) -> `FinishClose(Uncommitted)` (`:439-455`) -> `_cancelSelection()` = `BreadcrumbCoordinator.CancelSelector()` (`ItemViewer.Breadcrumb.cs:205`). This is the mechanism behind the click-without-select symptom: a click inside the popup's WebView2 shifts activation, the popup auto-closes, and the uncommitted selection is cancelled before the row's selection message commits. This hazard was recorded as unverifiable in `docs\features\archive\2026-08-07-quickfiler-search-keystroke-focus-steal-438\research\2026-08-08T10-30-...-research.md:172`.
  3. `QfcItemController.TextBoxSearch_Leave` (`QuickFiler\Controllers\QfcItemController.EventHandlers.cs:217-228`) closes the drop-down on search-box leave; its `_searchLeaveHandoffPending` latch (#680) is set only for the Down-arrow path (`:195`). Since Down also flashes, this path is not the primary cause but the missing mouse-path latch remains a gap.
- `MayRestoreBreadcrumbFocus` (`ItemViewer.Breadcrumb.cs:270-274`) requires `Form.ActiveForm` to be the QuickFiler form, so once the popup owns activation the focus step becomes a no-op while the cancel step always runs (asymmetry documented at `BreadcrumbDropDownHost.cs:452`).
- Existing tests that the fix must reconcile with: `QfcFormControllerDeactivateTests.FormDeactivated_CancelsSelectorOnEveryItemController` (`QuickFiler.Test\Controllers\QfcFormControllerDeactivateTests.cs:172`) pins cancel-on-deactivate; `BreadcrumbPendingOpenCloseTests` (`:124`, `:143`) encode "close wins over a pending open".
- Typing keeps the list open because `ReplaceItemsPreservingSession` (`FolderBreadcrumbBridgeRouter.SearchPresentation.cs:38-55`) reports no `OpenStateChanged` (#438 AC-3), and because after typing the search box holds focus inside the QuickFiler form, so no deactivation occurs.

## Proposed Fix / Validation Ideas

Acceptance criteria settled with the maintainer on 2026-09-06:

- [ ] AC1: Opening the list by arrow click or by Down in the search box leaves it open until Escape, Left, a second arrow click, an item selection, or selection of a different QfcItem.
- [ ] AC2: A deactivation of the QuickFiler form caused by the popup taking focus does not cancel the selector session; a deactivation caused by any other window still does (the #677 contract is preserved for genuine deactivation).
- [ ] AC3: A mouse click on a row in the open list selects that row and closes the list; the selection is committed before any auto-close cancel runs.
- [ ] AC4: The #680 leave-handoff latch covers the mouse open path as well as the Down-arrow path.
- [ ] AC5: Row-set refreshes while open (search, late decoration) continue not to close the list (#438 AC-3 regression guard).
- [ ] AC6: The first implementation step instruments `ParkFocusAndCancelSelectors` and `OnDropDownClosed` with debug log lines so the runtime ordering is confirmed before the fix is chosen.

Validation:

- [ ] Unit coverage areas: self-inflicted-deactivation latch (Moq the form viewer's active-form seam); commit-before-cancel ordering in `BreadcrumbDropDownHost.FinishClose`; mouse-path leave latch; existing deactivate and pending-open tests updated to the new contract rather than weakened.
- [ ] Integration scenario to retest: open by mouse, open by Down, refresh while open, click a row, select a different QfcItem while open.
- [ ] Manual verification notes: the runbook `docs\features\archive\...-438\runbooks\verify-search-focus-retention.runbook.md` covers the search path; extend it with the arrow-click and row-click gestures.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
