# efc-arrow-keys-skip-subfolders (Issue #1007)

- Date captured: 2026-10-22
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-arrow-keys-skip-subfolders/ (Issue #1007)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #1007
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/1007
- Last Updated: 2026-10-22
## Summary

In the EFC viewer's folder tree, expanded subfolder rows can be selected with the mouse but not with the Up and Down arrow keys. Keyboard navigation does not move the selection onto any subfolder. This is a gap in the fix delivered for issue #1002 (PR #1003).

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in), QuickFiler EFC viewer (WebView2 breadcrumb tree)
- Python version: not applicable (C# .NET Framework 4.8 and JavaScript in `QuickFiler/Resources/FolderBreadcrumb.js`)
- Command/flags used: interactive use of the EFC viewer in Outlook
- Data source or fixture: any mail folder hierarchy with nested subfolders

## Steps to Reproduce

1. Open the EFC viewer on a mail item.
2. Expand a folder row so that its subfolder rows are shown.
3. Click a subfolder row: it is selected, as expected after #1002.
4. Press the Up or Down arrow key to move through the list.

## Expected Behavior

The arrow keys move the selection through the visible rows, including expanded subfolder rows, the same way mouse selection reaches them.

## Actual Behavior

The arrow keys do not select any subfolder row. Keyboard navigation reaches only the top-level rows.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: reported by the maintainer from interactive use; no log captured.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

- #1002 ported the QFC #997 subfolder selection to EFC for mouse clicks. The arrow-key path (`QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`, the EFC breadcrumb wiring in `EfcFormController.Breadcrumb.cs`, and the key handling in `QuickFiler/Resources/FolderBreadcrumb.js`) appears to iterate only the top-level rows and not the expanded descendant rows.
- Compare with how QFC handles arrow keys over expanded subfolder rows after #997.
- The exact expected keyboard behaviour is being settled in a grill-me round and will be recorded as acceptance criteria.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: arrow navigation over a row list that contains expanded descendant rows (C# router tests and node tests under `tests/quickfiler-breadcrumb/`)
- [ ] Integration scenario to retest: Up and Down across parent, nested subfolder, and next top-level rows
- [ ] Manual verification notes: confirm in Outlook that the arrow keys select subfolder rows in the EFC viewer

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
