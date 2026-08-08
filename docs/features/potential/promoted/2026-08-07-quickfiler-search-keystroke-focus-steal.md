# quickfiler-search-keystroke-focus-steal (Issue #438)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-search-keystroke-focus-steal/ (Issue #438)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #438
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/438
- Last Updated: 2026-08-08
## Summary

Typing into the QuickFiler folder-search textbox loses keyboard focus after roughly one to two characters. Each `TextChanged` event opens the breadcrumb folder drop-down, and opening the drop-down moves focus to the popup surface, so the remainder of the typed search string is not delivered to the textbox and the view jumps to the folder that was selected in the partially-typed result set.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- UI path: QuickFiler `ItemViewer` folder-search textbox (`TxtboxSearch`) and the WebView2 breadcrumb folder selector
- Data source or fixture: `FolderPredictor` / `IFolderSearchHandler.FindFolder` results for a wildcard search string

## Steps to Reproduce

1. Open QuickFiler on an item so an `ItemViewer` folder-search textbox is available.
2. Place focus in the folder-search textbox.
3. Type a multi-character folder search string at normal typing speed (for example `invoices`).
4. Observe where the caret and keyboard focus are after the first one to two characters, and observe which folder the selector shows.

## Expected Behavior

- Typing in the search textbox opens (or keeps open) the folder drop-down and refreshes its filtered contents on every keystroke.
- Keyboard focus and the caret stay in the search textbox for the whole time the user is typing. No keystroke is redirected to the drop-down surface.
- The drop-down updates its rows and its highlighted row without taking focus and without committing a folder selection.
- Focus moves to the drop-down only on an explicit user gesture: Down arrow from the textbox (existing `TextBoxSearch_KeyDown` contract), a click on the drop-down arrow, or a click on a row.

## Actual Behavior

- After approximately one to two characters, keyboard focus leaves the search textbox and the selector surface receives focus, so subsequent characters are not appended to the search text.
- The selector highlights and shows the folder that matched the truncated search string, which reads as the view "jumping to the open folder" mid-typing.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence (2026-08-07) is recorded under Suspected Cause below; no runtime log capture yet.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Folder search is the primary way to file an item to a folder that is not among the suggestions. Because focus is lost after one to two characters, the search textbox is effectively unusable for any multi-character query, and the user must retype or click back into the textbox repeatedly.

## Suspected Cause / Notes

Read of the current sources on 2026-08-07:

- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:164` — `TextBoxSearch_TextChanged` runs on every keystroke and unconditionally calls `_itemViewer.SetFolderSelectedIndex(1)` (when two or more rows matched) and `_itemViewer.SetFolderDroppedDown(true)` at lines 175-177. There is no guard that leaves focus with the sender.
- `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:31` — `SetFolderDroppedDown(true)` forwards to `SetBreadcrumbDropDownState(true)`.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:223` — with no lifecycle coordinator this calls `FocusBreadcrumb()` directly; with a coordinator it calls `BreadcrumbItemViewerLifecycleCoordinator.SetDroppedDown(droppedDown, FocusBreadcrumbCore)`.
- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:176` — `SetDroppedDown` focuses when no open coordinator is present, otherwise delegates to `BreadcrumbDropDownOpenCoordinator.SetDroppedDown`.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:237` and `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:294` — the open path schedules `_focusPending`, which `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:164` supplies as `() => host.ControlHost?.Control.Focus()`. Opening the drop-down therefore focuses the popup control by design.

The composition of "refresh on every keystroke" with "opening the drop-down focuses the popup" is the defect: the open intent used by the search path needs a non-focusing variant, or the search path must not re-issue the open intent once the drop-down is already open.

Also worth checking, but not claimed here: whether the EfcViewer search path (`QuickFiler/Controllers/EfcFormController.cs:556` `SearchText_TextChanged`) exhibits the same focus loss. The EFC folder list is a persistently visible WebView2 rather than a popup, so its `NavigateToString` re-delivery is a separate mechanism and may or may not steal focus.

## Proposed Fix / Validation Ideas

Design direction (to be confirmed during planning):

- Separate the "show / refresh the drop-down" intent from the "focus the drop-down" intent so the search-text path can open and update the selector without a focus transfer.
- Make the search-driven refresh idempotent with respect to open state: when the selector is already open, refresh rows only.
- Keep `TextBoxSearch_KeyDown` (Down arrow) as the explicit focus-transfer gesture, unchanged.
- Re-evaluate the unconditional `SetFolderSelectedIndex(1)` on each keystroke: highlighting a row is appropriate, but it must not commit a selection or change what the collapsed surface reports while typing.

Validation:

- [ ] Unit coverage areas: MSTest coverage over `QfcItemController.TextBoxSearch_TextChanged` asserting the refresh intent is issued and that no focus-transfer intent is issued; coverage over the drop-down open coordinator asserting a refresh-only open does not schedule `_focusPending`; regression coverage that Down arrow still issues both `SetFolderDroppedDown(true)` and `FocusFolderDropDown()` (existing tests in `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs` and `QfcItemController.NavigationTests.cs` are part of the spec).
- [ ] Integration scenario to retest: multi-character search string typed through the viewer seam, asserting the full string reaches `SearchText` and the row set reflects the complete query.
- [ ] Manual verification notes: type an eight-character folder name at normal speed and confirm the caret never leaves the textbox and the drop-down contents track each keystroke.

Related prior work: issue #400 (`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/`) defined the current drop-down open, focus, commit, and cancel contract. Any fix here must preserve the #400 acceptance criteria for explicit open/commit/cancel gestures.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
