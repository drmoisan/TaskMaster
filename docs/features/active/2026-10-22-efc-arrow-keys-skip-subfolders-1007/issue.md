# efc-arrow-keys-skip-subfolders (Issue #1007)

- Date captured: 2026-10-22
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-arrow-keys-skip-subfolders/ (Issue #1007)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #1007
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/1007
- Last Updated: 2026-10-22
- Work Mode: full-bug

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

## Acceptance Criteria

The following criteria come from the grill-me interview (`grill-transcript.md`, rounds 1 to 3, confirmed by the maintainer on 2026-10-23, amended by round 4 the same day). They apply to EFC and QFC. "Visible rows" are the top-level rows plus every currently shown subfolder at any depth, in screen order.

- [x] AC1 (Up/Down tree order): Down and Up visit every visible row in tree order. The children of a collapsed node are skipped. Up from the row below an expanded block lands on the block's last visible descendant. (Q1, Q2)
- [x] AC2 (No wrap): the ends do not wrap. In EFC, Up on the first row moves focus to the search box and Down on the last row does nothing. (Q5)
- [x] AC3 (EFC selects on move): in EFC an arrow onto a row or subfolder selects it exactly as a mouse click does, and Enter files to it. (Q3)
- [x] AC4 (QFC open-list pending model): with the QFC folder list open, arrows move only the pending highlight. Enter commits, Escape restores the original choice, and Left on a highlighted item that is not expanded commits it and closes the list. (Q7, Q15)
- [x] AC5 (Right): Right on an item that is not expanded and has children expands it. Right on an expanded item moves to its first child (selected in EFC, pending in QFC). A shortened top-level row (`Archive > Clients [+]`) expands to list the segment's subfolders, as in AC9; it does not restore the trailing segments. (Q4, Q13, Q23)
- [x] AC6 (Right on a leaf): in EFC, Right on a folder with no subfolders does nothing. With the QFC list open, the list closes, the original choice is kept, and the "Pop Out Item or Enumerate Conversation?" dialog opens. (Q12, Q21)
- [x] AC7 (Left): Left on an expanded item collapses only that item, and the highlight stays on it. Left on a leaf, a collapsed subfolder or a non-expanded top-level row does nothing, except in the open QFC list, where AC4 applies. Left never selects a parent: the #440 parent-segment walk and the Left segment hiding are removed in both viewers. When a collapse hides the highlighted row, the highlight moves to its nearest visible ancestor. (Q9, Q10, Q11)
- [x] AC8 (Ctrl+Left): Ctrl+Left selects the parent folder and stops one level below the archive root. On a top-level row it does what a double-click on the parent segment does (AC9). On a subfolder it moves to the parent subfolder or row and leaves that expanded. With the QFC list open it moves only the pending highlight; with the list closed it commits immediately. With the QFC list open, Escape after Ctrl+Left restores both the row's shape (`Archive > Clients > Beta`) and the committed value. Ctrl+Right behaves as Right. (Q19, Q20, Q24)
- [x] AC9 (Segment double-click): in both viewers, a double-click on a path segment selects that folder and shortens the row (`Archive > Clients > Beta` becomes `Archive > Clients [+]`). In QFC it commits and closes the list. Expanding the `[+]` lists every subfolder of Clients instead of restoring Beta. The highlight stays on Clients, and the next Right moves to the first child. In the open QFC list, a single click on a non-leaf segment (`Archive` or `Clients`) moves only the pending highlight to that row and does not close the list; a click on the leaf segment (`Beta`) or the percentage still commits and closes. (Q13, Q18, Q27)
- [x] AC10 (QFC closed list): with the QFC list closed, Right opens the list with the pending highlight on the committed choice: on the committed subfolder (for example Specs under `Archive > Projects > Alpha`) when it is visible, otherwise on its row. Left does nothing. Up and Down keep today's direct commit of the adjacent top-level row. Home and End commit the first and last top-level row. PageUp and PageDown do nothing. (Q14, Q16, Q22, Q25, Q29)
- [x] AC11 (Home/End/PageUp/PageDown): Home and End go to the first and last visible row in tree order, and Home does not move focus to the search box. PageUp and PageDown move by the number of rows fully visible and stop at the first and last row without wrapping. EFC selects as in AC3; with the QFC list open only the pending highlight moves. In EFC with nothing selected, Down and Home select the first row and Up and End select the last row. (Q8, Q16, Q17, Q26)
- [x] AC14 (No `[+]` without subfolders): an expand affordance `[+]` is never shown on a folder that has no subfolders, in either viewer, on top-level rows or on nested subfolders. A folder's has-children flag is derived from the children that resolve in the folder snapshot (the set `FolderTreeSnapshot.GetChildren` returns), not from the raw child-key count. A fetch that attaches zero children leaves no expand or collapse affordance. (Q28)
- [x] AC12 (Regression tests first): each behaviour above has a deterministic regression test (MSTest + Moq + FluentAssertions for C#, node tests under `tests/quickfiler-breadcrumb/` for JavaScript) that fails before the fix and passes after it. Existing tests that pin the removed #440 Left walk, the Left segment hiding and the `[+]` restore are updated with the reason recorded.
- [x] AC13 (Toolchain and coverage): the C# toolchain (CSharpier, analyzers with `/t:Rebuild`, `/p:TreatWarningsAsErrors=true`, `Invoke-MSTestWithCoverage.ps1`) and the JavaScript node tests pass in one clean pass. Line coverage is at least 80% and branch coverage at least 75%, new code reaches at least 90%, changed lines do not lose coverage, and no file exceeds 500 lines.

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
