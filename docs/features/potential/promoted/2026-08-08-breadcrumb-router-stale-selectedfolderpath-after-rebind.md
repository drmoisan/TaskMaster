# breadcrumb-router-stale-selectedfolderpath-after-rebind (Issue #499)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-router-stale-selectedfolderpath-after-rebind/ (Issue #499)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #499
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/499
- Last Updated: 2026-08-08
## Summary

`BreadcrumbBridgeRouter.BindRowsAsync` clears `_selectedRowId` but does not clear
`SelectedFolderPath`. After a re-bind the UI shows no row highlighted while
`EfcFormController.SelectedFolder` still reports the previously selected folder. Because
`BindFolderRows` runs on every search keystroke, a confirm action taken at that moment can file mail
to a folder the user can no longer see selected.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2)
- Command/flags used: n/a — reached through the EfcViewer folder-list breadcrumb surface
- Data source or fixture: any folder set where a selection is made and then the search text changes

## Steps to Reproduce

1. Open the EfcViewer folder list and type enough search text to show candidate folder rows.
2. Select a folder row, so `SelectRow` sets both `_selectedRowId` and `SelectedFolderPath`.
3. Type one more character in the search box. This reaches
   `EfcFormController.BindFolderRows` (`QuickFiler/Controllers/EfcFormController.cs:873-883`) and so
   `BreadcrumbBridgeRouter.BindRowsAsync`.
4. Observe that no row is highlighted in the re-rendered document.
5. Trigger a move or folder-open action.

## Expected Behavior

After a re-bind clears the visible selection, the controller's `SelectedFolder` should agree with the
UI: either no folder is reported as the filing target, or the selection is visibly restored. The two
state fields `_selectedRowId` and `SelectedFolderPath` are written together in `SelectRow` and should
be cleared together.

## Actual Behavior

Only `_selectedRowId` is cleared. `SelectedFolderPath` retains the previous value, so
`EfcFormController.SelectedFolder` keeps returning the old folder while the UI shows nothing
selected. A move performed at that point targets the stale folder.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — this is a silent state divergence with no error text.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Rationale: the consequence is mail filed to an unintended folder — a silent, user-visible data
placement error with no exception to signal it. Recorded by research as Medium-High; raised to High
here because `BindFolderRows` runs on every keystroke, so the divergent window is common rather than
rare, and the failure is silent.

## Suspected Cause / Notes

Verified call chain:

1. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:114` — `_selectedRowId = null;` after a re-bind.
   `SelectedFolderPath` (declared `:58`) is not reset; it is written only in `SelectRow` (`:372`).
2. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:399` — the re-rendered document is built with
   `_selectedRowId = null`, so no row is visually highlighted.
3. `QuickFiler/Controllers/EfcFormController.cs:289-294` —
   `public string SelectedFolder => _router?.SelectedFolderPath;` still returns the previous value.
4. `QuickFiler/Controllers/EfcFormController.cs:873-883` — `BindFolderRows` is invoked from the
   `SearchText.TextChanged` path and from the delete-path trash rebind.
5. `QuickFiler/Controllers/EfcFormController.cs:493` and `:772` pass `SelectedFolder` into the move
   operation; `:478`, `:722`, and `:760` pass it into folder-open.

Discovered during preparation research for epic #136 child F12 (issue #495), recorded in
`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/research/2026-08-08T02-10-breadcrumb-bridge-router.md`
as LD-2. Not a duplicate: #462 concerns the drop-down coordinator's `closePending` flag, #488 the
ItemViewer breadcrumb pipeline lifecycle, and #440 arrow-key tree semantics. None touches this field
pair.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a failing regression test first, per the repository Bugfix Workflow,
      asserting that after `BindRowsAsync` re-binds, `SelectedFolderPath` no longer reports the
      pre-rebind folder.
- [ ] Integration scenario to retest: select a folder in EfcViewer, type an additional search
      character, then confirm a move and verify the destination matches the visible selection.
- [ ] Manual verification notes: the fix must decide whether clearing `SelectedFolderPath` should
      also raise `SelectedFolderPathChanged(null)`, and whether re-binding should instead attempt to
      restore the prior selection when the same folder is still present in the new row set. Either
      choice is an observable production contract change, which is why it was out of scope for #495
      under the epic's no-behavior-change NFR.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
