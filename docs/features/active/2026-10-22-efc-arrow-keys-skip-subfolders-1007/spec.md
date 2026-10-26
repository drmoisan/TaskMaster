# efc-arrow-keys-skip-subfolders (Spec)

- **Issue:** #1007
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-23T12-00
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-bug. This file is the sole authoritative acceptance-criteria source for the item; no `user-story.md` is produced.

> Formatting note for later editors. Backticked repository paths in this document are the change footprint: every file the fix creates or modifies appears at least once as a backticked repository-relative path, and files the fix does not touch are written as plain prose. Do not add backticks to the plain-prose paths in Root Cause Analysis or Scope & Non-Goals. The verbatim acceptance-criteria block is the one exception: it reproduces the maintainer's inline spans unchanged, and the spans there (`Invoke-MSTestWithCoverage.ps1`, the msbuild switch tokens, `FolderTreeSnapshot.GetChildren`) are a script name, command switches and a C# member, not write claims.

## Context

In the EFC viewer's folder tree, expanded subfolder rows can be selected with the mouse but not with the Up and Down arrow keys. Keyboard navigation does not move the selection onto any subfolder. The QFC item-viewer folder dropdown has the same gap. The behaviour is a recorded scope exclusion of #1002 (PR #1003) and #997, not a regression: both specs excluded new arrow-key navigation into descendant rows.

The maintainer settled the target keyboard behaviour for both viewers in four grill-me rounds (grill-transcript.md in this feature folder, rounds 1 to 4 and the Shared Understanding, confirmed on 2026-10-23). The same rounds turned one research question (Q28) into a defect: an expand affordance `[+]` can be shown on a folder with no subfolders. That defect is in scope as AC14.

Environment:
- OS/version: Windows 11, Outlook desktop (VSTO add-in), QuickFiler EFC viewer (WebView2 breadcrumb tree) and QFC item viewer (WebView2 folder dropdown).
- Runtime: C# on .NET Framework 4.8, plus the page scripts `QuickFiler/Resources/FolderBreadcrumb.js` (QFC) and the `BridgeJs` constant in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` (EFC).
- Command/flags used: interactive use of the EFC viewer and the QFC dropdown in Outlook.
- Data source or fixture: any mail folder hierarchy with nested subfolders below the archive root.

Impact / Severity: High. Keyboard-driven filing, the primary QuickFiler workflow, cannot reach any subfolder in either viewer.

Glossary used in this document:
- **Visible rows** (also "visible order"): the top-level rows plus every currently shown subfolder at any depth, in screen order (pre-order). The children of a collapsed node are not visible.
- **Row entry / node entry:** a position in the visible order. A row entry is a top-level row; a node entry is a descendant subfolder of a row.
- **Pending highlight (QFC):** the entry the open QFC list highlights; it becomes the committed choice only on Enter, Left commit-and-close, or a commit gesture.
- **Shortened row:** a top-level row whose trailing segments are hidden by a segment double-click or Ctrl+Left (`Archive > Clients [+]`).

### Decisions

Maintainer decisions from grill-transcript.md. The decision ID is the transcript question number. Research open questions Q-A to Q-G are Q23 to Q29 and are settled.

| ID | Decision |
|---|---|
| D-Q1 | Down and Up visit every visible row in tree order (option A). |
| D-Q2 | Up into an expanded block from below lands on the block's last visible descendant (A). |
| D-Q3 | In EFC an arrow onto a row or subfolder selects it exactly as a mouse click does; Enter files to it (A). |
| D-Q4 | Standard tree semantics: Right expands, or moves to the first child when already expanded; Left collapses; Left never selects a parent. |
| D-Q5 | No wrap. EFC Up on the first row focuses the search box; Down on the last row does nothing (A). |
| D-Q6 | The change covers EFC and QFC (B). |
| D-Q7 | QFC open list: arrows move only the pending highlight; Enter commits; Escape cancels (A). Nuance: with the list closed, Right opens it; Left on a highlighted non-expanded item commits it and closes the list. |
| D-Q8 | Home, End, PageUp and PageDown are in scope (B). |
| D-Q9 | Left on a non-expanded top-level row does nothing; the #440 parent walk and the Left segment hiding are removed (A). Parent selection moves to Ctrl+Left (see D-Q19). |
| D-Q10 | Left on a leaf or a collapsed subfolder does nothing; in the open QFC list D-Q15 applies (A). |
| D-Q11 | Left on an expanded item collapses only that item and the highlight stays; a highlight hidden by a collapse moves to the nearest visible ancestor (A, yes). |
| D-Q12 | Right on a leaf in the open QFC list opens the "Pop Out Item or Enumerate Conversation?" dialog (B); EFC does nothing. |
| D-Q13 | A segment double-click on `Archive > Clients > Beta` at Clients shortens the row to `Archive > Clients [+]`; expanding the `[+]` lists every child of Clients. |
| D-Q14 | QFC closed list: Left does nothing; Right opens the list (A, A). Up and Down were settled by D-Q22. |
| D-Q15 | QFC open list: Left on a highlighted non-expanded item commits and closes (A). |
| D-Q16 | Home and End go to the first and last visible row (A). QFC closed list: Home and End commit the first and last top-level row (ii). |
| D-Q17 | PageUp and PageDown move by the number of rows fully visible (A). |
| D-Q18 | After a double-click shortens the row and Right lists the children, the highlight stays on Clients and the next Right goes to the first child (A). |
| D-Q19 | Ctrl+Left selects the parent and stops below the archive root. On an EFC subfolder it moves to the parent and leaves it expanded (A). QFC open: pending highlight only. QFC closed: commits immediately (i). |
| D-Q20 | Ctrl+Right behaves as Right (A). |
| D-Q21 | Right on a leaf in the open QFC list closes the list, keeps the original choice, then opens the pop-out dialog (A). QFC closed: Right opens the list first. EFC: no-op. |
| D-Q22 | QFC closed list: Up and Down keep today's direct commit of the adjacent top-level row (A). |
| D-Q23 | On a shortened row, AC9 governs: Right lists the segment's subfolders. The "restore the full path" clause of Shared Understanding rule 5 is dropped (A). |
| D-Q24 | QFC open list: Escape after Ctrl+Left restores both the row shape and the committed value (A). |
| D-Q25 | QFC closed list: PageUp and PageDown do nothing (A). |
| D-Q26 | EFC with nothing selected: Down and Home select the first row; Up and End select the last row (A). |
| D-Q27 | QFC open list: a single click on a non-leaf segment moves only the pending highlight and does not bubble; a double-click commits that segment's folder and closes; a click on the leaf segment or the percentage still commits (A). |
| D-Q28 | Not an option choice: a `[+]` on a folder with no subfolders is a bug. Recorded as the AC14 invariant; root cause under "Q28 investigation" in the transcript. |
| D-Q29 | QFC closed-list Right with a committed subfolder: the pending highlight goes on that subfolder when it is visible, otherwise on its row (A). |

Spec-level design decisions (this document; they implement the D-Q decisions and do not change their meaning):

| ID | Decision | Rationale |
|---|---|---|
| S1 | #1002 invariant I3 ("a toggle never changes the selection") is narrowed to "unless the collapse hides the selected node", in which case the selection moves to the nearest visible ancestor. | Required by AC7 (D-Q11). The existing test `DescendantToggle_DoesNotChangeSelection` toggles a node below an unselected position and stays valid. |
| S2 | One pure visible-order helper in `UtilitiesCS` serves both viewers, exposed through public members. | `BreadcrumbDescendantNode` is `internal` and UtilitiesCS grants `InternalsVisibleTo` only to the test assemblies, so the QuickFiler router cannot walk the nodes itself. |
| S3 | Each page script measures the fully visible row count and posts it as `pageSize`; C# moves by that count. A missing, zero or negative `pageSize` makes the key a logged no-op. | C# has no viewport. Rejecting invalid input explicitly follows the fail-fast policy; a fixed size was rejected by the maintainer (D-Q17). |
| S4 | EFC Ctrl+Left is a new `arrowKey` value `ParentLeft`. QFC Ctrl+Left is a new selector key `parent`, not a new arrow direction. | A new `BreadcrumbArrowDirection` value would reach `KeyboardHandler.BreadcrumbArrowFallThrough`, which treats every non-Right direction as Left. |
| S5 | The QFC segment double-click moves to the selector channel as a new `selectorSegmentDoubleClick` message so the coordinator can publish the commit and close transitions. The bridge-channel `segmentDoubleClick` arm in the QFC router becomes a logged no-op. | `FolderBreadcrumbBridgeRouter.RouteAsync` returns JSON strings, not a `BreadcrumbSelectionTransition`, so it cannot commit and close. A logged no-op prevents a stale page from shortening a row without committing it. |
| S6 | The QFC `onArrow` pre-filter is removed: Left and Right always post `arrowKey`, and C# decides. | The pre-filter reads the committed row, which is wrong when the pending row differs (for example Trash committed, a suggestion pending). |
| S7 | The EFC row-leaf expansion becomes idempotent (expand-to-true) and guarded against overlapping fetches. | Fixes the re-entrant Right defect (Root Cause item 5) on the AC5 path. |
| S8 | AC14 is fixed at the source (has-children derived from resolvable snapshot children) and defended in depth in every renderer (no affordance after a fetch that attached zero children). | Matches the Q28 investigation's "primary" and "defence in depth" requirements. |
| S9 | QFC `GetSelectedFolder` returns the active-segment folder for a shortened or parent-activated row through a new `BreadcrumbSelectionMap.SelectedRowValue`. All other map queries keep `RowValue`. | AC8 and AC9 require the shortened row to file to the folder it displays; the presented targets must not change. |
| S10 | The EFC `SegmentDoubleClick` arm becomes atomic: activate the segment if it is not already active, commit it through `SelectHierarchyPath`, then `CollapseAfter`. The "already active" second-click rejection is downgraded from an error log. | Removes the dependency on click, click, dblclick ordering in WebView2. |

## Repro & Evidence

Steps to Reproduce:
1. Open the EFC viewer on a mail item.
2. Expand a folder row so that its subfolder rows are shown.
3. Click a subfolder row: it is selected, as expected after #1002.
4. Press the Up or Down arrow key to move through the list.

Expected:
The arrow keys move the selection through the visible rows, including expanded subfolder rows, the same way mouse selection reaches them.

Actual:
The arrow keys do not select any subfolder row. Keyboard navigation reaches only the top-level rows. Pressing Down while a subfolder is selected clears the subfolder and jumps to the next top-level row.

Second reproduction (QFC): open the QFC folder dropdown, expand a row, and press Down. The pending highlight skips the shown subfolders.

Third reproduction (AC14): with a folder whose reader-reported child key does not resolve in the folder snapshot, the row shows `[+]`; expanding it shows `[−]` with nothing below it.

Logs / Screenshots: reported by the maintainer from interactive use; no log captured. The code-level evidence is in the research record (research/2026-10-23T10-40-arrow-key-subfolder-navigation-research.md, sections 2 and 3) and the transcript's "Q28 investigation".

## Scope & Non-Goals

- In scope:
  - EFC breadcrumb keyboard navigation over the visible order: Up, Down, Left, Right, Ctrl+Left, Ctrl+Right, Home, End, PageUp, PageDown, and the no-selection start (AC1 to AC3, AC5 to AC8, AC11).
  - QFC item-viewer dropdown keyboard navigation in the open and closed states, including the pending-highlight model, Enter, Escape, Left commit-and-close, closed-list Right opening the list, and the leaf pop-out sequence (AC4 to AC8, AC10, AC11).
  - Segment double-click and the `[+]` expansion in both viewers, and the QFC open-list single click on a non-leaf segment (AC9).
  - Removal of the #440 Left parent walk and the EFC Left segment hiding in both viewers (AC7).
  - The AC14 has-children defect in the hierarchy provider and the zero-children defence in the EFC renderer, the QFC projection and the QFC page script.
  - The re-entrant Right defect in the EFC row-leaf expansion, because AC5 is its trigger.
  - Regression tests, rewrites of tests that pin removed behaviour, and the matching C# string-contract tests for both page scripts (AC12, AC13).
- Out of scope / non-goals:
  - TaskMaster and QuickFiler folder views other than the EFC breadcrumb and the QFC item-viewer dropdown (for example the folder-filter dialogs and any other folder picker).
  - Wrap-around at either end of any list (D-Q5).
  - New keyboard features beyond AC1 to AC14: no type-ahead search, no multi-select, no new shortcuts other than those listed in AC1 to AC11.
  - Any change to filing itself, to suggestion scoring, or to the archive-root rules (#614), which this change consumes but does not alter.
  - Running the node suites in CI. CI has no node step; the node suites are paired with C# string-contract tests instead (see Test Strategy).
- Explicitly excluded systems, integrations, or datasets (paths deliberately unbackticked because they are not changed):
  - QuickFiler/Controllers/KeyboardHandler.cs (coverage-exempt; its fall-through is reached in the same way as today).
  - QuickFiler/Controllers/EfcFormController.Breadcrumb.cs (the wiring, including `FocusSearchRequested`, is unchanged).
  - QuickFiler/Viewers/BreadcrumbMessengerHub.cs, QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs and QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs (the added `pendingNodeIdentity` field passes through the hub unchanged).
  - UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs, UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshot.cs and UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs.
  - The Outlook folder tree service, the snapshot reader and any COM interop.

## Root Cause Analysis

1. **EFC Up and Down walk only the top-level rows.** The page script (`BridgeJs` in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`) posts `arrowKey` with `Up` or `Down` and the selected row id. In `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`, `HandleArrowKeyAsync` handles Up with `HandleUpArrow` and Down with `MoveSelection`. Both use `FindSelectable` and `IndexOf` in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`, which walk `_rows` only. The keyed descendant tree added by #1002 (`_descendants`, `SelectedDescendantId` in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs`) is never consulted, and `SelectRow` calls `TrySelectDescendant(null)`, so Down from a selected subfolder clears it and jumps to the next row.
2. **QFC navigation walks only rows and acts on the committed row.** `BreadcrumbSelectionSession.Move` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`) walks `_model.Rows` with `NextSelectableIndex`, and the session tracks pending state by row identity only (`PendingIdentity`). QFC Left and Right go through `FolderBreadcrumbBridgeRouter.ArrowAsync` (`UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`), which calls `_model.RightArrow()` or `_model.LeftArrow()` on the committed row (`BreadcrumbStateModel.SelectedRow`), not on the pending highlight. The page-side `onArrow` pre-filter in `QuickFiler/Resources/FolderBreadcrumb.js` also decides from the committed row.
3. **The gap was a recorded scope exclusion.** The #1002 spec (docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md), decision D3, limited EFC keyboard support to toggle parity with #997, and its non-goals state "No new arrow-key navigation (Left/Right/Up/Down) into or across EFC descendants". The #997 spec (docs/features/archive/2026-10-17-quickfiler-recursive-folder-selection-997/spec.md) likewise excluded new keyboard features. The existing #440 arrow transitions (parent-segment walk on Left, descent on Right) were kept unchanged and now conflict with D-Q4 and D-Q9.
4. **`[+]` on a folder with no subfolders (AC14).** `OutlookFolderHierarchyProvider.MapNode` (`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`) sets `HasChildren` to `node.ChildKeys.Count > 0`. Both `GetAncestorChainAsync` and `GetImmediateSubfoldersAsync` map through it. `FolderTreeSnapshot.GetChildren` returns only the child keys present in the snapshot, and the snapshot builder skips a child key with no node. When the reader reports a child key whose folder is absent from the snapshot, the segment carries `HasChildren == true`, every renderer draws `[+]`, and the fetch returns zero children; the expanded state then shows `[−]` with nothing below it. The renderers involved are `BreadcrumbHtmlRenderer` (leaf affordance and descendant toggle), `BreadcrumbRenderProjection` and `makeSubfolders` in `FolderBreadcrumb.js`.
5. **Re-entrant Right on the EFC row leaf.** `BreadcrumbBridgeRouter.ExpandLeafAsync` ends with `row.ToggleLeafExpanded()`. If a second Right (for example key auto-repeat) arrives while the first provider fetch is still awaited, both completions toggle and the row ends collapsed. The descendant path has a generation guard (`TryBeginDescendantExpand`); the row-leaf path has none. Today the path is reachable by mouse double-toggle; AC5 makes it reachable by a held key.
6. **Secondary causes that block specific criteria.**
   - QFC `BreadcrumbSelectionMap.GetSelectedFolder` returns the leaf `FolderPath` and ignores `ActiveSegmentIndex`, so a QFC parent selection never changes the filing value (blocks AC8 and AC9).
   - In the open QFC list a single click on any segment bubbles to the row's `click` listener and commits the row before a `dblclick` can arrive (blocks AC9 in the open list).
   - The `[+]` restore (`ReExpand`) in EFC `HandleLeafToggleAsync` and Right, and in QFC `ToggleAsync` and `BreadcrumbStateModel.RightArrow`, restores the trailing segments instead of listing the active segment's children (conflicts with AC5 and AC9).
   - `BreadcrumbRow.TryCollapseDescendant` leaves `SelectedDescendantId` on a hidden node, and `ToggleLeafExpanded(false)` and `CollapseAfter` clear the descendant while `SelectedFolderPath` keeps the hidden stem (blocks AC7's nearest-visible-ancestor rule).

## Proposed Fix

### Design summary (what changes where):

- **Shared visible-order helper (UtilitiesCS).** A new pure static `BreadcrumbVisibleOrder` in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs` with a public immutable `BreadcrumbVisiblePosition` (`RowIndex`, `NodeId` null for a row entry, `ParentNodeId`, `Depth`, `HasChildren`, `IsExpanded`). It flattens each row's descendants in pre-order (a node's children are included only when it is expanded) and exposes pure index operations: `IndexOf(order, rowIndex, nodeId)`, `Step(order, index, delta)` clamped without wrap and returning -1 when there is no move, `First` and `Last`. `BreadcrumbVisiblePosition` is a plain sealed class or `readonly struct`, not a `record`, because net48 has no `IsExternalInit`.
  - EFC entry point: a public `BreadcrumbRow.AppendVisiblePositions(int rowIndex, List<BreadcrumbVisiblePosition> target)` in `BreadcrumbRow.Descendants.cs`; first-level nodes are visible when `IsLeafExpanded`. Banner rows are skipped, `TrashPseudoRow` is included (the `FindSelectable` rule).
  - QFC entry point: an internal `BreadcrumbStateModel.VisiblePositions()`; first-level nodes are `DescendantRoot.Children`, visible when `LeafExpanded`; non-selectable rows are skipped (the `NextSelectableIndex` rule).
- **EFC router (QuickFiler).** Navigation moves to a new partial `QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs`. The position is the existing pair `_selectedRowId` plus the row's `SelectedDescendantId`; no new state. Moving onto a node commits through `SelectHierarchyPath(row, row.GetDescendantPath(nodeId), nodeId)`, the `ActivateDescendant` path (AC3). Moving onto a row entry uses a new `SelectRowEntry(row)`: today's `SelectRow` when the active segment is the leaf, otherwise `SelectHierarchyPath(row, ActiveSegment.FullPath)`. New key arms: `ParentLeft`, `Home`, `End`, `PageUp`, `PageDown`; `default` keeps logging unknown keys. The #440 Left walk, the Left segment hiding and `TryRightTreeTransitionAsync` are removed. Right and Left follow the decision table below. The `[+]` on a shortened row expands the active segment (one provider query) instead of calling `ReExpand`. The collapse that hides the selected node re-commits the nearest visible ancestor (S1). The re-entrancy guard is added to `ExpandLeafAsync` (S7).
- **QFC session, router and coordinator.** A new partial `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs` adds `PendingNode` next to `PendingIdentity`, the open-list visible-order walk, `MoveToBoundary(first)`, page moves, the Ctrl+Left parent move and a row-shape snapshot (`ActiveSegmentIndex`, `CollapsedAfterIndex`) taken on open and restored by `Cancel` (D-Q24). `CommitPending` calls `_model.SelectDescendant(rowIndex, PendingNode)` when a node is pending. The closed-list `Move` is unchanged (AC10). A new partial `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs` makes Left and Right act on the pending entry when the list is open. A new partial `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs` routes closed-list Right to `OpenSelector()` (consuming the message), consumes closed-list Left, maps open-list Left on a non-expanded entry to `CommitSelector()`, sequences the open-list leaf Right as `CancelSelector()` then `UnhandledArrow(Right)` (D-Q21), and handles `selectorSegmentDoubleClick` (S5).
- **Model and projection changes.** `BreadcrumbStateModel.LeftArrow` and `RightArrow` lose the #440 walks; `ActiveSegmentHasSubfolders` and `TryExpandActiveSegment` accept `CollapsedAfterIndex == ActiveSegmentIndex`; `BreadcrumbRenderProjection.ProjectRow` emits `Minus` at the shortened terminal while expanded. EFC `BreadcrumbRow.LeftArrow` loses the trailing-segment hiding, `ToggleLeafExpanded` allows expansion while `CollapsedAfterIndex == ActiveSegmentIndex`, and `BreadcrumbHtmlRenderer.AppendSuggestionRow` renders a collapse affordance at the terminal segment while expanded.
- **AC14.** `OutlookFolderHierarchyProvider.MapNode` derives `HasChildren` from `snapshot.GetChildren(node.Key).Count > 0`, so every mapped segment agrees with the set the subfolder fetch returns. Defence in depth: after a fetch that attaches zero children, the EFC renderer (row leaf and descendant toggle), the QFC projection and `makeSubfolders` show no expand or collapse affordance on that row or node.
- **Page scripts.** `BridgeJs`: Ctrl+ArrowLeft maps to `ParentLeft`, Ctrl+ArrowRight to `Right`, Home, End, PageUp and PageDown map to their key values, and PageUp and PageDown carry a measured `pageSize`; the post-message scroll targets the selected `.child` when present, else `.rowwrap.selected`. `FolderBreadcrumb.js`: `selectorKeys` gains Home, End, PageUp and PageDown (with `pageSize`), Ctrl+ArrowLeft posts `{ type: "selectorKey", key: "parent" }`, `onArrow` always posts `arrowKey` (S6), `makeSubfolders` marks `.sub.active` where `child.identity === state.pendingNodeIdentity` on the pending row, a single click on a non-leaf segment in the open list stops propagation and posts a pending move, and the segment `dblclick` posts `selectorSegmentDoubleClick`. The fully-visible count is a small pure function (for example `fullyVisibleCount(rects, top, bottom)`, minimum 1) so node tests can drive it with stub rectangles.

**Right and Left decision table** (P is the current position: the EFC selection, or the QFC pending entry when the list is open):

| P | Right | Left |
|---|---|---|
| Row, has children, not expanded | Expand (EFC `ExpandLeafAsync`; QFC `TryExpandLeaf` or `TryExpandActiveSegment` plus fetch) | Nothing (QFC open: commit and close) |
| Row, expanded | Move to the first visible child (EFC select; QFC pending) | Collapse the row's expansion; P stays |
| Row, no children (leaf, Trash, plain or fallback row) | EFC: nothing. QFC open: cancel, close, pop-out dialog | Nothing (QFC open: commit and close) |
| Node, has children, not expanded | Expand (EFC `HandleDescendantToggleAsync`; QFC `ToggleDescendantAsync`) | Nothing (QFC open: commit and close) |
| Node, expanded, children loaded | Move to the first child | Collapse; P stays |
| Node, expanded, fetch in flight | Nothing | Collapse; P stays |
| Node, no children | As for a leaf row | As for a leaf row |

After AC14, an expanded node with zero loaded children carries no affordance and is treated as "no children" (D-Q28).

**Ctrl+Left** (AC8): on a node, move to the parent node, or to the row entry at depth 0, keeping the expansion; on a row entry, perform the parent-segment double-click (activate `ActiveSegmentIndex - 1`, commit, `CollapseAfter`); with the active index at 0, do nothing. Segment index 0 is already one level below the archive root because `GetAncestorChainAsync` trims every chain below the root. QFC open: pending only; QFC closed: commit immediately. Ctrl+Right is Right.

### Boundaries and invariants to preserve:

- **#614 archive-root rules.** EFC commits only through `SelectHierarchyPath`, which enforces `ArchiveStemContract.TryMakeArchiveRelative` and rejects the root itself and paths outside it. QFC `BreadcrumbSelectionMap.TryDescendantValue` rejects an empty stem when `ArchiveRoot` is set, and the new `SelectedRowValue` uses the same conversion. Ctrl+Left never selects the archive root.
- **#1002 invariants I1 to I6**, with the single narrowing S1:
  - I1: a descendant is addressed only by a router-issued `nodeId` present in the addressed row; others are logged with no state change.
  - I2: a provider result is applied only when the row instance, node presence, expansion and `RequestGeneration` still match; this now also covers the re-entrant row-leaf expansion (S7).
  - I3: a toggle changes expansion only, except when the collapse hides the selected node (S1).
  - I4: descendant commits go through `SelectHierarchyPath`; out-of-root targets are non-selections.
  - I5: the trash gesture's selection is established by the bind itself.
  - I6: all EFC router state is mutated on the UI context; new awaits in router files do not use `ConfigureAwait(false)`; no lock is introduced.
- **Failure handling stays a logged no-op with unchanged state** for unknown row ids, unknown node ids, rejected archive-relative paths, invalid `pageSize` and provider faults.
- **Tests:** no temporary files; no `Task.Delay`, `Thread.Sleep` or wall-clock waits (in-flight provider calls use a `TaskCompletionSource`); tests are safe under parallel execution (`Workers = 0`, `ClassLevel`) with no shared static mutable state; node GUIDs are never asserted as literals.
- **500-line limit (total lines).** These files are at or near the limit and must not be extended; new code goes into the new partials named below:
  - Production: `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (471), `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (451, routing edits only), `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` (446, call-throughs into the Tree partial only), `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` (404, avoid growth), `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` (408, guard changes only). Not touched: BreadcrumbItemViewerLifecycleCoordinator.cs (497), BreadcrumbMessengerHub.cs (490), KeyboardHandler.cs (414).
  - Tests, edited in place with no net growth: `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` (500), `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (495), `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` (488), `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` (462), `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` (454). Not to be extended if touched: BreadcrumbBridgeRouterIssue1002Tests.cs (487), BreadcrumbRowDescendantsIssue1002Tests.cs (466), BreadcrumbBridgeRouterQueueTests.cs (462), BreadcrumbBridgeRouterIssue439Tests.cs (455), OutlookFolderHierarchyProviderTests.cs (480).
  - `QuickFiler/Resources/FolderBreadcrumb.js` (350) may grow but stays under 500.

### Dependencies or blocked work:

- Builds on #1002 (descendant tree, `EfcDescendantRouterFixture`, I1 to I6) and #997 (QFC descendant model and selector session). Both are on `main` in this branch's base.
- No external dependency, package or configuration change. No blocking issue.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

Production (modify):
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`: drop the trailing-segment hiding from `LeftArrow`; allow expansion while `CollapsedAfterIndex == ActiveSegmentIndex`; add an idempotent expand-to-true operation.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs`: `AppendVisiblePositions`; collapse operations that report whether the selected node was hidden.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`: collapse affordance at the shortened terminal while expanded; no affordance after a zero-children fetch.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`: `BridgeJs` key mapping, `pageSize`, scroll target.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` and `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs`: optional `pageSize`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` and `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs`: remove the #440 walks; shortened-anchor expansion; `VisiblePositions()`; zero-children has-subfolders state.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`: `Cancel` and `CommitPending` call into the Tree partial.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs`: `SelectedRowValue`, used only by `GetSelectedFolder`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs`: new selector keys, `pageSize`, `pendingNodeIdentity`, `selectorSegmentDoubleClick`, `selectorHighlight`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs`: `Minus` at the shortened terminal while expanded; no affordance after a zero-children fetch.
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` and `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs`: delegate arrows to the Navigation partial; `[+]` expands instead of `ReExpand`; the AC7 pending move on a hiding collapse; the bridge `segmentDoubleClick` arm becomes a logged no-op.
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`: `MapNode` has-children from resolvable children (AC14).
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`: dispatch of the new key arms and the atomic `SegmentDoubleClick` arm, delegating to the Navigation partial.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`: rewritten Right and Left; #440 walk and `TryRightTreeTransitionAsync` removed; re-entrancy guard in `ExpandLeafAsync`; `[+]` expansion in `HandleLeafToggleAsync`.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`: `SelectRowEntry`.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs`: AC7 re-commit of the nearest visible ancestor.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`: routing only, delegating to the Keyboard partial; `PostSelectorStateCore` passes `pendingNodeIdentity`.
- `QuickFiler/Resources/FolderBreadcrumb.js`: as in the design summary.

Production (create), each with a `<Compile Include>` in its legacy project file:
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs` in `UtilitiesCS/UtilitiesCS.csproj`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs` in `UtilitiesCS/UtilitiesCS.csproj`.
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs` in `UtilitiesCS/UtilitiesCS.csproj`.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs` in `QuickFiler/QuickFiler.csproj`.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs` in `QuickFiler/QuickFiler.csproj`.

Tests (create), each with a `<Compile Include>` in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` or `QuickFiler.Test/QuickFiler.Test.csproj`:
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbVisibleOrderTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTreeIssue1007Tests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderIssue1007Tests.cs` (AC14 primary)
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbEmptyExpanderIssue1007Tests.cs` (AC14 renderer and projection defence)
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeJsKeysIssue1007Tests.cs` (EFC page-script string contract)
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.cs`, with `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.Navigation.cs` and `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs` partials to stay under 500 lines
- `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs`
- `QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs` (QFC page-script string contract)
- `tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs` (node; no project entry)

Tests and fixtures (modify): `tests/quickfiler-breadcrumb/issue-997-browser-harness.mjs` (`getBoundingClientRect` stubs), `tests/quickfiler-breadcrumb/issue-997-common.test.mjs`, and the existing C# tests listed under Test Strategy, "Existing tests whose disposition changes".

#### Functions/classes/CLI commands impacted:

- EFC: `BreadcrumbBridgeRouter.HandleArrowKeyAsync`, `HandleUpArrow`, `MoveSelection`, `FindSelectable`, `SelectRow`, `ExpandLeafAsync`, `HandleLeafToggleAsync`, `TryRightTreeTransitionAsync` (removed), the `SegmentDoubleClick` arm, `HandleDescendantToggleAsync`; `BreadcrumbRow.LeftArrow`, `ToggleLeafExpanded`, `TryCollapseDescendant`; `BreadcrumbHtmlRenderer.AppendSuggestionRow`, `AppendLeafAffordance`, `AppendDescendants`; `BreadcrumbMessageCodec.DeserializeInbound`; `BreadcrumbInboundMessage` (new optional `PageSize`).
- QFC: `BreadcrumbSelectionSession.Move`, `CommitPending`, `Cancel`, `Open`; `BreadcrumbSelectorState` (new `PendingNodeIdentity`); `BreadcrumbSelectorViewMessage`, `BreadcrumbSelectorKeyMessage`, `BreadcrumbSelectorKey`, `BreadcrumbSelectorMessageSerializer.ParseKey`; `FolderBreadcrumbBridgeRouter.ArrowAsync`, `RouteAsync`, `ToggleAsync`; `BreadcrumbStateModel.LeftArrow`, `RightArrow`, `TryRightTreeTransition`; `BreadcrumbStateRow.ActiveSegmentHasSubfolders`, `LeafHasSubfolders`; `BreadcrumbSelectionMap.GetSelectedFolder`; `BreadcrumbRenderProjection.ProjectRow`, `ProjectDescendant`; `BreadcrumbBridgeCoordinator.HandleSelectorMessage`, `HandleSelectorKey`, `PostSelectorStateCore`, `DispatchAsync`.
- Shared: `OutlookFolderHierarchyProvider.MapNode`, `MapNodes`.
- Page scripts: `BridgeJs` keydown and click handlers; `FolderBreadcrumb.js` `handleKeyDown`, `selectorKeys`, `onArrow`, `makeRow`, `makeCell`, `makeSubfolders`, `render`.
- No CLI commands are affected.

#### Data flow and validation changes:

- EFC keydown → `arrowKey { rowId, key, pageSize? }` → codec (non-empty `key`; `pageSize` optional integer) → router: compute the visible order, locate the current position, step, commit through `SelectHierarchyPath` or `SelectRowEntry` → full-list render → page scroll to the selected element.
- EFC with no selection (`rowId` empty): Down and Home select the first entry, Up and End the last (D-Q26), instead of today's rejection of the unknown row.
- QFC keydown → `selectorKey { key, pageSize? }` → coordinator → session (open: pending walk over the visible order; closed: unchanged row commit, Home and End commit the first and last top-level row, PageUp and PageDown ignored) → `selectorView { …, pendingNodeIdentity }` → page highlight.
- QFC Left and Right → `arrowKey` always (S6) → coordinator Keyboard partial: closed Right opens the selector, closed Left is consumed, open Left on a non-expanded entry commits and closes, open Right on a leaf cancels then raises `UnhandledArrow(Right)`; everything else goes to the router's pending-aware Navigation partial.
- Validation: `pageSize` absent, zero or negative on a page key is a logged no-op; selector key parsing accepts `parent`, `home`, `end`, `pageup`, `pagedown` (matching `Serialize`'s lower-cased enum name); unknown keys remain rejected.

#### Error handling and logging updates:

- New key arms log and ignore an invalid `pageSize`, an unknown row or node, and a rejected archive-relative path, using the router's existing log4net logger; state is unchanged.
- The EFC "segment already active" rejection on the second click of a double-click is downgraded from an error to a debug entry (S10).
- The QFC bridge-channel `segmentDoubleClick` arm logs that the message is retired and changes no state (S5).
- No broad catch is added. Provider faults keep today's handling in `FetchAndAttachSubfoldersAsync` and the EFC expansion paths.

#### Rollback/feature-flag considerations (if applicable):

- No feature flag. The change is a single revertible merge; reverting it restores the #440 behaviour and the leaf-only navigation. No persisted data or settings change, so rollback needs no migration.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- EFC inbound `arrowKey`: `{ "type": "arrowKey", "rowId": string, "key": "Up" | "Down" | "Left" | "Right" | "ParentLeft" | "Home" | "End" | "PageUp" | "PageDown", "pageSize"?: integer }`. `pageSize` is required for `PageUp` and `PageDown` and ignored otherwise.
- QFC inbound `selectorKey`: `key` adds `parent`, `home`, `end`, `pageup`, `pagedown` to the existing `up`, `down`, `enter`, `escape`; optional `pageSize` (non-negative integer when present; zero is rejected as a no-op by the session).
- QFC inbound `selectorSegmentDoubleClick` (new): `{ "type": "selectorSegmentDoubleClick", "rowIdentity": string, "segmentIndex": integer }`.
- QFC inbound `selectorHighlight` (new; planner amendment, 2026-10-23, because AC9's open-list non-leaf single click needs a host message to move the host-owned pending highlight): `{ "type": "selectorHighlight", "identity": string }`. Open session: the pending highlight moves to that row and any pending node is cleared; closed session or unknown identity: no-op.
- QFC outbound `selectorView`: new optional `pendingNodeIdentity` (string or absent). The hub's `mode` rewrite passes it through unchanged.
- `BreadcrumbVisiblePosition`: `RowIndex` (int), `NodeId` (string or null), `ParentNodeId` (string or null), `Depth` (int, 0 for a first-level node; the row entry is -1 or a separate flag, decided in the plan), `HasChildren` (bool), `IsExpanded` (bool).

#### Required configuration keys and defaults:

- None. No settings, `app.config` or runsettings change.

#### Backward-compatibility expectations:

- `BreadcrumbInboundMessage`, `BreadcrumbSelectorViewMessage` and `BreadcrumbSelectorKeyMessage` gain optional constructor parameters with defaults, so existing call sites compile unchanged.
- Messages without the new fields deserialize as before. The page scripts and the C# side ship together in the same assembly resources, so no mixed-version page exists in production; the S5 no-op covers a stale page in development.
- Intentional behaviour removals (called out as breaking for keyboard users): the #440 Left parent walk and Right descent in both viewers, the EFC Left segment hiding, the `[+]` restore of trailing segments, and the QFC open-list single-click commit on a non-leaf segment.

#### Performance constraints (latency/throughput/memory):

- The visible order is recomputed per key press in O(visible entries). Lists hold tens of rows and a bounded number of expanded descendants, so no caching is required. AC14's `GetChildren` call in `MapNode` is O(child keys) per mapped node over an already-acquired snapshot and adds no COM call.
- No new provider calls on navigation; a provider call occurs only on an expansion, as today. The re-entrancy guard (S7) prevents duplicate fetches from key auto-repeat.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - The archive root is bound in both production constructions, so segment index 0 is one level below it (`GetAncestorChainAsync` trims below the root).
  - Production EFC children are always keyed nodes (`SetLeafDescendants`); the key-less `SetLeafChildren` path is test-only and is not navigated.
  - No coverage baseline exists yet in this feature folder. The AC13 floors are evaluated on the testable denominator defined in CLAUDE.md UT2; the baseline is recorded in Phase 0 before any code change.
  - No acceptance criterion asserts a numeric count, enumeration or population; the research record's Numeric Derivation Evidence section states this, so no derivation block is required.
- Constraints (budget, performance, compatibility):
  - Large path: the change spans more than four production files across two projects plus page scripts (Shared Understanding rule 12).
  - .NET Framework 4.8: no `record`, `init` or `record struct`.
  - The 500-line ceiling (total lines) as listed under Boundaries.
  - Tests use MSTest, Moq and FluentAssertions; no temporary files; no real waits; parallel-safe.
- External dependencies (services, libraries, releases): none new. WebView2 and Outlook are exercised only in the manual verification step.

## Data / API / Config Impact

- User-facing or API changes: the keyboard behaviour in AC1 to AC11 in both viewers; the double-click and `[+]` behaviour in AC9; the removed #440 Left walk, Left segment hiding and `[+]` restore; no `[+]` on folders without subfolders (AC14). The message contracts change as listed under Inputs/outputs; all additions are optional fields or new enum values.
- Data or migration considerations: none. No persisted state, settings file or snapshot format changes.
- Logging/telemetry updates (if any): debug and warning entries for rejected keys, invalid `pageSize` and the retired QFC bridge double-click; the second-click rejection downgrade (S10). No telemetry.
- Compatibility notes (CLI flags, config schemas, versioning): no CLI or config schema. The `BreadcrumbSelectorKey` enum and the EFC key string set grow; existing values keep their meaning.

## Test Strategy

Bugfix workflow: failing regression tests come first. Every behaviour in AC1 to AC11 and AC14, and the re-entrant Right defect, gets a deterministic test that is captured failing before the production change and passing after it. C# tests use MSTest, Moq and FluentAssertions. JavaScript behaviour is tested under `tests/quickfiler-breadcrumb/` with `node --test`; because CI runs no node step, every node assertion is paired with a C# string-contract assertion over `BreadcrumbDocumentAssets.BridgeJs` or the QFC document HTML.

Seeded from the issue (now covered by the items below):
- Unit coverage areas: arrow navigation over a row list that contains expanded descendant rows (C# router tests and node tests under `tests/quickfiler-breadcrumb/`).
- Integration scenario to retest: Up and Down across parent, nested subfolder, and next top-level rows.
- Manual verification: confirm in Outlook that the arrow keys select subfolder rows in the EFC viewer.

- Regression tests to add or update:
  - Pure helper: `BreadcrumbVisibleOrderTests` (pre-order with collapsed children skipped, no wrap, `First`, `Last`, page clamping, empty order).
  - EFC router (`BreadcrumbBridgeRouterIssue1007Tests` and partials) through `ProcessInboundAsync` with mocked `IFolderHierarchyProvider`, `IBreadcrumbWebHost` and `IWebViewMessenger`, reusing `EfcDescendantRouterFixture` and extracting node ids from rendered `data-node-id` attributes.
  - QFC session (`BreadcrumbSelectionSessionTreeIssue1007Tests`), router (`FolderBreadcrumbBridgeRouterIssue1007Tests`) and coordinator (`BreadcrumbBridgeCoordinatorIssue1007Tests`, reusing `SubfolderActivationHarness` or `ItemViewerDropDownHarness`).
  - AC14: `OutlookFolderHierarchyProviderIssue1007Tests` (a reader-reported child key absent from the snapshot yields `HasChildren == false` from both `GetAncestorChainAsync` and `GetImmediateSubfoldersAsync`; a resolvable child yields true) and `BreadcrumbEmptyExpanderIssue1007Tests` (zero-children fetch renders no affordance in the EFC renderer and the QFC projection).
  - Re-entrant Right: hold the first provider call on a `TaskCompletionSource`, deliver a second Right, complete both, assert the row is expanded and the provider was queried once.
  - Page scripts: `issue-1007-keys.test.mjs` (Ctrl+Left, Ctrl+Right, Home, End, PageUp, PageDown with `pageSize` from stub rectangles, pending-node `.sub.active`, non-bubbling non-leaf segment click, `selectorSegmentDoubleClick`, `onArrow` always posting `arrowKey`), paired with `FolderBreadcrumbKeysIssue1007Tests` and `BreadcrumbBridgeJsKeysIssue1007Tests`.
- Existing tests whose disposition changes (each rewritten test's summary names "#1007" and the governing AC and records the reason):

  | Test | Disposition | Reason |
  |---|---|---|
  | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` `HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment` | Rewrite: Left on a non-expanded row is a no-op | #440 Left walk removed (AC7) |
  | same file `HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior` | Rewrite: repeated Left posts no render and keeps `SelectedFolderPath` | #440 walk and Left segment hiding removed (AC7) |
  | same file `HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall` | Update to the AC9 shape; single-query assertion kept | `[+]` lists the segment's children (AC9, D-Q23) |
  | same file `HandleArrowKey_RightAfterExpansion_DescendsByChildActivation` | Update: Right selects the first descendant with its `nodeId` | AC5 selects and highlights the child |
  | same file `Boundary_EfcLeftAtRootAndRightOnChildlessNode_RemainSilentNoOps` | Update: every Left is a silent no-op; Right on a childless node unchanged | AC7, AC6 |
  | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` `LeafExpandToggle_OnCollapsedRow_ReExpandsWithoutProviderQuery` and `ArrowKeyRight_WhenCollapsed_ReExpandsWithoutProviderQuery` | Rewrite: one provider query lists the active segment's children; trailing segment stays hidden | `[+]` restore removed (AC9, D-Q23) |
  | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` `SegmentDoubleClick_ValidIndex_ViaHostEvent_CollapsesRowAndPostsRender` | Re-run; update only an exact post count | The atomic double-click arm commits the segment (S10) |
  | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` `LeftArrow_WhenFullyExpanded_HidesTrailingSegment`, `LeftArrow_AtRootSegment_IsNoOp` | Rewrite (fold into one): `LeftArrow` returns false and `CollapsedAfterIndex` stays null | Left segment hiding removed (AC7) |
  | same file `RightArrow_WhenCollapsed_RestoresFullBreadcrumb`, `ToggleLeafExpanded_WhileCollapsed_IsNoOp` | Update for the shortened-anchor expansion | `[+]` restore removed (AC9) |
  | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` `LeftArrow_QfcMultiSegmentRow_SelectsParentNode`, `RightArrow_QfcSelectedParentNode_ExpandsIntoChildren` | Rewrite: no-op; AC9 entry point | QFC #440 walks removed (AC7, AC9) |
  | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges`, `RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion`, `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses`, `LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled`, `LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot` | Rewrite each to AC7 and AC9; `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` keeps its assertions (under AC7 the collapse hides the selected subfolder, so the selection moves to the row, which is today's observable result) and only its summary is updated to cite #1007 AC7 | #440 Left walk and `[+]` restore removed |
  | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs` `BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession` | Update the summary to cite #1007; assertions unchanged (planner amendment, 2026-10-23) | On its single-segment row the model Right and Left keep their meaning; the session-aware decisions (AC4, AC5, AC7) live in the router Navigation partial and are covered by new tests |
  | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`, `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition`, `ArrowAsync_QfcRightOnSelectedParentNode_RoutesChildExpansion` | Rewrite bodies in place, no net growth | QFC #440 walk removed (AC7, AC9) |
  | `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` `UnhandledRightArrow_RaisesUnhandledArrowRight`, `UnhandledLeftArrow_RaisesUnhandledArrowLeft`, `ArrowMessages_RaiseSyntheticFolderKeyDown` | Update: closed Right opens the list, closed Left does nothing; pop-out expectation moves to an open-list precondition | AC10, AC6 |
  | `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` `ExistingLeftAndRightMessages_StillForwardOnce` | Update in place, no added lines | Closed Left no longer forwards (AC10) |
  | `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` `LeftAndRightBreadcrumbMessages_RemainSupported`, `RenderReceiver_OwnsSelectedChildExpandedAndCollapsedProjection`, `SelectorKeys_PreventBrowserScrollingAndPostNativeKeyMessages` | Update literals; extend the key set; ArrowLeft and ArrowRight negative assertions stay | `onArrow` pre-filter removed (S6); pending-node highlight (AC4); new keys (AC11) |
  | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs` key round-trip and unknown-key cases | Extend with the new keys; unknown-key cases keep a value that stays unknown | AC8, AC11 |
  | `tests/quickfiler-breadcrumb/issue-997-common.test.mjs` arrow routing cases and "clicking a selectable row uses its host identity" | Update the arrow cases (every case posts `arrowKey`); the row-click case is unchanged because the default fixture's only segment is the leaf; the non-leaf single-click case is added in `issue-1007-keys.test.mjs` | QFC single click on a non-leaf segment moves only the pending highlight (AC9, D-Q27); `onArrow` pre-filter removed (S6) |
  | same file "row cells retain literal names, title, truncation and host percentage" | Update the double-click payload expectation to `selectorSegmentDoubleClick` with `rowIdentity` | Segment double-click moves to the selector channel (S5) |
  | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` `ArrowKey_UnknownKey_IsLoggedNoOp` | Update the unknown key value from `Home` to a value that stays unknown | `Home` becomes a supported key (AC11) |
  | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` `SegmentDoubleClick_OnNonLeafSegment_CollapsesAndReRenders` | Update: select the last row-scoped render instead of asserting a single render payload; affordance and hidden-leaf assertions kept | The atomic double-click arm also commits the segment (S10) |
  | `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` `InboundDoubleClick_PostsCollapsedRenderJson` | Rewrite in place, no net growth: send `selectorSegmentDoubleClick` | The bridge-channel `segmentDoubleClick` arm becomes a logged no-op (S5) |
  | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` `Route_SegmentDoubleClick_ProducesCollapsedRenderPayload` | Rewrite in place, no net growth: the bridge `segmentDoubleClick` is a no-op with no state change | S5 |
  | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` `Sequence_ExpandCollapseViaMessages_TransitionsDeterministically` | Rewrite: shorten through the selector double-click entry point; the following toggle expands the active segment and keeps the row shortened | S5; `[+]` restore removed (AC9, D-Q23) |
  | `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` (the chain `HasChildren` assertions) | Re-run; unchanged when the fixture's child keys resolve | AC14 changes the derivation only for unresolvable child keys |

  Unaffected and re-run: `ArrowKeyRight_ThenLeft_ExpandsAndCollapses`, `ArrowKey_SingleSegmentRow_TakesPreExistingCollapsePath`, the Selection-file Up and Down row tests, `LeftArrow_WhenLeafExpanded_CollapsesLeafFirst`, `ArrowKey_QfcSingleSegmentRow_TakesPreExistingCollapsePath`, `Boundary_QfcUnhandledArrow_StillReachesBreadcrumbArrowFallThrough`, `DescendantToggle_DoesNotChangeSelection`, `DescendantActivate_ThenRowConfirmed_RaisesConfirmationOnce`, `Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView`.
- Unit tests for the fixed behaviour and boundaries: the decision table rows (expand, move to first child, collapse, leaf, in-flight, zero children), the archive-root stop for Ctrl+Left, the first and last entry boundaries for every movement key, and the AC7 nearest-visible-ancestor re-commit.
- Edge cases and negative scenarios: no selection in EFC (D-Q26); invalid `pageSize` (absent, zero, negative); unknown row id and node id; out-of-root commit rejected; Ctrl+Left at active index 0; Escape after Ctrl+Left restoring the row shape (D-Q24); Right on a node whose fetch is in flight; a stale completion after the row is rebound (I2).
- Error handling and logging verification: each logged no-op is asserted through state invariance (no post, no event, `SelectedFolderPath` unchanged) and, where a test sink exists, the logged message.
- Coverage impact and targets: line coverage at least 80 percent and branch coverage at least 75 percent on the testable denominator (CLAUDE.md UT2); every new module and new partial (`BreadcrumbVisibleOrder`, the Tree, Navigation and Keyboard partials) at least 90 percent; changed lines do not lose coverage. Per-file figures are read from the post-processed Cobertura document written by the coverage run and are recorded in the committed coverage-comparison evidence; the committed JaCoCo projection carries package-level counters only and cannot supply a per-file figure (planner amendment, 2026-10-23).
- Toolchain commands to run (format → lint → type-check → test), in the CLAUDE.md order, restarting from step 1 on any failure or auto-fix:
  1. `dotnet tool run csharpier format .` then verify with `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. The `test: MSTest with Coverage (Koverage)` VS Code task, or `Invoke-MSTestWithCoverage.ps1` under scripts/vscode.
  5. Locally, `node --test tests/quickfiler-breadcrumb/` for the node suites (no CI signal; recorded as evidence).
- Evidence (projections only, per CLAUDE.md "Committed Test Evidence Format"; no raw TRX or coverage XML is committed):
  - Baseline: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/baseline/`
  - Fail-before run: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/regression-testing/`
  - Final gates and coverage projection: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/qa-gates/`
- Manual validation steps (human step, performed by the maintainer in Outlook; not automatable):
  - EFC and QFC: arrow through expanded subfolders at several depths; Home, End, PageUp and PageDown against the real viewport.
  - EFC: Up on the first row moves focus to the search box; Home does not.
  - QFC: Right on a leaf with the list open; confirm the list closes before the "Pop Out Item or Enumerate Conversation?" dialog paints.
  - Both: segment double-click timing in WebView2; hold Right for auto-repeat on an unexpanded row and confirm it ends expanded.
  - Both: confirm no `[+]` appears on a folder without subfolders.

## Acceptance Criteria

The criteria below are copied from issue.md, maintainer-confirmed on 2026-10-23 (grill rounds 1 to 3, amended by round 4). Numbering, order and wording are preserved; AC14 follows AC11 as in the issue. Sub-bullets are non-authoritative test-name placeholders for the planner and do not change the criteria.

- [x] AC1 (Up/Down tree order): Down and Up visit every visible row in tree order. The children of a collapsed node are skipped. Up from the row below an expanded block lands on the block's last visible descendant. (Q1, Q2)
  - Placeholders: `Step_DownFromExpandedRow_VisitsDescendantsInPreOrder`, `Step_SkipsChildrenOfCollapsedNode`, `ArrowUp_FromRowBelowExpandedBlock_SelectsLastVisibleDescendant`, `SelectorKeyDown_Open_MovesPendingThroughVisibleDescendants`.
- [x] AC2 (No wrap): the ends do not wrap. In EFC, Up on the first row moves focus to the search box and Down on the last row does nothing. (Q5)
  - Placeholders: `ArrowDown_OnLastVisibleEntry_PostsNothing`, `ArrowUp_OnFirstEntry_RaisesFocusSearchRequested`, `SelectorKeyDown_Open_OnLastEntry_KeepsPending`.
- [x] AC3 (EFC selects on move): in EFC an arrow onto a row or subfolder selects it exactly as a mouse click does, and Enter files to it. (Q3)
  - Placeholders: `ArrowDown_OntoDescendant_CommitsArchiveRelativePathAndRaisesChanged`, `ArrowDown_OntoDescendant_ThenRowConfirmed_RaisesConfirmationForDescendant`.
- [x] AC4 (QFC open-list pending model): with the QFC folder list open, arrows move only the pending highlight. Enter commits, Escape restores the original choice, and Left on a highlighted item that is not expanded commits it and closes the list. (Q7, Q15)
  - Placeholders: `OpenMove_DoesNotChangeCommittedValue`, `CommitPending_WithPendingNode_SelectsDescendant`, `Cancel_RestoresOriginalAndClearsPendingNode`, `OpenLeft_OnNonExpandedEntry_CommitsAndCloses`.
- [x] AC5 (Right): Right on an item that is not expanded and has children expands it. Right on an expanded item moves to its first child (selected in EFC, pending in QFC). A shortened top-level row (`Archive > Clients [+]`) expands to list the segment's subfolders, as in AC9; it does not restore the trailing segments. (Q4, Q13, Q23)
  - Placeholders: `ArrowRight_OnUnexpandedRowWithChildren_Expands`, `ArrowRight_OnExpandedRow_SelectsFirstDescendantWithNodeId`, `ArrowRight_OnShortenedRow_ListsSegmentChildrenWithSingleQuery`, `ArrowRight_ReentrantDuringFetch_EndsExpandedWithOneQuery`.
- [x] AC6 (Right on a leaf): in EFC, Right on a folder with no subfolders does nothing. With the QFC list open, the list closes, the original choice is kept, and the "Pop Out Item or Enumerate Conversation?" dialog opens. (Q12, Q21)
  - Placeholders: `ArrowRight_OnLeaf_PostsNothing`, `OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight`.
- [x] AC7 (Left): Left on an expanded item collapses only that item, and the highlight stays on it. Left on a leaf, a collapsed subfolder or a non-expanded top-level row does nothing, except in the open QFC list, where AC4 applies. Left never selects a parent: the #440 parent-segment walk and the Left segment hiding are removed in both viewers. When a collapse hides the highlighted row, the highlight moves to its nearest visible ancestor. (Q9, Q10, Q11)
  - Placeholders: `ArrowLeft_OnExpandedNode_CollapsesOnlyThatNode`, `ArrowLeft_OnNonExpandedRow_IsNoOp`, `LeftArrow_NeverHidesTrailingSegment`, `CollapseHidingSelection_RecommitsNearestVisibleAncestor`.
- [x] AC8 (Ctrl+Left): Ctrl+Left selects the parent folder and stops one level below the archive root. On a top-level row it does what a double-click on the parent segment does (AC9). On a subfolder it moves to the parent subfolder or row and leaves that expanded. With the QFC list open it moves only the pending highlight; with the list closed it commits immediately. With the QFC list open, Escape after Ctrl+Left restores both the row's shape (`Archive > Clients > Beta`) and the committed value. Ctrl+Right behaves as Right. (Q19, Q20, Q24)
  - Placeholders: `ParentLeft_OnRow_ActivatesParentSegmentCommitsAndShortens`, `ParentLeft_AtFirstSegment_IsNoOp`, `ParentLeft_OnNode_SelectsParentKeepingExpansion`, `SelectorKeyParent_Open_ThenEscape_RestoresRowShapeAndValue`, `SelectorKeyParent_Closed_CommitsImmediately`.
- [x] AC9 (Segment double-click): in both viewers, a double-click on a path segment selects that folder and shortens the row (`Archive > Clients > Beta` becomes `Archive > Clients [+]`). In QFC it commits and closes the list. Expanding the `[+]` lists every subfolder of Clients instead of restoring Beta. The highlight stays on Clients, and the next Right moves to the first child. In the open QFC list, a single click on a non-leaf segment (`Archive` or `Clients`) moves only the pending highlight to that row and does not close the list; a click on the leaf segment (`Beta`) or the percentage still commits and closes. (Q13, Q18, Q27)
  - Placeholders: `SegmentDoubleClick_ActivatesCommitsAndShortensAtomically`, `LeafExpandToggle_OnShortenedRow_ListsSegmentChildren`, `SelectorSegmentDoubleClick_CommitsSegmentFolderAndCloses`, `GetSelectedFolder_ShortenedRow_ReturnsActiveSegmentValue`, node case for the non-bubbling non-leaf click.
- [x] AC10 (QFC closed list): with the QFC list closed, Right opens the list with the pending highlight on the committed choice: on the committed subfolder (for example Specs under `Archive > Projects > Alpha`) when it is visible, otherwise on its row. Left does nothing. Up and Down keep today's direct commit of the adjacent top-level row. Home and End commit the first and last top-level row. PageUp and PageDown do nothing. (Q14, Q16, Q22, Q25, Q29)
  - Placeholders: `ClosedRight_OpensSelectorWithPendingOnVisibleCommittedSubfolder`, `ClosedRight_SubfolderHidden_PendingOnRow`, `ClosedLeft_IsConsumedWithoutEffect`, `ClosedHomeEnd_CommitFirstAndLastTopLevelRow`, `ClosedPageKeys_AreNoOps`.
- [x] AC11 (Home/End/PageUp/PageDown): Home and End go to the first and last visible row in tree order, and Home does not move focus to the search box. PageUp and PageDown move by the number of rows fully visible and stop at the first and last row without wrapping. EFC selects as in AC3; with the QFC list open only the pending highlight moves. In EFC with nothing selected, Down and Home select the first row and Up and End select the last row. (Q8, Q16, Q17, Q26)
  - Placeholders: `Home_SelectsFirstEntryWithoutFocusSearch`, `End_SelectsLastVisibleDescendant`, `PageDown_MovesByPageSizeAndClamps`, `PageKey_InvalidPageSize_IsLoggedNoOp`, `NoSelection_DownAndHomeSelectFirst_UpAndEndSelectLast`, node case for `fullyVisibleCount`.
- [x] AC14 (No `[+]` without subfolders): an expand affordance `[+]` is never shown on a folder that has no subfolders, in either viewer, on top-level rows or on nested subfolders. A folder's has-children flag is derived from the children that resolve in the folder snapshot (the set `FolderTreeSnapshot.GetChildren` returns), not from the raw child-key count. A fetch that attaches zero children leaves no expand or collapse affordance. (Q28)
  - Placeholders: `MapNode_UnresolvableChildKey_HasChildrenFalse`, `GetImmediateSubfolders_ChildWithOnlyUnresolvableKeys_HasChildrenFalse`, `EfcRenderer_ZeroChildrenFetch_RendersNoAffordance`, `QfcProjection_ZeroChildrenFetch_EmitsNoPlusOrMinus`.
- [x] AC12 (Regression tests first): each behaviour above has a deterministic regression test (MSTest + Moq + FluentAssertions for C#, node tests under `tests/quickfiler-breadcrumb/` for JavaScript) that fails before the fix and passes after it. Existing tests that pin the removed #440 Left walk, the Left segment hiding and the `[+]` restore are updated with the reason recorded.
  - The fail-before run is the Markdown projection under the feature's regression-testing evidence folder (full path in Test Strategy); rewritten tests are those in the Test Strategy disposition table.
- [x] AC13 (Toolchain and coverage): the C# toolchain (CSharpier, analyzers with `/t:Rebuild`, `/p:TreatWarningsAsErrors=true`, `Invoke-MSTestWithCoverage.ps1`) and the JavaScript node tests pass in one clean pass. Line coverage is at least 80% and branch coverage at least 75%, new code reaches at least 90%, changed lines do not lose coverage, and no file exceeds 500 lines.
  - The coverage floors are read on the testable denominator (CLAUDE.md UT2); the 500-line ceiling applies to the production, test and script files in the change (Markdown and legacy project files are exempt under the repository rule).

## Risks & Mitigations

- Technical or operational risks:
  - **Size of the change.** The production footprint listed under Proposed Fix spans UtilitiesCS, QuickFiler and both page scripts, with new production partials or modules in both projects, and a comparable set of new and rewritten test files. Risks: an incomplete port between viewers, merge conflicts with other breadcrumb work, and files breaching the 500-line ceiling.
  - **Behaviour removal from #440.** Users who rely on Left to walk to the parent folder, or on the Left segment hiding, lose that gesture; the `[+]` restore of trailing segments and the QFC open-list single-click commit on a non-leaf segment also change. Rewritten tests could hide an unintended regression if the reason is not recorded.
  - **Narrowing of #1002 I3** (S1) could be read as a silent contract change.
  - **No CI signal for page scripts.** A `FolderBreadcrumb.js` or `BridgeJs` regression could pass CI.
  - **Pop-out ordering.** Whether the popup is visibly closed before the modal dialog paints depends on the popup operations queue and the modal message pump, which C# tests cannot observe.
  - **Re-entrancy and auto-repeat.** Held keys can interleave with provider fetches.
  - **Coverage of new partials** below 90 percent would fail AC13.
- Mitigations and rollbacks:
  - Size: one shared pure helper (S2) instead of two traversals; new partials for every file at or near the limit; the planner splits the work into phases by layer (helper, EFC, QFC, page scripts, AC14) with a toolchain checkpoint per phase; the large-path route (rule 12) applies.
  - #440 removal: maintainer-confirmed (D-Q9, D-Q23); Ctrl+Left replaces parent selection (D-Q19); every rewritten test's summary records "#1007" and the governing AC; the PR body lists the removed gestures.
  - I3: recorded as decision S1 with the test that stays valid.
  - Page scripts: every node assertion has a C# string-contract twin; the local node run is committed as evidence.
  - Pop-out ordering: the C# test asserts the transition is published before `UnhandledArrow`; the visual ordering is a manual verification item.
  - Re-entrancy: S7 with a `TaskCompletionSource` regression test.
  - Coverage: each decision-table row, boundary and failure branch gets a test; per-file figures are checked against the committed projection.
  - Rollback: revert the merge; no data or settings migration.

## Rollout & Follow-up

- Release/rollout steps: merge to `main` after the toolchain passes in one clean pass and the maintainer completes the manual Outlook verification; the add-in ships with the next normal build. No staged rollout or flag.
- Post-fix monitoring or clean-up tasks:
  - Watch the add-in debug log for the new rejected-key and invalid-`pageSize` entries during the first sessions.
  - Promote any defect found during manual verification that is unrelated to these files through the issue promotion lifecycle.
  - Adding the node suites to CI is a separate improvement and is not part of this item.
- Links: issue #1007 (https://github.com/drmoisan/TaskMaster/issues/1007); predecessor issues #1002 (PR #1003), #997, #440, #614; feature-folder inputs issue.md, grill-transcript.md and research/2026-10-23T10-40-arrow-key-subfolder-navigation-research.md.
