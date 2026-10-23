# Grill Transcript: Issue #1007 (EFC arrow keys skip subfolders)

## Round 1 (2026-10-22)

### Root-cause hypothesis

- EFC page script (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`, `BridgeJs` keydown handler) posts `arrowKey` with `Up`/`Down` and the id of the selected row only.
- The EFC router (`QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`, `HandleArrowKeyAsync`) handles `Up` with `HandleUpArrow` and `Down` with `MoveSelection`. Both use `FindSelectable` (`BreadcrumbBridgeRouter.Selection.cs`), which walks the top-level `_rows` list only. The keyed descendant tree added by #1002 (`BreadcrumbRow.Descendants.cs`, `_descendants`, `SelectedDescendantId`) is never consulted.
- `SelectRow` calls `row.TrySelectDescendant(null)`, so pressing Down while a subfolder is selected clears the subfolder and jumps to the next top-level row.
- This is consistent with the #1002 spec, decision D3 and its non-goal "No new arrow-key navigation (Left/Right/Up/Down) into or across EFC descendants". #997 also excluded keyboard features. The gap was a recorded scope exclusion, not a regression.
- QFC has the same gap: `FolderBreadcrumb.js` posts `selectorKey` up/down, and `BreadcrumbSelectionSession.Move` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`) walks `_model.Rows` only.
- Existing end behaviour in EFC: Up on the top row returns focus to the search box. Down on the last row does nothing. Neither wraps.

### Questions

Example list used in the questions (row 1 expanded, its subfolder Invoices expanded):

```
1  Archive > Projects > Alpha   [−]
     Specs
     Invoices                  [−]
        2025
        2026
     Notes
2  Archive > Clients > Beta
3  Trash
```

- Q1 Down traversal. A: every visible row in tree order; B: first level only; C: rows only. Recommended A.
- Q2 Up into an expanded block from below. A: lands on the last visible descendant; B: lands on the parent row. Recommended A.
- Q3 Arrow onto a subfolder. A: selects it as a click does, Enter files there; B: focus only until Enter or Space. Recommended A.
- Q4 Left/Right on a highlighted subfolder. A: standard tree view; B: unchanged. Recommended A.
- Q5 Wrap. A: keep today's ends (Up on the top row focuses search, Down on the last row does nothing); B: wrap. Recommended A.
- Q6 QFC parity. A: EFC only; B: EFC and QFC. Recommended B.
- Q7 QFC pending model. A: arrows move the pending highlight, Enter commits, Escape cancels; B: commit immediately. Recommended A.
- Q8 Home/End/PageUp/PageDown. A: out of scope; B: in scope. Recommended A.

### Round 1 answers (user, verbatim as relayed)

- Q1 -> A
- Q2 -> A
- Q3 -> A
- Q4 -> Right expands a row. If already expanded it goes to the first child. Left collapses a tree. It never selects a parent
- Q5 -> A
- Q6 -> B
- Q7 -> A. But with one nuance. When the folder drop down is collapsed but selected, a right arrow expands the drop down. A left arrow when an item is selected but not expanded commits the selection and collapses the drop down.
- Q8 -> B

## Round 2 (2026-10-22)

### Additional facts established

- EFC top-level Left (#440, `BreadcrumbBridgeRouter.Arrows.cs` `HandleArrowKeyAsync`): first moves the row's active breadcrumb segment one step toward the root (`ActivateSegment(index - 1)`), which commits the parent folder; otherwise `BreadcrumbRow.LeftArrow` collapses the leaf expansion, or hides the trailing segment (collapse-after).
- QFC top-level Left (`BreadcrumbStateModel.LeftArrow`): the same parent-segment walk, then leaf collapse. An unhandled Left falls through to `KeyboardHandler.BreadcrumbArrowFallThrough`, which closes the dropdown (`SetFolderDroppedDown(false)`).
- QFC unhandled Right falls through to the "Pop Out Item or Enumerate Conversation?" dialog (`KeyboardHandler.BreadcrumbArrowFallThrough`).
- QFC closed-state Up/Down commit directly (`BreadcrumbSelectionSession.Move`); open-state Up/Down move the pending identity only.
- The "Left never selects a parent" answer conflicts with the existing #440 parent-segment walk on top-level rows in both viewers.

### Round 2 questions (summary)

- Q9 Left on a non-expanded top-level row. A: nothing (removes the #440 parent walk and segment hiding); B: hide the trailing segment; C: keep #440. Recommended A.
- Q10 Left on a leaf or collapsed subfolder. A: nothing (QFC open: Q7 commit-and-close); B: collapse the branch and move to the parent. Recommended A.
- Q11 Left on an expanded item. A: collapse only that item, highlight stays; follow-up: a hidden highlight moves to the nearest visible ancestor. Recommended A, yes.
- Q12 Right on a leaf. A: nothing; B: QFC pop-out dialog. Recommended A for the open dropdown.
- Q13 Right on a trimmed top-level row. A: restore the full path first; B: expand directly. Recommended A.
- Q14 QFC closed dropdown. Right A: open the dropdown; Left A: nothing; Up/Down A: keep direct commit. Recommended A for all three.
- Q15 QFC open, Left on a non-expanded highlighted item. A: commit and close; B: cancel and close. Recommended A.
- Q16 Home/End. A: first/last visible row in tree order; sub-choice for closed QFC i: nothing, ii: commit the first/last top-level row. Recommended A with i.
- Q17 PageUp/PageDown. A: page = rows fully visible; B: fixed 10. Recommended A.

### Round 2 answers (user, verbatim as relayed)

- Q9 is A. But we a way to select the parent as well. Could control + left arrow work in that scenario?
- Q10 A;
- Q11 A yes;
- Q12 B;
- Q13 Yes, but your case is incomplete. Let us assume that `Archive > Clients > Beta` is suggested. Then `Archive > Clients` is selected through a double click, `Beta` should disappear and be replaced with a plus sign. Upon expanding, it should expand every child of Clients.
- Q14 Left A Right A;
- Q15 A;
- Q16 EFC A QFC ii;
- Q17 A;

Parent note: Q14 Up/Down received no answer and remains open.

## Round 3 (2026-10-22)

### Additional facts established

- No Ctrl+arrow handling exists in either page script. EFC `BridgeJs` maps `ArrowLeft`/`ArrowRight` without reading `ctrlKey`; `FolderBreadcrumb.js` `handleKeyDown` does the same. Ctrl+Left therefore behaves as plain Left today.
- EFC segment double-click (`BreadcrumbBridgeRouter.cs`, `SegmentDoubleClick` arm) calls `BreadcrumbRow.CollapseAfter`, which hides the downstream segments and renders a re-expand `[+]` to the left of the now-terminal segment (`BreadcrumbHtmlRenderer`). The first click of the double-click is a single click on a non-leaf segment (`data-segment-activate`), which posts `segmentActivate` and commits the parent path. Clicking `[+]` calls `ReExpand`, which restores the original segments (`Archive > Clients > Beta`); it does not list the children of Clients.
- QFC segment double-click (`FolderBreadcrumbBridgeRouter`, `SegmentDoubleClickMessage`) calls `CollapseAfter` only. QFC has no single-click segment activation, so the selection does not change.

### Round 3 questions (summary)

- Q18 Double-click restatement (`Archive > Clients > Beta`; double-click Clients gives `Archive > Clients [+]` with target Clients, committed and closed in QFC; Right lists Acme, Beta, Gamma with the highlight on Clients). A: the next Right goes to the first child (Acme); B: it goes to Beta. Recommended A.
- Q19 Ctrl+Left selects the parent, like a double-click on the parent segment; it stops below the archive root. EFC subfolders: A moves to the parent subfolder and leaves it expanded; B also collapses it. QFC open: pending highlight only. QFC closed: i commits immediately; ii does nothing. Recommended A and i.
- Q20 Ctrl+Right. A: same as Right; B: restores the suggested path. Recommended A.
- Q21 Right on a leaf, pop-out dialog scope. QFC open: A closes and keeps the original choice, then opens the dialog; B commits, then opens the dialog; C opens the dialog over the open dropdown. QFC closed: Right opens the dropdown first. EFC: no-op. Recommended A.
- Q22 QFC closed Up/Down. A: keep the direct commit; B: Down opens the dropdown. Recommended A.

### Round 3 answers (user, verbatim as relayed)

- "i agree with the recos"

Resolved by the parent: Q18 A; Q19 A for EFC subfolders and i for the closed QFC dropdown; Q20 A; Q21 A for the open QFC dropdown, Right opens the dropdown first when it is closed, and EFC does nothing; Q22 A.

## Frontier

Empty after round 3. The shared-understanding summary below was sent for the user's confirmation.

## Shared Understanding (confirmed by the maintainer on 2026-10-23: "Confirmed")

Scope: EFC and QFC. Rows are visited in visible tree order: top-level rows plus every visible subfolder at any depth.

1. Down and Up visit every visible row in tree order. The children of a collapsed node are skipped. Up from the row below an expanded block lands on the block's last visible descendant. (Q1, Q2)
2. Ends do not wrap. EFC Up on the first row focuses the search box, and Down on the last row does nothing. (Q5)
3. EFC: an arrow onto a row or subfolder selects it, as a click does, and Enter files there. (Q3)
4. QFC with the dropdown open: arrows move only the pending highlight. Enter commits, Escape restores the original choice, and Left on a highlighted item that is not expanded commits it and closes the dropdown. (Q7, Q15)
5. Right on an item that is not expanded and has children expands it. Right on an expanded item goes to its first child, selected in EFC or pending in QFC. Right on a shortened top-level row first restores the full path. (Q4, Q13)
6. Right on a folder with no subfolders: EFC does nothing. With the QFC dropdown open, the dropdown closes, the original choice is kept, and the "Pop Out Item or Enumerate Conversation?" dialog opens. (Q12, Q21)
7. Left on an expanded item collapses only that item, and the highlight stays on it. Left on a leaf, a collapsed subfolder or a non-expanded top-level row does nothing, except when the QFC dropdown is open, where rule 4 applies. Left never selects a parent: the #440 parent-segment walk and the Left segment hiding are removed in both viewers. When a collapse hides the highlighted row, the highlight moves to its nearest visible ancestor. (Q9, Q10, Q11)
8. Ctrl+Left selects the parent and stops below the archive root.
   - On a top-level row it does what a double-click on the parent segment does.
   - On a subfolder it moves to the parent subfolder or row and leaves that expanded.
   - With the QFC dropdown open it moves the pending highlight only; with it closed it commits immediately.
   - Ctrl+Right is the same as Right. (Q19, Q20)
9. Double-click on a path segment (both viewers) selects that folder and shortens the row to `Archive > Clients [+]`; in QFC it commits and closes the dropdown. Expanding the `[+]` lists every subfolder of Clients and does not restore Beta. The highlight stays on Clients, and the next Right goes to the first child. (Q13, Q18)
10. QFC with the dropdown closed: Right opens the dropdown with the pending highlight on the committed row. Left does nothing. Up and Down keep today's direct commit of the adjacent top-level row. Home and End commit the first and last top-level row. (Q14, Q16, Q22)
11. Home and End go to the first and last visible row in tree order. Home does not focus the search box. PageUp and PageDown move by the number of rows fully visible and stop at the first and last row without wrapping. EFC commits, as in rule 3; with the QFC dropdown open only the pending highlight moves. (Q8, Q16, Q17)
12. Route: large path. The change covers 4 or more production files across both viewers (EFC router and page script, the QFC selection session and router, `FolderBreadcrumb.js`, and shared `UtilitiesCS` row and model types), plus the node tests under `tests/quickfiler-breadcrumb/`.
